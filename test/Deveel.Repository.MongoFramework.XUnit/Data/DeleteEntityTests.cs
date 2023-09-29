using System;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
	public sealed class DeleteEntityTests : MongoFrameworkRepositoryTestBase {
		private readonly IList<MongoPerson> people;

		public DeleteEntityTests(MongoFrameworkTestFixture mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(MongoRepository<MongoDbContext, MongoPerson> repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Mongo_DeleteExisting() {
			var entity = people[^1];

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteExisting() {
			var entity = people[^1];

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteExisting() {
			var entity = people[^1];

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.True(result);
		}


		[Fact]
		public async Task Mongo_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteNotExisting() {
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId() };

			var result = await FacadeRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await MongoRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await Repository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_DeleteById_Existing() {
			var id = people[56].Id;

			var result = await FacadeRepository.DeleteByIdAsync(id.ToEntityId());

			Assert.True(result);
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
