using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
	[Obsolete("This class is obsolete: please use the Deveel.Repository.MongoFramework instead")]
	public class MongoRepositoryProvider<TEntity> : MongoStoreProvider<TEntity>, 
		IRepositoryProvider<TEntity> where TEntity : class
    {
        public MongoRepositoryProvider(
            IOptions<MongoDbOptions> baseOptions,
            IDocumentFieldMapper<TEntity>? fieldMapper = null,
            ITenantConnectionProvider? connectionProvider = null,
            ICollectionKeyProvider? collectionNameProvider = null,
            ILoggerFactory? loggerFactory = null)
            : base(baseOptions, connectionProvider, collectionNameProvider, loggerFactory)
        {
            FieldMapper = fieldMapper;
        }

        protected IDocumentFieldMapper<TEntity>? FieldMapper { get; }

        /// <inheritdoc />
        Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId)
        {
            var store = (MongoRepository<TEntity>)GetStore(tenantId);
            return Task.FromResult<IRepository<TEntity>>(store);
        }

        Task<IRepository> IRepositoryProvider.GetRepositoryAsync(string tenantId) {
            var store = (MongoRepository<TEntity>)GetStore(tenantId);
            return Task.FromResult<IRepository>(store);
        }

        public MongoRepository<TEntity> GetRepository(string tenantId)
            => (MongoRepository<TEntity>)GetStore(tenantId);

        protected override MongoStore<TEntity> CreateStore(IOptions<MongoDbStoreOptions<TEntity>> options, ILogger logger)
            => new MongoRepository<TEntity>(options, FieldMapper, logger);
    }
}
