using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data
{
	public class MongoDbTenantConnection<TContext> : MongoDbTenantConnection, IMongoDbTenantConnection<TContext>
		where TContext : class, IMongoDbContext
	{
#if NET7_0_OR_GREATER
		public MongoDbTenantConnection(TenantInfo tenantInfo)
#else
		public MongoDbTenantConnection(ITenantInfo tenantInfo)
#endif
			: base(tenantInfo)
		{
		}
	}
}
