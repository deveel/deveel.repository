using Finbuckle.MultiTenant;

namespace Deveel.Data
{
	public class DbTenantInfo : TenantInfo
	{
		public string? ConnectionString { get; set; }
	}
}
