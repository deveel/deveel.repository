using Deveel.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deveel.Data {
    public class MongoRepository<TEntity, TFacade> : MongoRepository<TEntity>, IRepository<TFacade>
        where TEntity : class, TFacade, IEntity
        where TFacade : class, IEntity {
        public MongoRepository(IOptions<MongoDbStoreOptions<TEntity>> options, IDocumentFieldMapper<TEntity>? fieldMapper = null, ILogger<MongoStore<TEntity>>? logger = null)
            : base(options, fieldMapper, logger) {
        }

        protected internal MongoRepository(IOptions<MongoDbStoreOptions<TEntity>> options, IDocumentFieldMapper<TEntity>? fieldMapper = null, ILogger? logger = null)
            : base(options, fieldMapper, logger) {
        }

        Type IRepository.EntityType => typeof(TFacade);

        protected static TEntity Assert(TFacade obj) {
            if (!(obj is TEntity entity))
                throw new ArgumentException($"Cannot cast object of type '{typeof(TFacade)}' to '{typeof(TEntity)}' entity type");

            return entity;
        }

        async Task<IList<TFacade>> IRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
            var result = await FindAllAsync(GetFilterDefinition(filter), cancellationToken);
            return result.Cast<TFacade>().ToList();
        }

        async Task<TFacade?> IRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(GetFilterDefinition(filter), cancellationToken);

        Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
            => CreateAsync(Assert(entity), cancellationToken);

        Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
            => base.CreateAsync(entities.Select(Assert), cancellationToken);

        Task<IList<string>> IRepository<TFacade>.CreateAsync(IDataTransaction transaction, IEnumerable<TFacade> entities, CancellationToken cancellationToken)
            => base.CreateAsync(AssertMongoDbSession(transaction), entities.Select(Assert), cancellationToken);

        Task<string> IRepository<TFacade>.CreateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
            => CreateAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);

        Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken)
            => DeleteAsync(Assert(entity), cancellationToken);

        Task<bool> IRepository<TFacade>.DeleteAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
            => DeleteAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);

        async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
            => await FindByIdAsync(id, cancellationToken);

        async Task<RepositoryPage<TFacade>> IRepository<TFacade>.GetPageAsync(RepositoryPageRequest<TFacade> page, CancellationToken cancellationToken) {
            var newPage = page.As<TEntity>();
            var result = await GetPageAsync(newPage, cancellationToken);

            if (result == null)
                return RepositoryPage<TFacade>.Empty(page);

            return result.As<TFacade>();
        }

        Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken)
            => UpdateAsync(Assert(entity), cancellationToken);

        Task<bool> IRepository<TFacade>.UpdateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
            => UpdateAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);
    }
}