using Finbuckle.MultiTenant;

namespace Deveel.Data
{
	public class MongoDbTenantInfo : TenantInfo
	{
#if NET7_0_OR_GREATER
		public string? ConnectionString { get; set; }
#endif
	}
}
