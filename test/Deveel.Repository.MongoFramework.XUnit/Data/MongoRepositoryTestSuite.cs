using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public abstract class MongoRepositoryTestSuite<TPerson> : RepositoryTestSuite<TPerson, MongoPersonRelationship> 
		where TPerson : MongoPerson {
		private MongoSingleDatabase mongo;

		protected MongoRepositoryTestSuite(MongoSingleDatabase mongo, ITestOutputHelper? testOutput) : base(testOutput) {
			this.mongo = mongo;

			ConnectionString = mongo.SetDatabase(DatabaseName);
		}

		protected string ConnectionString { get; }

		protected string DatabaseName => "test_db";

		protected override string GeneratePersonId() => ObjectId.GenerateNewId().ToString();

		protected override Faker<MongoPersonRelationship> RelationshipFaker => new MongoPersonRelationshipFaker();

		protected IMongoCollection<TPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase(DatabaseName)
			.GetCollection<TPerson>("persons");

		protected override Task AddRelationshipAsync(TPerson person, MongoPersonRelationship relationship) {
			if (person.Relationships == null)
				person.Relationships = new List<MongoPersonRelationship>();

			person.Relationships.Add(relationship);

			return Task.CompletedTask;
		}

		protected override Task RemoveRelationshipAsync(TPerson person, MongoPersonRelationship relationship) {
			if (person.Relationships != null)
				person.Relationships.Remove(relationship);

			return Task.CompletedTask;
		}

		protected override async Task InitializeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.CreateRepositoryAsync<MongoPerson>();

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.DropRepositoryAsync<MongoPerson>();

			await base.DisposeAsync();
		}

		[Fact]
		public async Task FindByObjectId() {
			var person = await RandomPersonAsync();

			var found = await Repository.FindByKeyAsync(person.Id);

			Assert.NotNull(found);
			Assert.Equal(person.Id, found.Id);
		}

		[Fact]
		public async Task FindByObjectId_NotExisting() {
			var personId = ObjectId.GenerateNewId();

			var found = await Repository.FindByKeyAsync(personId.ToString());

			Assert.Null(found);
		}
	}
}
