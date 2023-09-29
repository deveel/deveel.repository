using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public sealed class MongoDbConnection<TContext> : MongoDbConnection, IMongoDbConnection<TContext> 
		where TContext : class, IMongoDbContext {
		private MongoDbConnection(MongoUrl url) {
			Url = url;
		}

		public static new MongoDbConnection<TContext> FromUrl(MongoUrl url)
			=> new MongoDbConnection<TContext>(url);

		public static new MongoDbConnection<TContext> FromConnectionString(string connectionString)
			=> new MongoDbConnection<TContext>(new MongoUrl(connectionString));
	}
}
