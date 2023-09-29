using Deveel.Repository;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Linq;

namespace Deveel.Data {
    public class MongoRepository<TContext, TEntity, TFacade> : MongoRepository<TContext, TEntity>, 
		IRepository<TFacade>, 
		IPageableRepository<TFacade>, 
		IFilterableRepository<TFacade>
		where TContext : class, IMongoDbContext
		where TEntity : class, TFacade
		where TFacade : class {

		public MongoRepository(TContext context, ISystemTime? systemTime = null, ILogger<MongoRepository<TContext, TEntity, TFacade>>? logger = null)
			: this(context, systemTime, (ILogger?) logger) {

		}

		protected internal MongoRepository(TContext context, ISystemTime? systemTime = null, ILogger? logger = null) 
			: base(context, systemTime, logger) {
		}

		Type IRepository.EntityType => typeof(TFacade);

		protected static TEntity Assert(TFacade obj) {
			if (!(obj is TEntity entity))
				throw new ArgumentException($"Cannot cast object of type '{typeof(TFacade)}' to '{typeof(TEntity)}' entity type");

			return entity;
		}

		protected override FilterDefinition<TEntity> GetFilterDefinition(IQueryFilter? filter) {
			if (filter is ExpressionQueryFilter<TFacade> exprFilter) {
				var expr = exprFilter.Expression.AsMongoFilter<TEntity>();

				filter = new MongoQueryFilter<TEntity>(expr);
				// filter = new ExpressionQueryFilter<TEntity>(expr);
			}

			return base.GetFilterDefinition(filter);
		}

		string? IRepository<TFacade>.GetEntityId(TFacade entity) => ((IRepository<TEntity>)this).GetEntityId(Assert(entity));

		async Task<IList<TFacade>> IFilterableRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			var result = await FindAllAsync(GetFilterDefinition(filter), cancellationToken);
			return result.Cast<TFacade>().ToList();
		}

		async Task<long> IFilterableRepository.CountAsync(IQueryFilter filter, System.Threading.CancellationToken cancellationToken)
			=> await CountAsync(GetFilterDefinition(filter), cancellationToken);

		async Task<TFacade?> IFilterableRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await FindAsync(GetFilterDefinition(filter), cancellationToken);

		Task<string> IRepository<TFacade>.AddAsync(TFacade entity, CancellationToken cancellationToken)
			=> AddAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.AddRangeAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> base.AddRangeAsync(entities.Select(Assert), cancellationToken);

		Task<bool> IRepository<TFacade>.RemoveAsync(TFacade entity, CancellationToken cancellationToken)
			=> RemoveAsync(Assert(entity), cancellationToken);

		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		async Task<RepositoryPage<TFacade>> IPageableRepository<TFacade>.GetPageAsync(RepositoryPageRequest<TFacade> page, CancellationToken cancellationToken) {
			try {
				var filter = Builders<TEntity>.Filter.Empty;
				if (page.Filter != null) {
					filter = page.Filter.AsLambda<TFacade>().AsMongoFilter<TEntity>();
				}

				var totalCount = await Collection.CountDocumentsAsync(filter, null, cancellationToken);

				var findOptions = new FindOptions<TEntity> {
					Skip = page.Offset,
					Limit = page.Size
				};

				if (page.ResultSorts != null) {
					foreach (var sort in page.ResultSorts) {
						SortDefinition<TEntity> sortDef;

						if (sort.Field is StringFieldRef stringRef) {
							sortDef = sort.Ascending 
								? Builders<TEntity>.Sort.Ascending(Field(stringRef.FieldName))
								: Builders<TEntity>.Sort.Descending(Field(stringRef.FieldName));
						} else if (sort.Field is ExpressionFieldRef<TEntity> exprRef) {
							sortDef = sort.Ascending
								? Builders<TEntity>.Sort.Ascending(exprRef.Expression)
								: Builders<TEntity>.Sort.Descending(exprRef.Expression);
						} else {
							throw new NotSupportedException();
						}

						if (findOptions.Sort == null) {
							findOptions.Sort = sortDef;
						} else {
							findOptions.Sort = Builders<TEntity>.Sort.Combine(findOptions.Sort, sortDef);
						}
					}
				}

				var result = await Collection.FindAsync(filter, findOptions, cancellationToken);
				var items = await result.ToListAsync(cancellationToken);
				return new RepositoryPage<TFacade>(page, (int)totalCount, items?.Cast<TFacade>());
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken)
			=> UpdateAsync(Assert(entity), cancellationToken);

	}
}
