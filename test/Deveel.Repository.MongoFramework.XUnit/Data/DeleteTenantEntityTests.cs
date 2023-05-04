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
			return repository.CreateAsync(people);
		}

		private MongoPerson NextRandom() => people[Random.Shared.Next(0, people.Count - 1)];

		[Fact]
		public async Task Mongo_DeleteExisting() {
			var entity = NextRandom();

			var result = await MongoRepository.DeleteAsync(entity);

			Assert.True(result);

			var found = await FindPerson(entity.Id);
			Assert.Null(found);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = NextRandom();

			var result = await Repository.DeleteAsync(entity);

			Assert.True(result);

			var found = await FindPerson(entity.Id);
			Assert.Null(found);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = NextRandom();

			var result = await FacadeRepository.DeleteAsync(entity);

			Assert.True(result);


			var found = await FindPerson(entity.Id);
			Assert.Null(found);

		}


		[Fact]
		public async Task Mongo_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await MongoRepository.DeleteAsync(entity);

			Assert.False(result);

		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await Repository.DeleteAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), TenantId = TenantId };

			var result = await FacadeRepository.DeleteAsync(entity);

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
