using Deveel.Data;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Utils
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMongoTenantContext(this IServiceCollection services, MongoDbTenantInfo tenantInfo)
		{
			//services.AddMultiTenant<MongoDbTenantInfo>()
			//	.WithStaticStrategy(tenantInfo.Identifier)
			//	.WithInMemoryStore(store =>
			//	{
			//		store.Tenants.Add(tenantInfo);
			//	});

			services.AddSingleton<ITenantInfo>(tenantInfo);
			services.AddSingleton<IMultiTenantContextAccessor<MongoDbTenantInfo>>(new StaticMultiTenantContextAccessor(tenantInfo));
			services.TryAddSingleton<IMultiTenantContextAccessor>(sp => (IMultiTenantContextAccessor) sp.GetService<IMultiTenantContextAccessor<MongoDbTenantInfo>>());
			//services.AddSingleton<IMultiTenantContext<MongoDbTenantInfo>>(sp => sp.GetService<IMultiTenantContextAccessor<MongoDbTenantInfo>>()?.MultiTenantContext);
			//services.TryAddSingleton<IMultiTenantContext>(sp => (IMultiTenantContext) sp.GetService<IMultiTenantContext<MongoDbTenantInfo>>());
			return services;
		}
	}
}
