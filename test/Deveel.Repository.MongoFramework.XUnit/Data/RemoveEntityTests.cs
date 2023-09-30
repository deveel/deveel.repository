using System;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
	public sealed class RemoveEntityTests : MongoFrameworkRepositoryTestBase {
		private readonly IList<MongoPerson> people;

		public RemoveEntityTests(MongoSingleDatabase mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(MongoRepository<MongoDbContext, MongoPerson> repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Mongo_RemoveExisting() {
			var entity = people.Random()!;

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_RemoveExisting() {
			var entity = people.Random()!;

			var result = await Repository.RemoveAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_RemoveNotExisting() {
			var entity = new MongoPersonFaker()
				.RuleFor(x => x.Id, f => ObjectId.GenerateNewId())
				.Generate();

			var result = await MongoRepository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_RemoveNotExisting() {
			var entity = new MongoPersonFaker()
				.RuleFor(x => x.Id, f => ObjectId.GenerateNewId())
				.Generate();

			var result = await Repository.RemoveAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_RemoveById_Existing() {
			var id = people.Random()!.Id;

			var result = await MongoRepository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task Repository_DeleteById_Existing() {
			var id = people.Random()!.Id;

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await MongoRepository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_DeleteById_NotExisting() {
			var id = ObjectId.GenerateNewId();

			var result = await Repository.RemoveByIdAsync(id.ToEntityId());

			Assert.False(result);
		}
	}
}
