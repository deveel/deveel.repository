using System;

using MongoDB.Bson;

namespace Deveel.Data {
	public class UpdateEntityTests : MongoFrameworkRepositoryTestBase {
		private readonly IList<MongoPerson> people;

		public UpdateEntityTests(MongoFrameworkTestFixture mongo) 
			: base(mongo) {
			var nameGen = new RandomNameGenerator();

			people = Enumerable.Range(1, 100)
				.Select(_ => nameGen.NewName())
				.Select(x => new MongoPerson { 
					FirstName = x.Item1, 
					LastName = x.Item2, 
					Version = Guid.NewGuid().ToString()
				})
				.ToList();
		}

		protected override async Task SeedAsync(MongoRepository<MongoPerson> repository) {
			await repository.CreateAsync(people);
		}

		[Fact]
		public async Task Mongo_UpdateExisting() {
			var entity = people[^1];

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await MongoRepository.UpdateAsync(entity);

			Assert.True(result);
		}

		//[Fact]
		//public async Task Mongo_UpdateExisting_TransactionCommit() {
		//	var entity = people[^1];

		//	entity.BirthDate = new DateOnly(1980, 06, 04);

		//	using var transaction = (MongoTransaction) await TransactionFactory.CreateTransactionAsync();
		//	await transaction.BeginAsync();

		//	var result = await MongoRepository.UpdateAsync(transaction, entity);

		//	await transaction.CommitAsync();

		//	Assert.True(result);
		//}

		//[Fact]
		//public async Task Mongo_UpdateExisting_TransactionRollback() {
		//	var entity = people[^1];

		//	entity.BirthDate = new DateOnly(1980, 06, 04);

		//	using var transaction = (MongoTransaction) await TransactionFactory.CreateTransactionAsync();
		//	await transaction.BeginAsync();

		//	var result = await MongoRepository.UpdateAsync(transaction, entity);

		//	await transaction.RollbackAsync();

		//	Assert.True(result);
		//}


		[Fact]
		public async Task Repository_UpdateExisting() {
			var entity = people[^1];

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateExisting() {
			var entity = people[^1];

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_UpdateNotExisting() {
			var name = new RandomNameGenerator().NewName();
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), FirstName = name.Item1, LastName = name.Item2 };

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await MongoRepository.UpdateAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_UpdateNotExisting() {
			var name = new RandomNameGenerator().NewName();
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), FirstName = name.Item1, LastName = name.Item2 };

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateNotExisting() {
			var name = new RandomNameGenerator().NewName();
			var entity = new MongoPerson { Id = ObjectId.GenerateNewId(), FirstName = name.Item1, LastName = name.Item2 };

			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Mongo_UpdateWrongVersion() {
			var entity = people[^1];

			entity.Version = Guid.NewGuid().ToString();
			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await MongoRepository.UpdateAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task Repository_UpdateWrongVersion() {
			var entity = people[^1];

			entity.Version = Guid.NewGuid().ToString();
			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.False(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateWrongVersion() {
			var entity = people[^1];

			entity.Version = Guid.NewGuid().ToString();
			entity.BirthDate = new DateOnly(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(entity);

			Assert.False(result);
		}
	}
}
