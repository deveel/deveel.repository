using Deveel.Data;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

namespace Deveel.Utils
{
	class StaticMultiTenantContextAccessor : IMultiTenantContextAccessor<MongoDbTenantInfo>, IMultiTenantContextAccessor
	{
		public StaticMultiTenantContextAccessor(MongoDbTenantInfo tenantInfo)
		{
			MultiTenantContext = new MultiTenantContext<MongoDbTenantInfo>
			{
				TenantInfo = tenantInfo,
			};
		}

#if NET7_0_OR_GREATER
		public IMultiTenantContext<MongoDbTenantInfo> MultiTenantContext { get; }

		IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => MultiTenantContext;
#else
		public IMultiTenantContext<MongoDbTenantInfo>? MultiTenantContext { get; set; }
		
		IMultiTenantContext? IMultiTenantContextAccessor.MultiTenantContext { 
			get => (IMultiTenantContext?) MultiTenantContext;
			set => MultiTenantContext = (IMultiTenantContext<MongoDbTenantInfo>?) value;
		}
#endif
	}
}
