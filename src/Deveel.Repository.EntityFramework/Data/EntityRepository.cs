// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Globalization;
using System.Linq.Expressions;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	/// <summary>
	/// A repository that uses an <see cref="DbContext"/> to access the data
	/// of the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the repository.
	/// </typeparam>
	public class EntityRepository<TEntity> : 
        IRepository<TEntity>,
        IFilterableRepository<TEntity>,
        IQueryableRepository<TEntity>,
		IPageableRepository<TEntity>,
        IDisposable
        where TEntity : class {
        private bool disposedValue;
        private IKey primaryKey;

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/>.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="logger">
		/// A logger used to log the operations of the repository.
		/// </param>
		/// <remarks>
		/// When the given <paramref name="context"/> implements the <see cref="IMultiTenantDbContext"/>
		/// the repository will use the tenant information to access the data.
		/// </remarks>
        public EntityRepository(DbContext context, ILogger<EntityRepository<TEntity>>? logger = null)
            : this(context, (context as IMultiTenantDbContext)?.TenantInfo, logger) {
        }

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/> for
		/// the given tenant.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="tenantInfo">
		/// The information about the tenant that the repository will use to access the data.
		/// </param>
		/// <param name="logger">
		/// The logger used to log the operations of the repository.
		/// </param>
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

		/// <summary>
		/// The destructor of the repository.
		/// </summary>
        ~EntityRepository() {
            Dispose(disposing: false);
        }

		/// <summary>
		/// Gets the instance of the <see cref="DbContext"/> used by the repository.
		/// </summary>
        protected DbContext Context { get; private set; }

		/// <summary>
		/// Gets the logger used by the repository.
		/// </summary>
        protected ILogger Logger { get; }

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> used by the repository to access the data.
		/// </summary>
        protected virtual DbSet<TEntity> Entities => Context.Set<TEntity>();

		/// <summary>
		/// Gets the information about the tenant that the repository is using to access the data.
		/// </summary>
        protected virtual ITenantInfo? TenantInfo { get; }

		/// <summary>
		/// Gets the identifier of the tenant that the repository is using to access the data.
		/// </summary>
        protected string? TenantId => TenantInfo?.Id;

		/// <summary>
		/// Assesses if the repository has been disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Thrown when the repository has been disposed.
		/// </exception>
        protected void ThrowIfDisposed() {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().Name); 
        }

		/// <summary>
		/// Converts the given string identifier to the type of the 
		/// primary key of the entity.
		/// </summary>
		/// <param name="id">
		/// The string representation of the identifier.
		/// </param>
		/// <returns>
		/// Returns the identifier converted to the type of the primary key
		/// of the entity.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when the given string is not a valid identifier for the entity.
		/// </exception>
		protected virtual object? ConvertEntityKey(string id) {
            var keyType = primaryKey.GetKeyType();

			if (keyType == null)
				// The entity has no primary key
				return null;

			if (Nullable.GetUnderlyingType(keyType) != null)
				keyType = Nullable.GetUnderlyingType(keyType);

			// These are the most common types of primary keys in SQL databases
			if (keyType == typeof(string))
				return id;
			if (keyType == typeof(Guid)) {
				if (!Guid.TryParse(id, out var guid))
					throw new ArgumentException($"The string '{id}' is not valid GUID");

				return guid;
			}
			if (keyType == typeof(int)) {
				if (!int.TryParse(id, out var intId))
					throw new ArgumentException($"The string '{id}' is not valid integer");

				return intId;
			}

			return Convert.ChangeType(id, keyType!, CultureInfo.InvariantCulture);
		}

		/// <inheritdoc/>
		public virtual string? GetEntityId(TEntity entity) {
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

		/// <inheritdoc/>
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
                throw new RepositoryException("Unknown error while trying to add an entity to the repository", ex);
            }
        }

		/// <inheritdoc/>
		public async Task<IList<string>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

			try {
				await Entities.AddRangeAsync(entities, cancellationToken);

				var count = await Context.SaveChangesAsync(true, cancellationToken);

				return entities.Select(x => GetEntityId(x)!).ToList();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unknown error while trying to add a range of entities to the repository", ex);
			}
        }

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		public async Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			try {
				return await Entities.FindAsync(new object?[] { ConvertEntityKey(id) }, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to find an entity in the repository because of an error", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

            try {
                var entityId = GetEntityId(entity)!;

                Logger.TraceUpdatingEntity(typeof(TEntity), entityId, TenantId);

				// TODO: is it there any better way to do this with EF?

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
                throw new RepositoryException("Unable to update the entity because of an error", ex);
            }
        }


        async Task<TEntity?> IFilterableRepository<TEntity>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAsync(AssertExpression(filter), cancellationToken);

        async Task<IList<TEntity>> IFilterableRepository<TEntity>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
            => await FindAllAsync(AssertExpression(filter), cancellationToken);

		async Task<bool> IFilterableRepository<TEntity>.ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await ExistsAsync(AssertExpression(filter), cancellationToken);

		/// <summary>
		/// Checks if the repository contains an entity that matches 
		/// the given filter.
		/// </summary>
		/// <param name="filter">
		/// The expression that defines the filter to apply to the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the repository contains at least one entity
		/// that matches the given filter, otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="RepositoryException"></exception>
		public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			try {
				return await Entities.AnyAsync(EnsureFilter(filter), cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to determine the existence of an entity", ex);
			}
		}

		/// <summary>
		/// Counts the number of entities in the repository that match 
		/// the given filter.
		/// </summary>
		/// <param name="filter">
		/// The expression that defines the filter to apply to the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns></returns>
        public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
            => await Entities.LongCountAsync(EnsureFilter(filter), cancellationToken);

		Task<long> IFilterableRepository<TEntity>.CountAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> CountAsync(AssertExpression(filter), cancellationToken);

		/// <summary>
		/// Finds the first entity in the repository that matches the given filter.
		/// </summary>
		/// <param name="filter">
		/// The expression that defines the filter to apply to the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the first entity that matches the given filter, or <c>null</c>
		/// if no entity is found.
		/// </returns>
        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			try {
				return await Entities.FirstOrDefaultAsync(EnsureFilter(filter), cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unknown error while trying to find an entity", ex);
			}
        }

        private static Expression<Func<TEntity, bool>> EnsureFilter(Expression<Func<TEntity, bool>>? filter) {
            if (filter == null)
                filter = e => true;

            return filter;
        }

        private Expression<Func<TEntity, bool>> AssertExpression(IQueryFilter filter) {
            if (filter == null || filter.IsEmpty())
                return x => true;

            if (!(filter is IExpressionQueryFilter exprFilter))
                throw new RepositoryException($"The filter of type {filter.GetType()} is not supported");

			try {
				return exprFilter.AsLambda<TEntity>();
			} catch (Exception ex) {
				throw new RepositoryException("Unable to trasnform the provided filter to an expression", ex);
			}
		}

		/// <summary>
		/// Finds all the entities in the repository that match the given filter.
		/// </summary>
		/// <param name="filter">
		/// The expression that defines the filter to apply to the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a list of entities that match the given filter.
		/// </returns>
		public async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
			try {
				return await Entities.Where(filter).ToListAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to list the entities", ex);
			}
        }

		/// <inheritdoc/>
        public IQueryable<TEntity> AsQueryable() => Entities.AsQueryable();

		/// <summary>
		/// Disposes the repository and frees all the resources used by it.
		/// </summary>
		/// <param name="disposing">
		/// Indicates if the repository is explicitly disposing.
		/// </param>
        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                Context = null;
                disposedValue = true;
            }
        }

		/// <inheritdoc/>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

		/// <inheritdoc/>
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
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Could not get the page of entities", ex);
			}
		}
	}
}
