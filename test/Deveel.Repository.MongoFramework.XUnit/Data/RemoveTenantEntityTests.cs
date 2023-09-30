using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

using SharpCompress.Common;

namespace Deveel.Data {
	public sealed class RemoveTenantEntityTests : MongoRepositoryProviderTestBase {
		private readonly IList<MongoTenantPerson> people;

		public RemoveTenantEntityTests(MongoSingleDatabase mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override Task SeedAsync(IRepository<MongoTenantPerson> repository) {
			return repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Mongo_RemoveExisting() {
			var entity = people.Random()!;

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.True(result);

			var found = await FindPerson(entity.Id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_RemoveExisting() {
			var person = people.Random()!;

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
		public async Task FacadeRepository_RemoveExisting() {
			var person = people.Random()!;

			var entity = await FacadeRepository.FindByIdAsync(person.Id.ToEntityId());
			Assert.NotNull(entity);

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.True(result);


			var found = await FindPerson(ObjectId.Parse(entity.Id));
			Assert.Null(found);

		}


		[Fact]
		public async Task Mongo_RemoveNotExisting() {
			var entity = new MongoTenantPersonFaker(TenantId)
				.RuleFor(x => x.Id, ObjectId.GenerateNewId())
				.Generate();

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.False(result);

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
		public async Task FacadeRepository_RemoveNotExisting() {
			var entity = new MongoTenantPersonFaker(TenantId)
				.RuleFor(x => x.Id, ObjectId.GenerateNewId())
				.Generate();

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_RemoveById_Existing() {
			var id = people.Random()!.Id;

			var result = await MongoRepository.RemoveByIdAsync(id.ToEntityId());
			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_RemoveById_Existing() {
			var id = people.Random()!.Id;

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task FacadeRepository_RemoveById_Existing() {
			var id = people.Random()!.Id;

			var result = await FacadeRepository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Mongo_RemoveById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await MongoRepository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_RemoveById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_RemoveById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await FacadeRepository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}
	}
}
