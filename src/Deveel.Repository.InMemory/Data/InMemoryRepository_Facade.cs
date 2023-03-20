using System;

namespace Deveel.Data {
	public class InMemoryRepository<TEntity, TFacade> : InMemoryRepository<TEntity>, IRepository<TFacade>, IPageableRepository<TFacade>, IFilterableRepository<TFacade>
		where TEntity : class, IDataEntity, TFacade
		where TFacade : class, IDataEntity {
		public InMemoryRepository() {
		}

		public InMemoryRepository(IEnumerable<TEntity> list) : base(list) {
		}

		Type IRepository.EntityType => typeof(TFacade);

		protected static TEntity Assert(TFacade obj) {
			if (!(obj is TEntity entity))
				throw new ArgumentException($"Cannot cast object of type '{typeof(TFacade)}' to '{typeof(TEntity)}' entity type");

			return entity;
		}

		async Task<TFacade?> IFilterableRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await FindAsync(filter, cancellationToken);

		Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> CreateAsync(entities.Select(Assert), cancellationToken);



		Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken) 
			=> DeleteAsync(Assert(entity), cancellationToken);

		async Task<RepositoryPage<TFacade>> IPageableRepository<TFacade>.GetPageAsync(RepositoryPageRequest<TFacade> request, CancellationToken cancellationToken) {
			var newPage = request.As<TEntity>();

			var result = await GetPageAsync(newPage, cancellationToken);

			if (result == null)
				return RepositoryPage<TFacade>.Empty(request);

			return result.As<TFacade>();
		}

		Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken) 
			=> UpdateAsync(Assert(entity), cancellationToken);
		
		async Task<IList<TFacade>> IFilterableRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			var result = await FindAllAsync(filter, cancellationToken);
			return result.Cast<TFacade>().ToList();
		}
		
		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken) 
			=> await FindByIdAsync(id, cancellationToken);
	}
}
