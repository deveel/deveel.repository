using Deveel.Data;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

namespace Deveel.Utils
{
	class StaticMultiTenantContextAccessor : IMultiTenantContextAccessor<MongoTenantInfo>
	{
		public StaticMultiTenantContextAccessor(MongoTenantInfo tenantInfo)
		{
			MultiTenantContext = new MultiTenantContext<MongoTenantInfo>
			{
				TenantInfo = tenantInfo,
			};
		}

#if NET7_0_OR_GREATER
		public IMultiTenantContext<MongoTenantInfo> MultiTenantContext { get; }

		IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;
#else
		public IMultiTenantContext<MongoTenantInfo>? MultiTenantContext { get; set; }
#endif
	}
}
