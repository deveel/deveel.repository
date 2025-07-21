using MongoDB.Driver.Core.Configuration;

using MongoFramework;

namespace Deveel.Data {
	public static class MongoDbConnectionTests {
		[Fact]
		public static void GetUrlFromConnection() {
			const string connectionString = "mongodb://localhost:27017/testdb";

			var connection = MongoDbConnection.FromConnectionString(connectionString);
			Assert.NotNull(connection);

			var url = connection.GetUrl();
			Assert.NotNull(url);
			Assert.Equal(ConnectionStringScheme.MongoDB, url.Scheme);
			Assert.Equal("localhost", url.Server.Host);
			Assert.Equal(27017, url.Server.Port);
			Assert.Equal("testdb", url.DatabaseName);
		}

		[Fact]
		public static void GetUrlFromConnectionOfContext() {
			const string connectionString = "mongodb://localhost:27017/testdb";

			var connection = new MongoDbConnection<MongoDbContext>(connectionString);
			Assert.NotNull(connection);

			var url = connection.GetUrl();
			Assert.NotNull(url);
			Assert.Equal(ConnectionStringScheme.MongoDB, url.Scheme);
			Assert.Equal("localhost", url.Server.Host);
			Assert.Equal(27017, url.Server.Port);
			Assert.Equal("testdb", url.DatabaseName);
		}

	}
}
