using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data {
	public interface IMongoDbTenantConnection : IMongoDbConnection {
		ITenantInfo TenantInfo { get; }
	}
}
