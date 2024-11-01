using MongoFramework;

namespace Deveel.Data
{
	public interface IMongoDbTenantConnection<TContext> : IMongoDbTenantConnection, IMongoDbConnection<TContext>
		where TContext : class, IMongoDbContext
	{
	}
}
