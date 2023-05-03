using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;

using MongoFramework;

namespace Deveel.Data {
    public class MongoRepositoryProvider<TEntity, TFacade, TTenantInfo> : MongoRepositoryProvider<TEntity, TTenantInfo>, IRepositoryProvider<TFacade>
        where TFacade : class
        where TEntity : class, TFacade
        where TTenantInfo : class, ITenantInfo, new() {
        public MongoRepositoryProvider(IEnumerable<IMultiTenantStore<TTenantInfo>> stores, ILoggerFactory? loggerFactory = null) : base(stores, loggerFactory) {
        }

        async Task<IRepository<TFacade>> IRepositoryProvider<TFacade>.GetRepositoryAsync(string tenantId) {
            return (IRepository<TFacade>) await GetRepositoryAsync(tenantId);
        }

        protected override MongoRepository<MongoDbTenantContext, TEntity> CreateRepository(MongoDbTenantContext context, ILogger logger) {
            return new MongoRepository<MongoDbTenantContext, TEntity, TFacade>(context, logger);
        }
    }
}
