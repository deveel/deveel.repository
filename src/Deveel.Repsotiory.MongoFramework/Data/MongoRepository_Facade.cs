using System;
using System.Linq.Expressions;

using Deveel.Repository;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Linq;

namespace Deveel.Data {
	public class MongoRepository<TEntity, TFacade> : MongoRepository<TEntity>, IRepository<TFacade>, IPageableRepository<TFacade>
		where TEntity : class, TFacade
		where TFacade : class, IEntity {

		public MongoRepository(MongoDbContext context, ILogger<MongoRepository<TEntity, TFacade>>? logger = null)
			: this(context, (ILogger?) logger) {

		}

		protected internal MongoRepository(MongoDbContext context, ILogger? logger = null) : base(context, logger) {
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

		async Task<IList<TFacade>> IRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			var result = await FindAllAsync(GetFilterDefinition(filter), cancellationToken);
			return result.Cast<TFacade>().ToList();
		}

		async Task<long> IRepository.CountAsync(IQueryFilter filter, System.Threading.CancellationToken cancellationToken)
			=> await CountAsync(GetFilterDefinition(filter), cancellationToken);

		async Task<TFacade?> IRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await FindAsync(GetFilterDefinition(filter), cancellationToken);

		Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> base.CreateAsync(entities.Select(Assert), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.CreateAsync(IDataTransaction transaction, IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		Task<string> IRepository<TFacade>.CreateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transaction not supported");

		Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken)
			=> DeleteAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository<TFacade>.DeleteAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transaction not supported");

		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		async Task<RepositoryPage<TFacade>> IPageableRepository<TFacade>.GetPageAsync(RepositoryPageRequest<TFacade> page, CancellationToken cancellationToken) {
			try {
				var filter = Builders<TEntity>.Filter.Empty;
				if (page.Filter != null) {
					filter = page.Filter.AsMongoFilter<TEntity>();
				}

				var totalCount = await Collection.CountDocumentsAsync(filter, null, cancellationToken);

				var findOptions = new FindOptions<TEntity> {
					Skip = page.Offset,
					Limit = page.Size
				};

				if (page.SortBy != null) {
					foreach (var sort in page.SortBy) {
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

		Task<bool> IRepository<TFacade>.UpdateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transaction not supported");

	}
}
