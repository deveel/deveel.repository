using System;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data {
	public class UpdateEntityTests : MongoFrameworkRepositoryTestBase {
		private readonly IList<MongoPerson> people;
		private readonly ISystemTime testTime = new TestTime();

		public UpdateEntityTests(MongoSingleDatabase mongo) 
			: base(mongo) {
			people = GeneratePersons(100);
		}

		protected override void AddRepository(IServiceCollection services) {
			services.AddSystemTime(testTime);

			base.AddRepository(services);
		}

		protected override async Task SeedAsync(MongoRepository<MongoDbContext, MongoPerson> repository) {
			await repository.AddRangeAsync(people);
		}

		[Fact]
		public async Task Mongo_UpdateExisting() {
			var entity = people.Random()!;

			entity.BirthDate = new DateTime(1980, 06, 04).ToUniversalTime();

			var result = await MongoRepository.UpdateAsync(entity);

			Assert.True(result);

			var updated = await FindPerson(entity.Id);

			Assert.NotNull(updated);
			Assert.NotNull(entity.BirthDate);
			Assert.NotNull(updated.BirthDate);
			Assert.Equal(entity.BirthDate.Value, updated.BirthDate.Value);

			Assert.Equal(entity.FirstName, updated.FirstName);
			Assert.Equal(entity.LastName, updated.LastName);
			Assert.NotNull(updated.UpdatedAtUtc);
			Assert.Equal(testTime.UtcNow, updated.UpdatedAtUtc.Value);
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
			var person = people.Random()!;

			var entity = await Repository.FindByIdAsync(person.Id.ToString());

			Assert.NotNull(entity);
			entity.BirthDate = new DateTime(1980, 06, 04);

			var result = await Repository.UpdateAsync(entity);

			Assert.True(result);
		}

		[Fact]
		public async Task FacadeRepository_UpdateExisting() {
			var person = people.Random()!;

			var entity = await FacadeRepository.FindByIdAsync(person.Id.ToString());

			Assert.NotNull(entity);

			((MongoPerson) entity).BirthDate = new DateTime(1980, 06, 04);

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
	}
}
