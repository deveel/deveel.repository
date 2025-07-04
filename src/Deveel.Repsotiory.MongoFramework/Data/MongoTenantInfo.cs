using Finbuckle.MultiTenant;

namespace Deveel.Data
{
    public class MongoTenantInfo : TenantInfo
	{
		public string? ConnectionString { get; set; }
	}
}