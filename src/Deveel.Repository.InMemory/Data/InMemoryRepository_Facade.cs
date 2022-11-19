﻿using System;

namespace Deveel.Data {
	public class InMemoryRepository<TEntity, TFacade> : InMemoryRepository<TEntity>, IRepository<TFacade>
		where TEntity : class, IEntity, TFacade
		where TFacade : class, IEntity {
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

		async Task<TFacade?> IRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await base.FindAsync(filter, cancellationToken);

		Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> base.CreateAsync(entities.Select(Assert), cancellationToken);


		Task<IList<string>> IRepository<TFacade>.CreateAsync(IDataTransaction transaction, IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		Task<string> IRepository<TFacade>.CreateAsync(IDataTransaction transaction, TFacade entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");


		Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken) 
			=> DeleteAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository<TFacade>.DeleteAsync(IDataTransaction transaction, TFacade entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		async Task<PaginatedResult<TFacade>> IRepository<TFacade>.GetPageAsync(PageRequest<TFacade> request, CancellationToken cancellationToken) {
			var newPage = new PageRequest<TEntity>(request.Page, request.Size);
			var result = await GetPageAsync(newPage, cancellationToken);

			if (result == null)
				return PaginatedResult<TFacade>.Empty(request);

			return result.CastTo<TFacade>();
		}

		Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken) 
			=> UpdateAsync(Assert(entity), cancellationToken);
		
		Task<bool> IRepository<TFacade>.UpdateAsync(IDataTransaction transaction, TFacade entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");
		
		Task<IList<TFacade>> IRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) => throw new NotImplementedException();
		
		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken) 
			=> await FindByIdAsync(id, cancellationToken);
	}
}