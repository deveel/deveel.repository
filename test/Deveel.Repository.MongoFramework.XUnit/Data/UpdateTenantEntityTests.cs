using System;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
	public class UpdateTenantEntityTests : MongoRepositoryProviderTestBase {
		private readonly IList<MongoPerson> people;

		public UpdateTenantEntityTests(MongoFrameworkTestFixture mongo) : base(mongo) {
			people = GeneratePersons(100);
		}

		protected override async Task SeedAsync(MongoRepository<MongoDbTenantContext, MongoPerson> repository) {
			await repository.CreateAsync(people);
		}

		[Fact]
		public async Task Mongo_UpdateExisting() {
			var entity = people[^1];

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await MongoRepository.UpdateAsync(entity);

			Assert.True(result);
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
			var entity = people[^1];

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateExisting() {
			var entity = people[^1];

			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task Mongo_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = ObjectId.GenerateNewId();

			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await MongoRepository.UpdateAsync(person);

			Assert.False(result);
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
		public async Task FacadeRepository_UpdateNotExisting() {
			var person = GeneratePerson();
			person.Id = ObjectId.GenerateNewId();

			person.BirthDate = new DateTime(1980, 06, 04);

			var result = await FacadeRepository.UpdateAsync(person);

			Assert.False(result);
		}
	}
}
