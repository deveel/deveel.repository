using MongoFramework;

namespace Deveel.Data {
	public interface IMongoDbConnection<TContext> : IMongoDbConnection where TContext : class, IMongoDbContext {
	}
}
