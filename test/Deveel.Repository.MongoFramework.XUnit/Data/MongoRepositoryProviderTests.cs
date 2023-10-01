using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoRepositoryProviderTests : IAsyncLifetime {
		private MongoSingleDatabase mongo;
		private readonly IServiceProvider serviceProvider;

		public MongoRepositoryProviderTests(MongoSingleDatabase mongo) {
			this.mongo = mongo;
			PersonFaker = new MongoTenantPersonFaker(TenantId);

			People = GeneratePersons(100);

			var services = new ServiceCollection();
			ConfigureServices(services);

			serviceProvider = services.BuildServiceProvider();
		}

		protected virtual string DatabaseName { get; } = "test_db";

		protected IMongoCollection<MongoTenantPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase(DatabaseName)
			.GetCollection<MongoTenantPerson>("persons");

		protected async Task<MongoTenantPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}

		protected Faker<MongoTenantPerson> PersonFaker { get; }

		protected string TenantId { get; } = Guid.NewGuid().ToString("N");

		protected IList<MongoTenantPerson> People { get; }

		protected string ConnectionString => mongo.ConnectionString;

		protected IRepositoryProvider<MongoTenantPerson> RepositoryProvider => 
			serviceProvider.GetRequiredService<IRepositoryProvider<MongoTenantPerson>>();

		protected IRepository<MongoTenantPerson> Repository => RepositoryProvider.GetRepositoryAsync(TenantId).ConfigureAwait(false).GetAwaiter().GetResult();

		protected IDataTransactionFactory TransactionFactory => serviceProvider.GetRequiredService<IDataTransactionFactory>();

		protected ISystemTime Time { get; } = new TestTime();

		protected MongoTenantPerson GeneratePerson() => PersonFaker.Generate();

		protected IList<MongoTenantPerson> GeneratePersons(int count)
			=> PersonFaker.Generate(count);

		protected virtual void ConfigureServices(IServiceCollection services) {
			services.AddSystemTime(Time);

			AddRepositoryProvider(services);
		}

		protected virtual void AddRepositoryProvider(IServiceCollection services) {
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(config => {
					config.Tenants.Add(new TenantInfo {
						Id = TenantId,
						Identifier = "test",
						Name = "Test Tenant",
						ConnectionString = mongo.SetDatabase(DatabaseName)
					}) ;
				});

			AddMongoDbContext(services);
			
			services.AddRepositoryController();
		}

		protected virtual void AddMongoDbContext(IServiceCollection services) {
			var builder = services.AddMongoTenantContext();
			AddRepository(builder);
		}

		protected virtual void AddRepository<TContext>(MongoDbContextBuilder<TContext> builder)
			where TContext : class, IMongoDbContext {
            builder.UseTenantConnection();
			builder.AddRepository<MongoTenantPerson>()
				.WithDefaultTenantProvider();
        }

        public virtual async Task InitializeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.CreateTenantRepositoryAsync<MongoTenantPerson>(TenantId);

			var repository = await RepositoryProvider.GetRepositoryAsync(TenantId);
			
			await SeedAsync(repository);
		}

		public virtual async Task DisposeAsync() {
			var controller = serviceProvider.GetRequiredService<IRepositoryController>();
			await controller.DropTenantRepositoryAsync<MongoTenantPerson>(TenantId);

			(serviceProvider as IDisposable)?.Dispose();
		}

		protected virtual async Task SeedAsync(IRepository<MongoTenantPerson> repository) {
			// TODO: use the low-level MongoFramework API to seed the database
			await repository.AddRangeAsync(People);
		}

		[Fact]
		public async Task Repository_AddNewPerson() {
			var person = GeneratePerson();
			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);

			var created = await FindPerson(ObjectId.Parse(id));

			Assert.NotNull(created);
			Assert.Equal(person.FirstName, created.FirstName);
			Assert.Equal(person.LastName, created.LastName);
			Assert.NotNull(person.BirthDate);
			Assert.NotNull(created.BirthDate);
			Assert.Equal(person.BirthDate.Value.Date, created.BirthDate.Value.Date);
			Assert.NotNull(created.CreatedAtUtc);
			Assert.Equal(Time.UtcNow.UtcDateTime, created.CreatedAtUtc.Value.UtcDateTime);
		}

		[Fact]
		public async Task Repository_AddNewPersons() {
			var persons = GeneratePersons(100);

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

		//[Fact]
		//public async Task Mongo_UpdateExisting_TransactionCommit() {
		//	var entity = people[^1];

		//	entity.BirthDate = new DateOnly(1980, 06, 04);

		//	using var transaction = (MongoTransaction)await TransactionFactory.CreateTransactionAsync();
		//	await transaction.BeginAsync();

		//	var result = await MongoRepository.UpdateAsync(transaction, entity);

		//	await transaction.CommitAsync();

		//	Assert.True(result);
		//}

		//[Fact]
		//public async Task Mongo_UpdateExisting_TransactionRollback() {
		//	var entity = people[^1];

		//	entity.BirthDate = new DateOnly(1980, 06, 04);

		//	using var transaction = (MongoTransaction)await TransactionFactory.CreateTransactionAsync();
		//	await transaction.BeginAsync();

		//	var result = await MongoRepository.UpdateAsync(transaction, entity);

		//	await transaction.RollbackAsync();

		//	Assert.True(result);
		//}


		[Fact]
		public async Task Repository_UpdateExisting() {
			var person = People.Random()!;

			var entity = await Repository.FindByIdAsync(person.Id.ToEntityId());

			Assert.NotNull(entity);

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);

			var found = await FindPerson(person.Id);
			Assert.NotNull(found);
			Assert.NotNull(found.BirthDate);
			Assert.Equal(entity.BirthDate.Value.ToUniversalTime(), found.BirthDate.Value.ToUniversalTime());
		}

		[Fact]
		public async Task Repository_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = ObjectId.GenerateNewId();

			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_RemoveExisting() {
			var person = People.Random()!;

			var entity = await Repository.FindByIdAsync(person.Id.ToEntityId());
			Assert.NotNull(entity);

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);

			var found = await FindPerson(person.Id);
			Assert.Null(found);

			var repo = await RepositoryProvider.GetRepositoryAsync(TenantId);
			var found2 = await repo.FindByIdAsync(person.Id.ToEntityId());

			Assert.Null(found2);
		}

		[Fact]
		public async Task Repository_RemoveNotExisting() {
			var entity = new MongoTenantPersonFaker(TenantId)
				.RuleFor(x => x.Id, ObjectId.GenerateNewId())
				.Generate();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_RemoveById_Existing() {
			var id = People.Random()!.Id;

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_RemoveById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_CountAll() {
			var result = await Repository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(People.Count, result);
		}

		[Fact]
		public async Task Repository_CountFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}

		[Fact]
		public async Task Repository_FindById() {
			var id = People.Random()!.Id;

			var result = await Repository.FindByIdAsync(id.ToEntityId());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task Repository_FindFirstFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_ExistsFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_FindFirst() {
			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(People[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task Repository_FindAll() {
			var result = await Repository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(People.Count, result.Count);
		}

		[Fact]
		public async Task Repository_FindAllFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task Repository_GetPage() {
			var request = new RepositoryPageRequest<MongoTenantPerson>(1, 10);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}

		[Fact]
		public async Task Repository_GetFilteredPage() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoTenantPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		// TODO:
		//[Fact]
		//public async Task FacadeRepository_GetFilteredPage() {
		//	var firstName = people[people.Count - 1].FirstName;
		//	var peopleCount = people.Count(x => x.FirstName == firstName);
		//	var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
		//	var perPage = Math.Min(peopleCount, 10);

		//	var request = new PageRequest<IPerson>(1, 10) {
		//		Filter = x => x.FirstName == firstName
		//	};

		//	var result = await FacadeRepository.GetPageAsync(request);
		//	Assert.NotNull(result);
		//	Assert.Equal(totalPages, result.TotalPages);
		//	Assert.Equal(peopleCount, result.TotalItems);
		//	Assert.NotNull(result.Items);
		//	Assert.NotEmpty(result.Items);
		//	Assert.Equal(perPage, result.Items.Count());
		//}
	}
}
