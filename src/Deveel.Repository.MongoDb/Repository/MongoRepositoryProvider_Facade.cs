using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
    [Obsolete("This class is obsolete: please use the Deveel.Repository.MongoFramework instead")]
    public class MongoRepositoryProvider<TEntity, TFacade> : MongoRepositoryProvider<TEntity>, IRepositoryProvider<TFacade>
        where TEntity : class, TFacade
        where TFacade : class {
        public MongoRepositoryProvider(
            IOptions<MongoDbOptions> baseOptions,
            IDocumentFieldMapper<TEntity>? fieldMapper = null,
            ITenantConnectionProvider? connectionProvider = null,
            ICollectionKeyProvider? collectionNameProvider = null,
            ILoggerFactory? loggerFactory = null) : base(baseOptions, fieldMapper, connectionProvider, collectionNameProvider, loggerFactory) {
        }

        Task<IRepository<TFacade>> IRepositoryProvider<TFacade>.GetRepositoryAsync(string tenantId)  {
            var store = (IRepository<TFacade>) GetStore(tenantId);
            return Task.FromResult(store);
        }

        protected override MongoStore<TEntity> CreateStore(IOptions<MongoDbStoreOptions<TEntity>> options, ILogger logger)
            => new MongoRepository<TEntity, TFacade>(options, FieldMapper, logger);
    }
}
