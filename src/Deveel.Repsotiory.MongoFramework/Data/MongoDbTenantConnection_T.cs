using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoDbTenantConnection<TTenantInfo> : MongoDbTenantConnection where TTenantInfo : class, ITenantInfo, new() {
		public MongoDbTenantConnection(IMultiTenantContext<TTenantInfo> tenantContext)
			: base(tenantContext?.TenantInfo ?? throw new ArgumentNullException("Unable to resolve the tenant")) {
		}
	}
}
