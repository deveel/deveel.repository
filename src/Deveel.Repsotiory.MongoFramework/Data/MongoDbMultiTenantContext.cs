using Finbuckle.MultiTenant;
#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using MongoFramework;

namespace Deveel.Data
{
	public class MongoDbMultiTenantContext : MongoDbTenantContext
	{
		public MongoDbMultiTenantContext(IMongoDbConnection connection, IMultiTenantContextAccessor multiTenantContextAccessor) 
			: base(connection, multiTenantContextAccessor?.MultiTenantContext?.TenantInfo?.Id)
		{
		}
	}
}
