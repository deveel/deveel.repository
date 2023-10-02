using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoRepositoryTests : MongoRepositoryTestSuite<MongoPerson> {

		public MongoRepositoryTests(MongoSingleDatabase mongo, ITestOutputHelper outputHelper) : base(mongo, outputHelper) {
		}

		protected override Faker<MongoPerson> PersonFaker { get; } = new MongoPersonFaker();

		protected override void ConfigureServices(IServiceCollection services) {
			AddRepository(services);

			base.ConfigureServices(services);
		}

		protected virtual void AddRepository(IServiceCollection services) {
			services
				.AddMongoContext(builder => { 
					builder.UseConnection(ConnectionString);
					AddRepository(builder);
				})
				.AddRepositoryController();
		}

		protected virtual void AddRepository(MongoDbContextBuilder<MongoDbContext> builder) {
            // builder.AddRepository<MongoPerson>();
			builder.Services.AddRepository<MongoRepository<MongoDbContext, MongoPerson>>();
        }

		protected async Task<MongoPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}


		[Fact]
		public async Task FindAll_MongoQueryFiltered() {
			var firstName = People.Random()!.FirstName;
			var peopleCount = People.Count(x => x.FirstName == firstName);

			var result = await Repository.FindAllAsync(new MongoQueryFilter<MongoPerson>(Builders<MongoPerson>.Filter.Where(x => x.FirstName == firstName)));

			Assert.NotNull(result);
			Assert.NotEmpty(result);
			Assert.Equal(peopleCount, result.Count);
		}

		[Fact]
		public async Task GetFilteredPage_FieldNameOrdered() {
			var totalPages = (int)Math.Ceiling((double)People.Count / 10);
			var sorted = People.OrderBy(x => x.FirstName).Skip(0).Take(10).ToList();

			var request = new RepositoryPageRequest<MongoPerson>(1, 10)
				.OrderBy("FirstName");

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(People.Count, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);

			for (int i = 0; i < sorted.Count; i++) {
				Assert.Equal(sorted[i].FirstName, result.Items.ElementAt(i).FirstName);
			}
		}
	}
}
