using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Deveel.Data {
    public class MongoRepository<TDocument> : MongoStore<TDocument>, IRepository<TDocument>, IQueryableRepository<TDocument>, IControllableRepository
        where TDocument : class, IEntity {
        private bool disposed;

        public MongoRepository(IOptions<MongoDbStoreOptions<TDocument>> options, IDocumentFieldMapper<TDocument>? fieldMapper = null, ILogger<MongoRepository<TDocument>>? logger = null) 
            : this(options, fieldMapper, (ILogger?) logger) {
        }

        protected internal MongoRepository(IOptions<MongoDbStoreOptions<TDocument>> options, IDocumentFieldMapper<TDocument>? fieldMapper = null, ILogger? logger = null)
            : base(options, logger) {
			FieldMapper = fieldMapper;
		}

        protected IDocumentFieldMapper<TDocument>? FieldMapper { get; private set; }

        bool IRepository.SupportsPaging => true;

        bool IRepository.SupportsFilters => true;

        Type IRepository.EntityType => typeof(TDocument);

        protected void ThrowIfDisposed() {
            if (disposed)
                throw new ObjectDisposedException(GetType().FullName);
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            disposed = true;
        }

        public void SetMapper(IDocumentFieldMapper<TDocument> mapper) {
            FieldMapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public void SetMapper(Func<string, string> mapper)
            => SetMapper(new DelegatedDocumentFieldMapper<TDocument>(mapper));

        IQueryable<TDocument> IQueryableRepository<TDocument>.AsQueryable() => AsQueryable();

        internal IClientSessionHandle AssertMongoDbSession(IDataTransaction dataSession) {
            if (dataSession is MongoTransaction session)
                return session.SessionHandle;

            throw new ArgumentException("The session type is invalid in this context");
        }

        private TDocument AssertIsEntity(object obj) {
            if (!(obj is TDocument entity))
                throw new ArgumentException($"The object provided is not of type {typeof(TDocument)}");

            return entity;
        }

		#region Controller Functions

        protected virtual CreateCollectionOptions GetCreateOptions() {
            return new CreateCollectionOptions();
        }

        protected virtual Task CreateAsync(CancellationToken cancellationToken = default) {
            return CreateAsync(GetCreateOptions(), cancellationToken);
        }

        Task IControllableRepository.CreateAsync(CancellationToken cancellationToken)
            => CreateAsync(cancellationToken);

        Task IControllableRepository.DropAsync(CancellationToken cancellationToken)
            => DropAsync(cancellationToken);

        protected virtual Task DropAsync(CancellationToken cancellationToken = default) {
            return DropAsync(GetDropOptions(), cancellationToken);
        }

        protected virtual DropCollectionOptions GetDropOptions() {
            return new DropCollectionOptions();
        }

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default) {
            // TODO: maybe use the filter in the options to optimize?
            var options = new ListCollectionNamesOptions {
                Filter = new BsonDocument(new Dictionary<string, object> { { "name", StoreOptions.CollectionName } })
            };

            using var names = await Database.ListCollectionNamesAsync(options, cancellationToken);
            var result = await names.SingleOrDefaultAsync(cancellationToken);

            return StoreOptions.CollectionName == result;
        }

		#endregion


		/// <inheritdoc />
		Task<string> IRepository<TDocument>.CreateAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
            return CreateAsync(AssertMongoDbSession(session), entity, cancellationToken);
        }

        /// <inheritdoc />
        Task<string> IRepository.CreateAsync(IEntity entity, CancellationToken cancellationToken) {
            return CreateAsync(AssertIsEntity(entity), cancellationToken);
        }

        /// <inheritdoc />
        Task<string> IRepository.CreateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
            return CreateAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
        }

        public Task<string> CreateAsync(MongoTransaction transaction, TDocument document, CancellationToken cancellationToken = default)
            => CreateAsync(transaction.SessionHandle, document, cancellationToken);

        Task<IList<string>> IRepository.CreateAsync(IEnumerable<IEntity> entities, CancellationToken cancellationToken)
            => base.CreateAsync(entities.Select(AssertIsEntity), cancellationToken);

        Task<IList<string>> IRepository.CreateAsync(IDataTransaction transaction, IEnumerable<IEntity> entities, CancellationToken cancellationToken)
            => base.CreateAsync(AssertMongoDbSession(transaction), entities.Select(AssertIsEntity), cancellationToken);

        Task<IList<string>> IRepository<TDocument>.CreateAsync(IEnumerable<TDocument> entities, CancellationToken cancellationToken)
            => base.CreateAsync(entities, cancellationToken);

        Task<IList<string>> IRepository<TDocument>.CreateAsync(IDataTransaction transaction, IEnumerable<TDocument> entities, CancellationToken cancellationToken)
            => base.CreateAsync(AssertMongoDbSession(transaction), entities, cancellationToken);

        public Task<IList<string>> CreateAsync(MongoTransaction transaction, IEnumerable<TDocument> entities, CancellationToken cancellationToken = default)
            => CreateAsync(transaction.SessionHandle, entities, cancellationToken);

        /// <inheritdoc />
        async Task<IEntity?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken) {
            return await FindByIdAsync(id, cancellationToken);
        }

        Task<IEntity?> IRepository.FindByIdAsync(IDataTransaction transaction, string id, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        Task<bool> IRepository<TDocument>.UpdateAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
            return UpdateAsync(AssertMongoDbSession(session), entity, cancellationToken);
        }

        public Task<bool> UpdateAsync(MongoTransaction transaction, TDocument entity, CancellationToken cancellationToken = default)
            => UpdateAsync(transaction?.SessionHandle, entity, cancellationToken);

        /// <inheritdoc />
        Task<bool> IRepository<TDocument>.DeleteAsync(IDataTransaction session, TDocument entity, CancellationToken cancellationToken) {
            return DeleteAsync(AssertMongoDbSession(session), entity, cancellationToken);
        }

        /// <inheritdoc />
        Task<bool> IRepository.DeleteAsync(IEntity entity, CancellationToken cancellationToken) {
            return DeleteAsync(AssertIsEntity(entity), cancellationToken);
        }

        /// <inheritdoc />
        Task<bool> IRepository.DeleteAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
            return DeleteAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
        }

        public Task<bool> DeleteAsync(MongoTransaction transaction, TDocument document, CancellationToken cancellationToken = default)
            => DeleteAsync(transaction.SessionHandle, document, cancellationToken);

        /// <inheritdoc />
        Task<bool> IRepository.UpdateAsync(IEntity entity, CancellationToken cancellationToken) {
            return UpdateAsync(AssertIsEntity(entity), cancellationToken);
        }

        /// <inheritdoc />
        Task<bool> IRepository.UpdateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken) {
            return UpdateAsync(AssertMongoDbSession(session), AssertIsEntity(entity), cancellationToken);
        }

        /// <inheritdoc />
        async Task<RepositoryPage> IRepository.GetPageAsync(RepositoryPageRequest page, CancellationToken cancellationToken) {
            var request = page.AsPageQuery(Field);

            var result = await base.GetPageAsync(request, cancellationToken);

            return new RepositoryPage(page, result.TotalItems, result.Items?.Cast<IEntity>());
        }

        public virtual async Task<RepositoryPage<TDocument>> GetPageAsync(RepositoryPageRequest<TDocument> page, CancellationToken cancellationToken = default) {
            var pageQuery = page.AsPageQuery(Field);
            var result = await base.GetPageAsync(pageQuery, cancellationToken);

            return new RepositoryPage<TDocument>(page, result.TotalItems, result.Items);
        }

        Task<bool> IRepository.ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => ExistsAsync(GetFilterDefinition(filter), cancellationToken);

        Task<long> IRepository.CountAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => CountAsync(GetFilterDefinition(filter), cancellationToken);

        async Task<IEntity?> IRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(GetFilterDefinition(filter), cancellationToken);

        Task<TDocument?> IRepository<TDocument>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => FindAsync(GetFilterDefinition(filter), cancellationToken);

        Task<IList<TDocument>> IRepository<TDocument>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => FindAllAsync(GetFilterDefinition(filter), cancellationToken);

        async Task<IList<IEntity>> IRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
            var result = await FindAllAsync(GetFilterDefinition(filter), cancellationToken);
            return result.Cast<IEntity>().ToList();
        }

        protected virtual FilterDefinition<TDocument> GetFilterDefinition(IQueryFilter filter) {
            if (filter == null || filter.IsEmpty())
                return Builders<TDocument>.Filter.Empty;

            if (filter is ExpressionQueryFilter<TDocument> expr)
                return Builders<TDocument>.Filter.Where(expr.Expression);
            if (filter is MongoQueryFilter<TDocument> filterDef)
                return filterDef.Filter;

            throw new ArgumentException($"The query filter type '{filter.GetType()}' is not supported by Mongo");
        }

        protected override FieldDefinition<TDocument, object> Field(string fieldName) {
            if (FieldMapper != null) {
                fieldName = FieldMapper.MapField(fieldName);
            }

            return new StringFieldDefinition<TDocument, object>(fieldName);
        }
    }
}