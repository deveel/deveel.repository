using Deveel.Data;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Utils
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddDbTenantContext(this IServiceCollection services, DbTenantInfo tenantInfo)
		{
			services.AddSingleton<IMultiTenantContextAccessor<DbTenantInfo>>(new StaticMultiTenantContextAccessor(tenantInfo));
			return services;
		}
	}
}
