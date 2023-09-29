using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    public class EntityRepository<TEntity, TFacade> : EntityRepository<TEntity>, IRepository<TFacade> 
        where TFacade : class 
        where TEntity : class, TFacade {
        public EntityRepository(DbContext context, ITenantInfo? tenantInfo = null, ILogger<EntityRepository<TEntity, TFacade>>? logger = null) : base(context, logger) {
        }

        protected EntityRepository(DbContext context, ITenantInfo? tenantInfo = null, ILogger? logger = null) 
			: base(context, tenantInfo, logger) {
        }

		private static TEntity Assert(TFacade facade) {
			if (!(facade is TEntity entity))
				throw new ArgumentException($"The object is not an instance of type '{typeof(TEntity)}'");

			return entity;
		}

        Task<string> IRepository<TFacade>.AddAsync(TFacade entity, CancellationToken cancellationToken)
			=> AddAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.AddRangeAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> AddRangeAsync(entities.Select(x => Assert(x)), cancellationToken);

        Task<bool> IRepository<TFacade>.RemoveAsync(TFacade entity, CancellationToken cancellationToken)
			=> RemoveAsync(Assert(entity), cancellationToken);

		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken)
			=> UpdateAsync(Assert(entity), cancellationToken);

		string? IRepository<TFacade>.GetEntityId(TFacade entity)
			=> GetEntityId((TEntity) entity);
	}
}
