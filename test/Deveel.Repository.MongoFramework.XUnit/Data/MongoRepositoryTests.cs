using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

using Xunit.Abstractions;

namespace Deveel.Data {
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
				.AddMongoDbContext<MongoDbContext>(builder => { 
					builder.UseConnection(ConnectionString);
				})
				.AddRepositoryController();

			services.AddRepository<MongoRepository<MongoPerson, ObjectId>>();
		}

		protected async Task<MongoPerson?> FindPerson(ObjectId id) {
			var collection = MongoCollection;
			var result = await collection.FindAsync(x => x.Id == id);

			return await result.FirstOrDefaultAsync();
		}

		protected override async Task DisposeAsync() {
			var result = await MongoCollection.DeleteManyAsync(x => true);

			await base.DisposeAsync();
		}

		[Fact]
		public async Task GetFilteredPage_FieldNameOrdered() {
			var totalPages = (int)Math.Ceiling((double)PeopleCount / 10);
			var sorted = People!.OrderBy(x => x.FirstName).Skip(0).Take(10).ToList();

			var request = new PageQuery<MongoPerson>(1, 10)
				.OrderBy("FirstName");

			var result = await Repository.GetPageAsync(request);

			Assert.NotNull(result);
			Assert.Equal(totalPages, result.TotalPages);
			Assert.Equal(PeopleCount, result.TotalItems);
			Assert.NotNull(result.Items);
			Assert.NotEmpty(result.Items);
			Assert.Equal(10, result.Items.Count);

			for (int i = 0; i < sorted.Count; i++) {
				Assert.Equal(sorted[i].FirstName, result.Items.ElementAt(i).FirstName);
			}
		}
	}
}
