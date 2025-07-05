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
		public static IServiceCollection AddMongoTenantContext(this IServiceCollection services, MongoDbTenantInfo tenantInfo)
		{
			services.AddSingleton<IMultiTenantContextAccessor<MongoDbTenantInfo>>(new StaticMultiTenantContextAccessor(tenantInfo));
			return services;
		}
	}
}
