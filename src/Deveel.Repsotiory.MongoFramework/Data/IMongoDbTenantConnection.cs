using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data {
	public interface IMongoDbTenantConnection<TContext> : IMongoDbConnection<TContext> where TContext : class, IMongoDbContext {
		ITenantInfo TenantInfo { get; }
	}
}
