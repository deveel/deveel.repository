using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoDbTenantConnection : MongoDbConnection, IMongoDbTenantConnection {
		public MongoDbTenantConnection(IMultiTenantContext tenantContext) 
			: this(tenantContext?.TenantInfo ?? throw new ArgumentNullException("Unable to resolve the tenant")) {
		}

		protected MongoDbTenantConnection(ITenantInfo tenantInfo) {
			TenantInfo = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));

			var connectionString = TenantInfo.ConnectionString;
			if (String.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("The connection string for the tenant is not defined");

			Url = MongoUrl.Create(connectionString);
		}

		public ITenantInfo TenantInfo { get; }
	}
}
