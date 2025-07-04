using Deveel.Data;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

namespace Deveel.Utils
{
	class StaticMultiTenantContextAccessor : IMultiTenantContextAccessor<DbTenantInfo>
	{
		public StaticMultiTenantContextAccessor(DbTenantInfo tenantInfo)
		{
			MultiTenantContext = new MultiTenantContext<DbTenantInfo>
			{
				TenantInfo = tenantInfo,
			};
		}

#if NET7_0_OR_GREATER
		public IMultiTenantContext<DbTenantInfo> MultiTenantContext { get; }

		IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;
#else
		public IMultiTenantContext<DbTenantInfo>? MultiTenantContext { get; set; }
#endif
	}
}
