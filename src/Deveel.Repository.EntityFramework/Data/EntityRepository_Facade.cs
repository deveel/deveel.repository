using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
    public class EntityRepository<TEntity, TFacade> : EntityRepository<TEntity>, IRepository<TFacade>, IFilterableRepository<TFacade> 
        where TFacade : class, IDataEntity 
        where TEntity : class, TFacade {
        public EntityRepository(DbContext context, ILogger<EntityRepository<TEntity, TFacade>>? logger = null) : base(context, logger) {
        }

        protected EntityRepository(DbContext context, ILogger? logger = null) : base(context, logger) {
        }

        Task<string> IRepository<TFacade>.CreateAsync(TFacade entity, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<IList<string>> IRepository<TFacade>.CreateAsync(IEnumerable<TFacade> entities, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<bool> IRepository<TFacade>.DeleteAsync(TFacade entity, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<IList<TFacade>> IFilterableRepository<TFacade>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<TFacade?> IFilterableRepository<TFacade>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<TFacade?> IRepository<TFacade>.FindByIdAsync(string id, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }

        Task<bool> IRepository<TFacade>.UpdateAsync(TFacade entity, CancellationToken cancellationToken) {
            throw new NotImplementedException();
        }
    }
}
