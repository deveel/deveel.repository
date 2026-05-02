using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data;

[Category("Integration")]
public class MongoRepositoryTests : MongoRepositoryTestSuite<MongoPerson> {
    protected override Faker<MongoPerson> PersonFaker { get; } = new MongoPersonFaker();

    protected override void ConfigureServices(IServiceCollection services) {
        AddRepository(services);
        base.ConfigureServices(services);
    }

    protected virtual void AddRepository(IServiceCollection services) {
        services
            .AddMongoDbContext<MongoDbContext>(builder => builder.UseConnection(ConnectionString))
            .AddRepositoryController();
        services.AddRepository<MongoRepository<MongoPerson, ObjectId>>();
    }

    protected async Task<MongoPerson?> FindPerson(ObjectId id) {
        var collection = MongoCollection;
        var result = await collection.FindAsync(x => x.Id == id);
        return await result.FirstOrDefaultAsync();
    }

    protected override async Task CleanupAsync() {
        await MongoCollection.DeleteManyAsync(x => true);
        await base.CleanupAsync();
    }

    [Test]
    [Category("Integration")]
    public async Task Should_ReturnSortedPage_When_OrderedByFieldName() {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var totalPages = (int)Math.Ceiling((double)PeopleCount / 10);
        var sorted = People!.OrderBy(x => x.FirstName).Take(10).ToList();
        var request = new PageQuery<MongoPerson>(1, 10).OrderBy("FirstName");

        // Act
        var result = await Repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.TotalPages).IsEqualTo(totalPages);
        await Assert.That(result.TotalItems).IsEqualTo(PeopleCount);
        await Assert.That(result.Items).IsNotNull();
        await Assert.That(result.Items.Count).IsEqualTo(10);
        for (int i = 0; i < sorted.Count; i++) {
            await Assert.That(result.Items.ElementAt(i).FirstName).IsEqualTo(sorted[i].FirstName);
        }
    }
}

