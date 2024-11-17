using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data
{
	class MongoRepositoryProvider<TRepository> : IRepositoryProvider<TRepository>
		where TRepository : class
	{
		private readonly IServiceScopeFactory scopeFactory;

		public MongoRepositoryProvider(IServiceScopeFactory scopeFactory)
		{
			this.scopeFactory = scopeFactory;
		}

		public async Task<TRepository?> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default)
		{
			var scope = scopeFactory.CreateScope();

#if NET7_0_OR_GREATER
			var tenantStores = scope.ServiceProvider.GetServices<IMultiTenantStore<MongoDbTenantInfo>>();
#else
			var tenantStores = scope.ServiceProvider.GetServices<IMultiTenantStore<MongoDbTenantInfo>>();
#endif

			if (tenantStores == null)
				return null;

			MongoDbTenantInfo? tenantInfo = null;

			foreach (var store in tenantStores)
			{
				tenantInfo = await store.TryGetByIdentifierAsync(tenantId);
                if (tenantInfo == null)
                    tenantInfo = await store.TryGetAsync(tenantId);

				if (tenantInfo != null)
					break;
			}

			if (tenantInfo == null)
				return null;

#if NET7_0_OR_GREATER
			var tenantContextSetter = scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
			tenantContextSetter.MultiTenantContext = new MultiTenantContext<MongoDbTenantInfo>
			{
				TenantInfo = tenantInfo
			};
#else
			var tenantContextAccessor = scope.ServiceProvider.GetRequiredService<IMultiTenantContextAccessor>();
			tenantContextAccessor.MultiTenantContext = new MultiTenantContext<MongoDbTenantInfo>
			{
				TenantInfo = tenantInfo
			};
#endif

			return scope.ServiceProvider.GetRequiredService<TRepository>();
		}
	}
}
