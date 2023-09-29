using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoDbTenantConnection<TContext> : MongoDbTenantConnection<TContext, TenantInfo> 
		where TContext : class, IMongoDbContext {
		public MongoDbTenantConnection(IMultiTenantContext<TenantInfo> tenantContext)
			: base(tenantContext?.TenantInfo ?? throw new ArgumentNullException("Unable to resolve the tenant")) {
		}
	}
}
