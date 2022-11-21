using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
    public class MongoRepositoryProvider<TEntity, TFacade> : MongoRepositoryProvider<TEntity>, IRepositoryProvider<TFacade>
        where TEntity : class, TFacade, IEntity
        where TFacade : class, IEntity
    {
        public MongoRepositoryProvider(
            IOptions<MongoDbOptions> baseOptions,
            IDocumentFieldMapper<TEntity>? fieldMapper = null,
            ITenantConnectionProvider? connectionProvider = null,
            ICollectionKeyProvider? collectionNameProvider = null,
            ILoggerFactory? loggerFactory = null) : base(baseOptions, fieldMapper, connectionProvider, collectionNameProvider, loggerFactory)
        {
        }

        IRepository<TFacade> IRepositoryProvider<TFacade>.GetRepository(string tenantId) => (IRepository<TFacade>)GetStore(tenantId);

        protected override MongoStore<TEntity> CreateStore(IOptions<MongoDbStoreOptions<TEntity>> options, ILogger logger)
            => new MongoRepository<TEntity, TFacade>(options, FieldMapper, logger);
    }
}
