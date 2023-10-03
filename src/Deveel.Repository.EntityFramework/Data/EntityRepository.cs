using System.Globalization;
using System.Linq.Expressions;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	public class EntityRepository<TEntity> : 
        IRepository<TEntity>,
        IFilterableRepository<TEntity>,
        IQueryableRepository<TEntity>,
		IPageableRepository<TEntity>,
        IDisposable
        where TEntity : class {
        private bool disposedValue;

        private IKey primaryKey;

        public EntityRepository(DbContext context, ILogger<EntityRepository<TEntity>>? logger = null)
            : this(context, (context as IMultiTenantDbContext)?.TenantInfo, logger) {
        }

        public EntityRepository(DbContext context, ITenantInfo? tenantInfo, ILogger<EntityRepository<TEntity>>? logger = null)
            : this(context, tenantInfo, (ILogger?) logger) {
        }

        internal EntityRepository(DbContext context, ITenantInfo? tenantInfo, ILogger? logger = null) {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger ?? NullLogger.Instance;

            var entityKey = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();

            if (entityKey == null)
                throw new RepositoryException($"The model of the entity '{typeof(TEntity)}' has no primary key configured");

            primaryKey = entityKey;

            TenantInfo = tenantInfo;
        }

        ~EntityRepository() {
            Dispose(disposing: false);
        }

        protected DbContext Context { get; private set; }

        protected ILogger Logger { get; }

        protected DbSet<TEntity> Entities => Context.Set<TEntity>();

        protected virtual ITenantInfo? TenantInfo { get; }

        protected string? TenantId => TenantInfo?.Id;

        protected void ThrowIfDisposed() {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().Name); 
        }

		string? IRepository<TEntity>.GetEntityId(TEntity entity)
			=> GetEntityId(entity);

		protected virtual object? GetEntityId(string id) {
            //var model = Context.Model.FindEntityType(typeof(TEntity));
            //if (model == null)
            //	throw new RepositoryException($"The entity type '{typeof(TEntity)}' was not mapped");

            //var key = model.FindPrimaryKey();
            //if (key == null)
            //	throw new RepositoryException($"The model of the entity '{typeof(TEntity)}' has no primary key configured");

            //var keyType = key.GetKeyType();

            var keyType = primaryKey.GetKeyType();

			if (keyType == typeof(string))
				return id;
			if (keyType == typeof(Guid)) {
				if (!Guid.TryParse(id, out var guid))
					throw new ArgumentException($"The string '{id}' is not valid GUID");

				return guid;
			}

			return Convert.ChangeType(id, keyType, CultureInfo.InvariantCulture);
		}

		protected virtual string? GetEntityId(TEntity entity) {
			//var model = Context.Model.FindEntityType(typeof(TEntity));
			//if (model == null)
			//	throw new RepositoryException($"The entity type '{typeof(TEntity)}' was not mapped");

			//var key = model.FindPrimaryKey();
			//if (key == null)
			//	throw new RepositoryException($"The model of the entity '{typeof(TEntity)}' has no primary key configured");

			var props = primaryKey.Properties.ToList();
			if (props.Count > 1)
				throw new RepositoryException($"The entity '{typeof(TEntity)}' has more than one property has primary key");

			var getter = props[0].GetGetter();
			var value = getter.GetClrValue(entity);

			if (value == null)
				return null;

			if (!(value is string id)) {
				id = Convert.ToString(value, CultureInfo.InvariantCulture)!;
			}

			return id;
		}

		public async Task<string> AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

            if (entity is null) throw new ArgumentNullException(nameof(entity));

            Logger.TraceCreatingEntity(typeof(TEntity), TenantId);

            try {
                Entities.Add(entity);
                var count = await Context.SaveChangesAsync(cancellationToken);

				if (count > 1) {
					// TODO: warn about this...
				}

				var id = GetEntityId(entity)!;

                Logger.LogEntityCreated(typeof(TEntity), id, TenantId);

                return id;
            } catch (Exception ex) {
                Logger.LogUnknownError(ex, typeof(TEntity));
                throw;
            }
        }

        public async Task<IList<string>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

			try {
				await Entities.AddRangeAsync(entities, cancellationToken);

				var count = await Context.SaveChangesAsync(true, cancellationToken);

				return entities.Select(x => GetEntityId(x)!).ToList();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Could not create the entities", ex);
			}
        }

        private static TEntity AssertEntity(object entity) {
            if (!(entity is TEntity dataEntity))
                throw new ArgumentException($"The entity is not assignable from '{typeof(TEntity)}'");

            return dataEntity;
        }

        public async Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			if (entity is null) throw new ArgumentNullException(nameof(entity));

			try {
                var entityId = GetEntityId(entity)!;

                Logger.TraceDeletingEntity(typeof(TEntity), entityId, TenantId);

                var existing = await FindByIdAsync(entityId, cancellationToken);
                if (existing == null) {
                    Logger.WarnEntityNotFound(typeof(TEntity), entityId, TenantId);
                    return false;
                }

				var entry = Context.Entry(entity);
                if (entry == null) {
                    Logger.WarnEntityNotFound(typeof(TEntity), entityId, TenantId);
                    return false;
                }

				entry.State = EntityState.Deleted;

				var count = await Context.SaveChangesAsync(cancellationToken);

				// It cannot be just one change, when the entity has related entities
			   var deleted = count > 0;

                if (deleted) {
                    Logger.LogEntityDeleted(typeof(TEntity), entityId, TenantId);
                } else {
                    Logger.WarnEntityNotDeleted(typeof(TEntity), entityId, TenantId);
                }

                return deleted;
			} catch(DbUpdateConcurrencyException ex) {
				throw new RepositoryException("Concurrency problem while deleting the entity", ex);
			} catch (Exception ex) {
                Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to delete the entity", ex);
			}
        }

        public async Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
            => await Entities.FindAsync(new object?[] { GetEntityId(id) }, cancellationToken);

        public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

            try {
                var entityId = GetEntityId(entity)!;

                Logger.TraceUpdatingEntity(typeof(TEntity), entityId, TenantId);

                var existing = await FindByIdAsync(entityId, cancellationToken);
                if (existing == null) {
                    Logger.WarnEntityNotUpdated(typeof(TEntity), entityId, TenantId);
                    return false;
                }

                var update = Entities.Update(entity);
                if (update.State != EntityState.Modified) {
                    Logger.WarnEntityNotUpdated(typeof(TEntity), entityId, TenantId);
                    return false;
                }

                var count = await Context.SaveChangesAsync(cancellationToken);

                var updated = count > 0;

                if (updated) {
                    Logger.LogEntityUpdated(typeof(TEntity), entityId, TenantId);
                } else {
                    Logger.WarnEntityNotUpdated(typeof(TEntity), entityId, TenantId);
                }

                return updated;
            } catch (Exception ex) {
                Logger.LogUnknownError(ex, typeof(TEntity));
                throw new RepositoryException("Unable to update the entity", ex);
            }
        }


        async Task<TEntity?> IFilterableRepository<TEntity>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(AssertExpression(filter), cancellationToken);

        async Task<IList<TEntity>> IFilterableRepository<TEntity>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAllAsync(AssertExpression(filter), cancellationToken);

		async Task<bool> IFilterableRepository<TEntity>.ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await ExistsAsync(AssertExpression(filter), cancellationToken);

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
            => await Entities.AnyAsync(EnsureFilter(filter), cancellationToken);

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
            => await Entities.LongCountAsync(EnsureFilter(filter), cancellationToken);

		Task<long> IFilterableRepository<TEntity>.CountAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> CountAsync(AssertExpression(filter), cancellationToken);

        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
            return await Entities.FirstOrDefaultAsync(EnsureFilter(filter), cancellationToken);
        }

        private static Expression<Func<TEntity, bool>> EnsureFilter(Expression<Func<TEntity, bool>>? filter) {
            if (filter == null)
                filter = e => true;

            return filter;
        }

        private Expression<Func<TEntity, bool>> AssertExpression(IQueryFilter filter) {
            if (filter == null || filter.IsEmpty())
                return e => true;

            if (!(filter is ExpressionQueryFilter<TEntity> queryFilter))
                throw new RepositoryException($"The filter of type {filter.GetType()} is not supported");

            return queryFilter.Expression;
        }

        public async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
            return await Entities.Where(filter).ToListAsync(cancellationToken);
        }

        public IQueryable<TEntity> AsQueryable() {
            return Entities.AsQueryable();
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {

                Context = null;
                disposedValue = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

		public async Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				var querySet = Entities.AsQueryable();
				if (request.Filter != null) {
					if (request.Filter is CombinedQueryFilter combined) {
						foreach (var filter in combined.Filters) {
							querySet = filter.Apply(querySet);
						}
					} else {
						querySet = request.Filter.Apply(querySet);
					}
				}

				if (request.ResultSorts != null) {
					foreach (var sort in request.ResultSorts) {
						querySet = sort.Apply(querySet);
					}
				}

				var total = await querySet.CountAsync(cancellationToken);

				var items = await querySet.Skip(request.Offset).Take(request.Size).ToListAsync(cancellationToken);

				return new RepositoryPage<TEntity>(request, total, items);
			} catch (Exception ex) {

				throw new RepositoryException("Could not get the page of entities", ex);
			}
		}
	}
}
