using System;

using Deveel.States;

using static System.Formats.Asn1.AsnWriter;

namespace Deveel.Repository {
	class FacadeRepository<TEntity, TFacade> : IRepository<TFacade>
		where TEntity : class, IEntity, TFacade
		where TFacade : class, IEntity {
		private readonly IRepository<TEntity> repository;

		public FacadeRepository(IRepository<TEntity> repository) {
			this.repository = repository;
		}

		public bool SupportsPaging => repository.SupportsPaging;

		public bool SupportsFilters => repository.SupportsFilters;

		private TEntity Assert(TFacade entity) {
			if (!(entity is TEntity other))
				throw new ArgumentException($"The object of type '{typeof(TFacade)}' cannot be converted to '{typeof(TEntity)}'");

			return other;
		}

		private TEntity Assert(object entity) {
			if (!(entity is TEntity other))
				throw new ArgumentException($"The object cannot be converted to '{typeof(TEntity)}'");

			return other;
		}

		public Task<string> CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> repository.CreateAsync(Assert(entity), cancellationToken);

		public Task<string> CreateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> repository.CreateAsync(session, Assert(entity), cancellationToken);

		public Task<string> CreateAsync(IEntity entity, CancellationToken cancellationToken)
			=> repository.CreateAsync(Assert(entity), cancellationToken);

		public Task<string> CreateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken)
			=> repository.CreateAsync(session, Assert(entity), cancellationToken);

		public Task<bool> DeleteAsync(TFacade entity, CancellationToken cancellationToken)
			=> repository.DeleteAsync(Assert(entity), cancellationToken);

		public Task<bool> DeleteAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> repository.DeleteAsync(session, Assert(entity), cancellationToken);

		public Task<bool> DeleteAsync(IEntity entity, CancellationToken cancellationToken)
			=> repository.DeleteAsync(Assert(entity), cancellationToken);

		public Task<bool> DeleteAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken)
			=> repository.DeleteAsync(session, Assert(entity), cancellationToken);

		public async Task<TFacade> FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await repository.FindByIdAsync(id, cancellationToken);

		public Task<bool> UpdateAsync(TFacade entity, CancellationToken cancellationToken)
			=> repository.UpdateAsync(Assert(entity), cancellationToken);

		public Task<bool> UpdateAsync(IDataTransaction session, TFacade entity, CancellationToken cancellationToken)
			=> repository.UpdateAsync(session, Assert(entity), cancellationToken);

		public Task<bool> UpdateAsync(IEntity entity, CancellationToken cancellationToken)
			=> repository.UpdateAsync(Assert(entity), cancellationToken);

		public Task<bool> UpdateAsync(IDataTransaction session, IEntity entity, CancellationToken cancellationToken)
			=> repository.UpdateAsync(session, Assert(entity), cancellationToken);

		async Task<IEntity> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
		=> await repository.FindByIdAsync(id, cancellationToken);

		async Task<PaginatedResult> IRepository.GetPageAsync(PageRequest page, CancellationToken cancellationToken)
			=> await repository.GetPageAsync(page, cancellationToken);

		public Task<PaginatedResult<TFacade>> GetPageAsync(PageRequest<TFacade> request, CancellationToken cancellationToken = default)
			=> throw new NotImplementedException();

		public async Task<TFacade> FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await repository.FindAsync(filter, cancellationToken);

		async Task<IEntity> IRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await repository.FindAsync(filter, cancellationToken);

		public async Task<IList<TFacade>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			var result = await repository.FindAllAsync(filter, cancellationToken);
			return result.Cast<TFacade>().ToList();
		}

		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> repository.ExistsAsync(filter, cancellationToken);

		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> repository.CountAsync(filter, cancellationToken);

		async Task<IList<IEntity>> IRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) { 
			var result = await FindAllAsync(filter, cancellationToken);
			return result.Cast<IEntity>().ToList();
		}
	}
}
