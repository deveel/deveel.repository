using MongoDB.Driver.Core.Configuration;

using MongoFramework;

namespace Deveel.Data;

[Category("Unit")]
public class MongoDbConnectionTests {
    [Test]
    public async Task Should_ParseConnectionString_When_UrlBuiltFromConnection() {
        // Arrange
        const string connectionString = "mongodb://localhost:27017/testdb";

        // Act
        var connection = MongoDbConnection.FromConnectionString(connectionString);
        var url = connection.GetUrl();

        // Assert
        await Assert.That(connection).IsNotNull();
        await Assert.That(url).IsNotNull();
        await Assert.That(url.Scheme).IsEqualTo(ConnectionStringScheme.MongoDB);
        await Assert.That(url.Server.Host).IsEqualTo("localhost");
        await Assert.That(url.Server.Port).IsEqualTo(27017);
        await Assert.That(url.DatabaseName).IsEqualTo("testdb");
    }

    [Test]
    public async Task Should_ParseConnectionString_When_TypedConnectionCreated() {
        // Arrange
        const string connectionString = "mongodb://localhost:27017/testdb";

        // Act
        var connection = new MongoDbConnection<MongoDbContext>(connectionString);
        var url = connection.GetUrl();

        // Assert
        await Assert.That(connection).IsNotNull();
        await Assert.That(url).IsNotNull();
        await Assert.That(url.Scheme).IsEqualTo(ConnectionStringScheme.MongoDB);
        await Assert.That(url.Server.Host).IsEqualTo("localhost");
        await Assert.That(url.Server.Port).IsEqualTo(27017);
        await Assert.That(url.DatabaseName).IsEqualTo("testdb");
    }
}

