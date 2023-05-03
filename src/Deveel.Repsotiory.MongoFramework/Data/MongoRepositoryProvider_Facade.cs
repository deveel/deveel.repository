using Finbuckle.MultiTenant;

using Microsoft.Extensions.Logging;

using MongoFramework;

namespace Deveel.Data {
    public class MongoRepositoryProvider<TContext, TEntity, TFacade, TTenantInfo> : MongoRepositoryProvider<TContext, TEntity, TTenantInfo>, IRepositoryProvider<TFacade>
        where TContext : class, IMongoDbContext
        where TFacade : class
        where TEntity : class, TFacade
        where TTenantInfo : class, ITenantInfo, new() {
        public MongoRepositoryProvider(IEnumerable<IMultiTenantStore<TTenantInfo>>? stores = null, ILoggerFactory? loggerFactory = null) 
            : base(stores, loggerFactory) {
        }

        async Task<IRepository<TFacade>> IRepositoryProvider<TFacade>.GetRepositoryAsync(string tenantId) {
            return (IRepository<TFacade>) await GetRepositoryAsync(tenantId);
        }

        protected override MongoRepository<TContext, TEntity> CreateRepository(TContext context) {
            var logger = LoggerFactory.CreateLogger<MongoRepository<TContext, TEntity, TFacade>>();
            return new MongoRepository<TContext, TEntity, TFacade>(context, logger);
        }
    }
}
