using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoRepositoryTests : IAsyncLifetime {
		private MongoSingleDatabase mongo;

		public MongoRepositoryTests(MongoSingleDatabase mongo) {
			this.mongo = mongo;

			People = GeneratePersons(100);

			var services = new ServiceCollection();
			ConfigureServices(services);

			Services = services.BuildServiceProvider();
			Scope = Services.CreateScope();
		}

		protected string ConnectionString => mongo.ConnectionString;

		protected IMongoCollection<MongoPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase("testdb")
			.GetCollection<MongoPerson>("persons");


		protected IServiceProvider Services { get; }

		protected IServiceScope Scope { get; }

		protected IList<MongoPerson> People { get; }

		protected IRepository<MongoPerson> Repository => Scope.ServiceProvider.GetRequiredService<IRepository<MongoPerson>>();

		protected IDataTransactionFactory TransactionFactory => Scope.ServiceProvider.GetRequiredService<IDataTransactionFactory>();

		protected Faker<MongoPerson> PersonFaker { get; } = new MongoPersonFaker();

		protected ISystemTime SystemTime => Scope.ServiceProvider.GetRequiredService<ISystemTime>();

		protected MongoPerson GeneratePerson() => PersonFaker.Generate();

		protected IList<MongoPerson> GeneratePersons(int count) => PersonFaker.Generate(count);

		protected virtual void ConfigureServices(IServiceCollection services) {
			services.AddSystemTime(new TestTime());

			AddRepository(services);
		}

		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddMongoContext(builder => { 
					builder.UseConnection(mongo.SetDatabase("testdb"));
					AddRepository(builder);
				})
				.AddRepositoryController();
		}

		protected virtual void AddRepository(MongoDbContextBuilder<MongoDbContext> builder) {
            builder.AddRepository<MongoPerson>();
        }

		protected virtual async Task SeedAsync(IRepository<MongoPerson> repository) {
			// TODO: use the low-level MongoFramework API to seed the database
			await repository.AddRangeAsync(People);
		}

		protected async Task<MongoPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}


		public virtual async Task InitializeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.CreateRepositoryAsync<MongoPerson>();

			await SeedAsync(Repository);
		}

		public virtual async Task DisposeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.DropRepositoryAsync<MongoPerson>();

			Scope.Dispose();
			(Services as IDisposable)?.Dispose();
		}

		[Fact]
		public async Task AddNewPerson() {
			var person = GeneratePerson();

			var id = await Repository.AddAsync(person);

			Assert.NotNull(id);
			Assert.NotEmpty(id);
		}

		[Fact]
		public async Task AddRangeOfPersons() {
			var persons = Enumerable.Range(0, 100)
				.Select(x => GeneratePerson())
				.ToList();

			var results = await Repository.AddRangeAsync(persons);

			Assert.NotNull(results);
			Assert.NotEmpty(results);
			Assert.Equal(persons.Count, results.Count);

			for (int i = 0; i < results.Count; i++) {
				Assert.Equal(persons[i].Id, ObjectId.Parse(results[i]));
			}
		}

		[Fact]
		public async Task CountAll() {
			var result = await Repository.CountAllAsync();

			Assert.NotEqual(0, result);
			Assert.Equal(People.Count, result);
		}

		[Fact]
		public async Task CountFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var count = await Repository.CountAsync(p => p.FirstName == firstName);

			Assert.Equal(peopleCount, count);
		}


		[Fact]
		public async Task FindById() {
			var id = People.Random()!.Id;

			var result = await Repository.FindByIdAsync(id.ToEntityId());

			Assert.NotNull(result);
			Assert.Equal(id, result.Id);
		}

		[Fact]
		public async Task FindFirstFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.FindAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.Equal(firstName, result.FirstName);
		}

		[Fact]
		public async Task ExistsFiltered() {
			var firstName = People.Random()!.FirstName;

			var result = await Repository.ExistsAsync(x => x.FirstName == firstName);

			Assert.True(result);
		}

		[Fact]
		public async Task FindFirst() {
			var result = await Repository.FindAsync();

			Assert.NotNull(result);
			Assert.Equal(People[0].FirstName, result.FirstName);
		}

		[Fact]
		public async Task FindAll() {
			var result = await Repository.FindAllAsync();

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(People.Count, result.Count);
		}

		[Fact]
		public async Task FindAll_MongoQueryFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(new MongoQueryFilter<MongoPerson>(Builders<MongoPerson>.Filter.Where(x => x.FirstName == firstName)));

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task FindAllFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(x => x.FirstName == firstName);

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task GetPage() {
			var request = new RepositoryPageRequest<MongoPerson>(1, 10);

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());
		}


		[Fact]
		public async Task GetFilteredPage() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count());
		}

		[Fact]
		public async Task GetMultiFilteredPage() {
			var person = People.Random(x => x.LastName != null)!;
			var firstName = person.FirstName;
			var lastName = person.LastName;

			var peopleCount = People.Count(x => x.FirstName == firstName && x.LastName == lastName);
			var totalPages = (int)Math.Ceiling((double)peopleCount / 10);
			var perPage = Math.Min(peopleCount, 10);

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.Where(x => x.FirstName == firstName)
				.Where(x => x.LastName == lastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(peopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(perPage, result.Items.Count);
		}


		[Fact]
		public async Task GetDescendingSortedPage() {
			var sorted = People.Skip(0).Take(10).OrderByDescending(x => x.LastName).ToList();
			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.OrderByDescending(x => x.LastName);

			var result = await Repository.GetPageAsync(request);
			Assert.NotNull(result);
			Assert.Equal(10, result.TotalPages);
			Assert.Equal(100, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count());

			for (int i = 0; i < sorted.Count; i++) {
				Assert.Equal(sorted[i].LastName, result.Items.ElementAt(i).LastName);
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
		public async Task UpdateExisting() {
			var person = People.Random()!;

			var entity = await Repository.FindByIdAsync(person.Id.ToString());

			Assert.NotNull(entity);
			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = ObjectId.GenerateNewId();

			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(person);

			Assert.False(result);
		}

		//[Fact]
		//public async Task Mongo_UpdateWrongVersion() {
		//	var entity = people[^1];

		//	entity.Version = Guid.NewGuid().ToString();
		//	entity.BirthDate = new DateTime(1980, 06, 04);

		//	var result = await MongoRepository.UpdateAsync(entity);

		//	Assert.False(result);
		//}

		//[Fact]
		//public async Task Repository_UpdateWrongVersion() {
		//	var entity = people[^1];

		//	entity.Version = Guid.NewGuid().ToString();
		//	entity.BirthDate = new DateTime(1980, 06, 04);

		//	var result = await Repository.UpdateAsync(entity);

		//	Assert.False(result);
		//}

		//[Fact]
		//public async Task FacadeRepository_UpdateWrongVersion() {
		//	var entity = people[^1];

		//	entity.Version = Guid.NewGuid().ToString();
		//	entity.BirthDate = new DateTime(1980, 06, 04);

		//	var result = await FacadeRepository.UpdateAsync(entity);

		//	Assert.False(result);
		//}

		[Fact]
		public async Task RemoveExisting() {
			var entity = People.Random()!;

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveNotExisting() {
			var entity = new MongoPersonFaker()
				.RuleFor(x => x.Id, f => ObjectId.GenerateNewId())
				.Generate();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task RemoveById_Existing() {
			var id = People.Random()!.Id;

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task RemoveById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}
	}
}
