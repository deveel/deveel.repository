using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public abstract class MongoRepositoryTestSuite<TPerson> : RepositoryTestSuite<TPerson> where TPerson : MongoPerson {
		private MongoSingleDatabase mongo;

		protected MongoRepositoryTestSuite(MongoSingleDatabase mongo, ITestOutputHelper? testOutput) : base(testOutput) {
			this.mongo = mongo;

			ConnectionString = mongo.SetDatabase(DatabaseName);
		}

		protected string ConnectionString { get; }

		protected string DatabaseName => "test_db";

		protected override string GeneratePersonId() => ObjectId.GenerateNewId().ToString();

		protected IMongoCollection<TPerson> MongoCollection => new MongoClient(mongo.ConnectionString)
			.GetDatabase(DatabaseName)
			.GetCollection<TPerson>("persons");

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
