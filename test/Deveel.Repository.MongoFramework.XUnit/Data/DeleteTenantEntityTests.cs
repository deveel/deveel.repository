using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

using SharpCompress.Common;

namespace Deveel.Data {
	public sealed class DeleteTenantEntityTests : MongoRepositoryProviderTestBase {
		private readonly IList<MongoPerson> people;

		public DeleteTenantEntityTests(MongoFrameworkTestFixture mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override Task SeedAsync(IRepository<MongoPerson> repository) {
			return repository.AddRangeAsync(people);
		}

		private MongoPerson NextRandom() => people[Random.Shared.Next(0, people.Count - 1)];

		[Fact]
		public async Task Mongo_DeleteExisting() {
			var entity = NextRandom();

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.True(result);

			var found = await FindPerson(entity.Id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var person = NextRandom();

			var entity = await Repository.FindByIdAsync(person.Id.ToEntityId());
			Assert.NotNull(entity);

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);

			var found = await FindPerson(person.Id);
			Assert.Null(found);

			var repo = RepositoryProvider.GetRepository(TenantId);
			var found2 = await repo.FindByIdAsync(person.Id.ToEntityId());

			Assert.Null(found2);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var person = NextRandom();

			var entity = await FacadeRepository.FindByIdAsync(person.Id.ToEntityId());
			Assert.NotNull(entity);

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.True(result);


			var found = await FindPerson(ObjectId.Parse(entity.Id));
			Assert.Null(found);

		}


		[Fact]
		public async Task Mongo_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.False(result);

		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_Existing() {
			var id = NextRandom().Id;

			var result = await MongoRepository.DeleteByIdAsync(id.ToEntityId());
			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = NextRandom().Id;

			var result = await Repository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = NextRandom().Id;

			var result = await FacadeRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);

			var found = await FindPerson(id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Mongo_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await MongoRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await FacadeRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.False(result);
		}
	}
}
