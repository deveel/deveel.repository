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

        Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken)
			=> CreateAsync(entities.Select(x => Assert(x)), cancellationToken);

        Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken)
			=> DeleteAsync(Assert(entity), cancellationToken);

		async Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

        Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken)
			=> UpdateAsync(Assert(entity), cancellationToken);
    }
}
