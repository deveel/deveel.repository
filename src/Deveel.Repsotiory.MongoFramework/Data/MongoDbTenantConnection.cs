using Finbuckle.MultiTenant;

using MongoDB.Driver;

using MongoFramework;

namespace Deveel.Data {
	public class MongoDbTenantConnection<TContext, TTenantInfo> : MongoDbConnection, IMongoDbTenantConnection<TContext>
		where TContext : class, IMongoDbContext 
		where TTenantInfo : class, ITenantInfo, new() {
		public MongoDbTenantConnection(IMultiTenantContext<TTenantInfo> tenantContext) 
			: this(tenantContext?.TenantInfo ?? throw new ArgumentNullException("Unable to resolve the tenant")) {
		}

		protected MongoDbTenantConnection(TTenantInfo tenantInfo) {
			TenantInfo = tenantInfo ?? throw new ArgumentNullException(nameof(tenantInfo));

			var connectionString = TenantInfo.ConnectionString;
			if (String.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("The connection string for the tenant is not defined");

			Url = MongoUrl.Create(connectionString);
		}

		public TTenantInfo TenantInfo { get; }

		ITenantInfo IMongoDbTenantConnection<TContext>.TenantInfo => TenantInfo;
	}
}
