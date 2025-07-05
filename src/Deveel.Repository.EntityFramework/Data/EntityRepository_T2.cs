﻿// Copyright 2023 Deveel AS
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System.Globalization;

namespace Deveel.Data
{
	/// <summary>
	/// A repository that uses an <see cref="DbContext"/> to access the data
	/// of the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key of the entity managed by the repository.
	/// </typeparam>
	public class EntityRepository<TEntity, TKey> :
		IRepository<TEntity, TKey>,
		IFilterableRepository<TEntity, TKey>,
		IQueryableRepository<TEntity, TKey>,
		IPageableRepository<TEntity, TKey>,
		ITrackingRepository<TEntity, TKey>,
		IDisposable
		where TEntity : class {
		private bool disposedValue;

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
		public EntityRepository(DbContext context, ILogger<EntityRepository<TEntity, TKey>>? logger = null) {
			Context = context ?? throw new ArgumentNullException(nameof(context));
			Logger = logger ?? NullLogger<EntityRepository<TEntity, TKey>>.Instance;

			var entityKey = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey();

			if (entityKey == null)
				throw new RepositoryException($"The model of the entity '{typeof(TEntity)}' has no primary key configured");
			if (entityKey.Properties.Count > 1)
				throw new NotSupportedException("The repository does not support entities with composite primary keys");

			//if (entityKey.Properties[0].ClrType != typeof(TKey))
			//	throw new RepositoryException($"The primary key of the entity '{typeof(TEntity)}' is not of type '{typeof(TKey)}'");

			PrimaryKey = entityKey;
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
		/// Gets a reference to the primary key of the entity.
		/// </summary>
		protected IKey PrimaryKey { get; }

		/// <summary>
		/// Gets the logger used by the repository.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> used by the repository to access the data.
		/// </summary>
		protected virtual DbSet<TEntity> Entities => Context.Set<TEntity>();

		/// <summary>
		/// Gets a value indicating if the repository is tracking the changes
		/// to the entities returned by the queries.
		/// </summary>
		protected bool IsTrackingChanges => Entities.Local != null ||
			Context.ChangeTracker.QueryTrackingBehavior != QueryTrackingBehavior.NoTracking;

		bool ITrackingRepository<TEntity, TKey>.IsTrackingChanges => IsTrackingChanges;

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
		/// Converts the given value to the type of the 
		/// primary key of the entity.
		/// </summary>
		/// <param name="key">
		/// The key that represents the identifier of the entity.
		/// </param>
		/// <returns>
		/// Returns the identifier converted to the type of the primary key
		/// of the entity.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when the given string is not a valid identifier for the entity.
		/// </exception>
		protected virtual TKey? ConvertEntityKey(TKey? key) {
			if (key == null)
				return default;

			var keyType = PrimaryKey.GetKeyType();

			if (keyType == null)
				// The entity has no primary key
				return default;

			if (Nullable.GetUnderlyingType(keyType) != null)
				keyType = Nullable.GetUnderlyingType(keyType);

			var valueType = key.GetType();

			// These are the most common types of primary keys in SQL databases
			if (keyType == valueType)
				return key;

			if (key is string s) {
				if (keyType == typeof(Guid) && Guid.TryParse(s, out var guid))
					return (TKey)(object) guid;

				if (typeof(IConvertible).IsAssignableFrom(keyType))
					return (TKey) Convert.ChangeType(key, keyType, CultureInfo.InvariantCulture);
			}

			throw new ArgumentException($"The given key '{key}' is not a valid identifier for the entity '{typeof(TEntity)}'");
		}

		/// <inheritdoc/>
		public virtual TKey? GetEntityKey(TEntity entity) {
			var props = PrimaryKey.Properties.ToList();
			if (props.Count > 1)
				throw new RepositoryException($"The entity '{typeof(TEntity)}' has more than one property has primary key");

			var getter = props[0].GetGetter();
			return (TKey?) getter.GetClrValue(entity);
		}

		/// <summary>
		/// A method that is invoked when an entity is 
		/// being added to the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity that is being added to the repository.
		/// </param>
		/// <returns>
		/// Returns the entity that will be added to the repository.
		/// </returns>
		protected virtual TEntity OnAddingEntity(TEntity entity)
		{
			return entity;
		}

		/// <inheritdoc/>
		public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			Logger.TraceCreatingEntity(typeof(TEntity));

			try {
				Entities.Add(OnAddingEntity(entity));

				var count = await Context.SaveChangesAsync(cancellationToken);

				if (count > 1) {
					// TODO: warn about this...
				}

				var key = GetEntityKey(entity)!;

				Logger.LogEntityCreated(typeof(TEntity), key);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unknown error while trying to add an entity to the repository", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				var toAdd = entities.Select(OnAddingEntity).ToList();

				await Entities.AddRangeAsync(toAdd, cancellationToken);

				var count = await Context.SaveChangesAsync(true, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unknown error while trying to add a range of entities to the repository", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			try {
				var entityId = GetEntityKey(entity)!;

				Logger.TraceDeletingEntity(typeof(TEntity), entityId);

				var entry = Context.Entry(entity);
				if (entry == null) {
					Logger.WarnEntityNotFound(typeof(TEntity), entityId);
					return false;
				} else if (entry.State == EntityState.Detached) {
					var existing = await FindAsync(entityId, cancellationToken);
					if (existing == null) {
						Logger.WarnEntityNotFound(typeof(TEntity), entityId);
						return false;
					}

					entry = Context.Entry(existing);
				}

				entry.State = EntityState.Deleted;

				var count = await Context.SaveChangesAsync(cancellationToken);

				// It cannot be just one change, when the entity has related entities
				var deleted = count > 0;

				if (deleted) {
					Logger.LogEntityDeleted(typeof(TEntity), entityId);
				} else {
					Logger.WarnEntityNotDeleted(typeof(TEntity), entityId);
				}

				return deleted;
			} catch (DbUpdateConcurrencyException ex) {
				throw new RepositoryException("Concurrency problem while deleting the entity", ex);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to delete the entity", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				foreach (var item in entities) {
					var entityId = GetEntityKey(item);
					if (entityId == null)
						throw new RepositoryException("One of the entities has no primary key configured");

					var entry = Context.Entry(item);
					if (entry.State == EntityState.Detached) {
						var existing = await FindAsync(entityId, cancellationToken);
						if (existing == null) {
							Logger.WarnEntityNotFound(typeof(TEntity), entityId);
							throw new RepositoryException($"The entity with the key '{entityId}' was not found in the repository");
						}

						entry = Context.Entry(existing);
					}

					entry.State = EntityState.Deleted;
				}

				//Entities.RemoveRange(entities);

				await Context.SaveChangesAsync(true, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unknown error while trying to remove a range of entities from the repository", ex);
			}
		}


		/// <summary>
		/// A callback invoked when an entity is found by its key.
		/// </summary>
		/// <param name="key">
		/// The key used to find the entity.
		/// </param>
		/// <param name="entity"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected virtual Task<TEntity> OnEntityFoundByKeyAsync(TKey key, TEntity entity, CancellationToken cancellationToken = default) {
			return Task.FromResult(entity);
		}

		/// <inheritdoc/>
		public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			try {
				var entityId = GetEntityKey(entity)!;

				Logger.TraceUpdatingEntity(typeof(TEntity), entityId);

				EntityEntry<TEntity>? entry = null;

				if (!IsTrackingChanges) {
					var existing = await FindAsync(entityId, cancellationToken);
					if (existing == null) {
						Logger.WarnEntityNotFound(typeof(TEntity), entityId);
						return false;
					}

					entry = Context.Entry(existing);
				} else {
					entry = Context.Entry(entity);

					if (entry.State == EntityState.Detached) {
						var existing = await FindAsync(entityId, cancellationToken);
						if (existing == null) {
							Logger.WarnEntityNotFound(typeof(TEntity), entityId);
							return false;
						}

						entry = Context.Entry(existing);
					}
				}

				entry.CurrentValues.SetValues(entity);
				entry.State = EntityState.Modified;

				var count = await Context.SaveChangesAsync(cancellationToken);

				var updated = count > 0;

				if (updated) {
					Logger.LogEntityUpdated(typeof(TEntity), entityId);
				} else {
					Logger.WarnEntityNotUpdated(typeof(TEntity), entityId);
				}

				return updated;
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to update the entity because of an error", ex);
			}
		}


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
		public virtual async Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			try {
				var query = AsQueryable();
				if (filter != null) {
					query = filter.Apply(query);
				}

				return await query.AnyAsync(cancellationToken);
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
		public virtual async Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			try {
				var query = AsQueryable();
				if (filter != null) {
					query = filter.Apply(query);
				}

				return await query.LongCountAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to count the entities", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<TEntity?> FindFirstAsync(IQuery query, CancellationToken cancellationToken = default) {
			try {
				var result = query.Apply(AsQueryable());

				return await result.FirstOrDefaultAsync(cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unknown error while trying to find an entity", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<TEntity?> FindAsync(TKey key, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				var result = await Entities.FindAsync(new object?[] { ConvertEntityKey(key) }, cancellationToken);
				if (result == null)
					return result;

				result = await OnEntityFoundByKeyAsync(key, result, cancellationToken);

				return result;
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to find an entity in the repository because of an error", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<TEntity?> FindOriginalAsync(TKey key, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				var result = await Entities.FindAsync(new object?[] { ConvertEntityKey(key) }, cancellationToken);
				if (result == null)
					return result;

				var entry = Context.Entry(result);
				
				//TODO: find a way to get the original values
				//      of related entities...

				return (TEntity) entry.OriginalValues.ToObject();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to find an entity int he repository because of an error", ex);
			}
		}

		/// <inheritdoc/>
		public virtual async Task<IList<TEntity>> FindAllAsync(IQuery query, CancellationToken cancellationToken = default) {
			try {
				var result = query.Apply(AsQueryable());
				return await result.ToListAsync(cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Unable to list the entities", ex);
			}
		}

		/// <inheritdoc/>
		public virtual IQueryable<TEntity> AsQueryable() => Entities.AsQueryable();

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
		public virtual async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> request, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			try {
				var querySet = request.ApplyQuery(AsQueryable());
				var total = await querySet.CountAsync(cancellationToken);

				var items = await querySet
					.Skip(request.Offset)
					.Take(request.Size)
					.ToListAsync(cancellationToken);

				return new PageResult<TEntity>(request, total, items);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex, typeof(TEntity));
				throw new RepositoryException("Could not get the page of entities", ex);
			}
		}
	}
}
