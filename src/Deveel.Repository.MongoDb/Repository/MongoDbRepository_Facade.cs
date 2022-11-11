using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public class MongoRepository<TEntity, TFacade> : MongoRepository<TEntity>, IRepository<TFacade> 
		where TEntity : class, TFacade, IEntity
		where TFacade : class, IEntity {
		public MongoRepository(IOptions<MongoDbStoreOptions<TEntity>> options, IDocumentFieldMapper<TEntity> fieldMapper = null, ILogger<MongoStore<TEntity>> logger = null)
			: base(options, fieldMapper, logger) {
		}

		protected static TEntity Assert(TFacade obj) {
			if (!(obj is TEntity entity))
				throw new ArgumentException($"Cannot cast object of type '{typeof(TFacade)}' to '{typeof(TEntity)}' entity type");

			return entity;
		}

		Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<string> IRepository<TFacade>.CreateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);

		Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken)
			=> DeleteAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository<TFacade>.DeleteAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> DeleteAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);

		async Task<TFacade> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		async Task<PaginatedResult<TFacade>> IRepository<TFacade>.GetPageAsync(PageRequest<TFacade> page, CancellationToken cancellationToken) {
			var newPage = new PageRequest<TEntity>(page.Page, page.Size);
			var result = await GetPageAsync(newPage, cancellationToken);

			if (result == null)
				return PaginatedResult<TFacade>.Empty(page);

			return result.CastTo<TFacade>();
		}

		Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken)
			=> UpdateAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository<TFacade>.UpdateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> UpdateAsync(AssertMongoDbSession(session), Assert(entity), cancellationToken);
	}
}
