// Copyright 2023-2025 Antonello Provenzano
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

using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Data {
	/// <summary>
	/// A repository that uses the memory of the process to store
	/// the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity managed by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key of the entity managed by the repository.
	/// </typeparam>
	/// <remarks>
	/// <para>
	/// <strong>Thread safety:</strong> This class is safe for concurrent use by
	/// multiple threads.  All read operations (
	/// <see cref="FindAsync"/>, <see cref="FindFirstAsync"/>,
	/// <see cref="FindAllAsync"/>, <see cref="ExistsAsync"/>,
	/// <see cref="CountAsync"/>, <see cref="GetPageAsync"/>,
	/// <see cref="Entities"/>) acquire a shared read lock so that they can
	/// execute in parallel.  All write operations (
	/// <see cref="AddAsync"/>, <see cref="AddRangeAsync"/>,
	/// <see cref="UpdateAsync"/>, <see cref="RemoveAsync"/>,
	/// <see cref="RemoveRangeAsync"/>) acquire an exclusive write lock.
	/// </para>
	/// <para>
	/// The reflection-based key-member discovery is performed exactly once per
	/// generic instantiation via a <see cref="Lazy{T}"/> guard, so it is safe
	/// regardless of how many threads call it simultaneously.
	/// </para>
	/// </remarks>
	public class InMemoryRepository<TEntity, TKey> :
		IRepository<TEntity, TKey>,
		IQueryableRepository<TEntity, TKey>,
		IPageableRepository<TEntity, TKey>,
		IFilterableRepository<TEntity, TKey>,
		ITrackingRepository<TEntity, TKey>,
		IDisposable
		where TEntity : class 
		where TKey : notnull {

		private SortedList<TKey, Entry> entities;
		private bool disposedValue;
		/// <summary>
		/// Guards <see cref="entities"/> for concurrent access.
		/// Multiple readers are allowed to run simultaneously;
		/// any writer holds exclusive access.
		/// </summary>
		private readonly ReaderWriterLockSlim _lock =
			new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

		/// <summary>
		/// Thread-safe, lazily-initialised cache of the <see cref="MemberInfo"/>
		/// decorated with <see cref="KeyAttribute"/> on <typeparamref name="TEntity"/>.
		/// </summary>
		private readonly Lazy<MemberInfo?> _idMember =
			new Lazy<MemberInfo?>(() =>
                    typeof(TEntity)
                        .GetMembers()
                        .FirstOrDefault(x => Attribute.IsDefined(x, typeof(KeyAttribute))),
				LazyThreadSafetyMode.ExecutionAndPublication);

		private readonly IFieldMapper<TEntity>? fieldMapper;

		/// <summary>
		/// Constructs the repository with the given list of
		/// initial entities.
		/// </summary>
		/// <param name="list">
		/// The list of entities to initialize the repository with.
		/// </param>
		/// <param name="fieldMapper">
		/// A service that maps a field by name to an expression that
		/// can select the field from an entity.
		/// </param>
		public InMemoryRepository(
			IEnumerable<TEntity>? list = null,
			IFieldMapper<TEntity>? fieldMapper = null) {
			entities = CopyList(list ?? Enumerable.Empty<TEntity>());
			this.fieldMapper = fieldMapper;
		}

		/// <summary>
		/// Destroys the instance of the repository.
		/// </summary>
		~InMemoryRepository() {
			Dispose(disposing: false);
		}

		IQueryable<TEntity> IQueryableRepository<TEntity, TKey>.AsQueryable() => Entities.AsQueryable();

		bool ITrackingRepository<TEntity, TKey>.IsTrackingChanges => true;

		/// <summary>
		/// Gets a point-in-time snapshot of all entities in the repository.
		/// </summary>
		/// <remarks>
		/// The snapshot is taken under a shared read lock; it is safe to call
		/// concurrently with any number of readers or with ongoing writes
		/// (readers will not see a partially-written state).
		/// </remarks>
		public virtual IReadOnlyList<TEntity> Entities {
			get {
				_lock.EnterReadLock();
				try {
					return entities.Values.Select(x => x.Entity).ToList().AsReadOnly();
				} finally {
					_lock.ExitReadLock();
				}
			}
		}

		private SortedList<TKey, Entry> CopyList(IEnumerable<TEntity> source) {
			var result = new SortedList<TKey, Entry>();
			foreach (var item in source) {
				var id = GetEntityId(item);
				if (id == null)
					throw new RepositoryException("The entity does not have an ID");

				result.Add(id, new Entry(item));
			}

			return result;
		}

		/// <inheritdoc/>
		public virtual TKey? GetEntityKey(TEntity entity) {
			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			return GetEntityId(entity);
		}

		private MemberInfo? DiscoverIdMember() => _idMember.Value;

		private static TKey? GetIdValue(MemberInfo memberInfo, TEntity entity) {
			if (memberInfo is PropertyInfo propertyInfo)
				return (TKey?) propertyInfo.GetValue(entity);
			if (memberInfo is FieldInfo fieldInfo)
				return (TKey?)fieldInfo.GetValue(entity);

			throw new NotSupportedException($"The member {memberInfo} is not supported");
		}

		private static void SetIdValue(MemberInfo memberInfo, TEntity entity, TKey value) {
			if (memberInfo is PropertyInfo propertyInfo) {
				propertyInfo.SetValue(entity, value);
			} else if (memberInfo is FieldInfo fieldInfo) {
				fieldInfo.SetValue(entity, value);
			} else {
				throw new NotSupportedException($"The member {memberInfo} is not supported");
			}
		}

		private void SetEntityId(TEntity entity, TKey value) {
			var member = DiscoverIdMember();
			if (member == null)
				throw new RepositoryException("The entity does not have an ID");

			SetIdValue(member, entity, value);
		}

		private TKey? GetEntityId(TEntity entity) {
			var member = DiscoverIdMember();
			if (member == null)
				return default;

			return GetIdValue(member, entity);
		}

		private TKey GenerateNewKey() {
			// TODO: make this generator configurable ...
			if (typeof(TKey) == typeof(Guid))
				return (TKey)(object)(Guid.NewGuid());
			if (typeof(TKey) == typeof(string))
				return (TKey)(object)Guid.NewGuid().ToString();

			throw new NotSupportedException($"The key type {typeof(TKey)} is not supported");
		}

		/// <inheritdoc/>
		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					var result = entities.Values.Select(x => x.Entity).AsQueryable().LongCount(filter);
					return Task.FromResult(result);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Could not count the entities", ex);
			}
		}

		/// <summary>
		/// Adds a single entity to the repository.
		/// </summary>
		/// <remarks>
		/// This method acquires an exclusive write lock before mutating the
		/// internal store, so it is safe to call concurrently from multiple threads.
		/// </remarks>
		/// <inheritdoc/>
		public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				var key = GetEntityId(entity);
				if (key == null)
				{
					key = GenerateNewKey();
					SetEntityId(entity, key);
				}

				_lock.EnterWriteLock();
				try {
					if (!entities.TryAdd(key, new Entry(entity)))
						throw new RepositoryException("An entity with the same ID already exists in the repository");
				} finally {
					_lock.ExitWriteLock();
				}

				return Task.CompletedTask;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not create the entity", ex);
			}
		}

		/// <summary>
		/// Adds a range of entities to the repository.
		/// </summary>
		/// <remarks>
		/// This method acquires an exclusive write lock before mutating the
		/// internal store, so it is safe to call concurrently from multiple threads.
		/// </remarks>
		/// <inheritdoc/>
		public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(entities, nameof(entities));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				// Assign keys outside the lock so key generation doesn't block readers
				var items = entities.Select(item => {
					var key = GenerateNewKey();
					SetEntityId(item, key);
					return (key, item);
				}).ToList();

				_lock.EnterWriteLock();
				try {
					foreach (var (key, item) in items) {
						this.entities.Add(key, new Entry(item));
					}
				} finally {
					_lock.ExitWriteLock();
				}

				return Task.CompletedTask;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not add the entities to the repository", ex);
			}
		}

		/// <summary>
		/// Removes a single entity from the repository.
		/// </summary>
		/// <remarks>
		/// This method acquires an exclusive write lock before mutating the
		/// internal store, so it is safe to call concurrently from multiple threads.
		/// </remarks>
		/// <inheritdoc/>
		public Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entityId = GetEntityId(entity);
				if (entityId == null)
					return Task.FromResult(false);

				_lock.EnterWriteLock();
				try {
					return Task.FromResult(this.entities.Remove(entityId));
				} finally {
					_lock.ExitWriteLock();
				}
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not delete the entity", ex);
			}
		}

		/// <summary>
		/// Removes a range of entities from the repository.
		/// </summary>
		/// <remarks>
		/// This method acquires an exclusive write lock before mutating the
		/// internal store, so it is safe to call concurrently from multiple threads.
		/// </remarks>
		/// <inheritdoc/>
		public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			ArgumentNullException.ThrowIfNull(entities, nameof(entities));

			try {
				var toRemove = entities.ToList();

				// Resolve all IDs before acquiring the lock
				var ids = toRemove.Select(entity => {
					var id = GetEntityId(entity);
					if (id == null)
						throw new RepositoryException("The entity does not have an ID");
					return id;
				}).ToList();

				_lock.EnterWriteLock();
				try {
					// verify all entities exist before removing any
					foreach (var id in ids) {
						if (!this.entities.ContainsKey(id))
							throw new RepositoryException("The entity is not in the repository");
					}

					foreach (var id in ids) {
						if (!this.entities.Remove(id))
							throw new RepositoryException("The entity was not removed from the repository");
					}
				} finally {
					_lock.ExitWriteLock();
				}

				return Task.CompletedTask;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not delete the entities", ex);
			}
		}

		/// <inheritdoc/>
		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					var result = entities.Values.Select(x => x.Entity).AsQueryable().Any(filter);
					return Task.FromResult(result);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Could not check if any entities exist in the repository", ex);
			}
		}


		/// <inheritdoc/>
		public Task<IList<TEntity>> FindAllAsync(IQuery query, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					var result = query.Apply(entities.Values.Select(x => x.Entity).AsQueryable()).ToList();
					return Task.FromResult<IList<TEntity>>(result);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Error while trying to find all the entities in the repository matching the filter", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindFirstAsync(IQuery query, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					var result = query.Apply(entities.Values.Select(x => x.Entity).AsQueryable()).FirstOrDefault();
					return Task.FromResult(result);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Error while searching for any entities in the repository matching the filter", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindOriginalAsync(TKey key, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					if (!entities.TryGetValue(key, out var entry))
						return Task.FromResult<TEntity?>(null);

					return Task.FromResult<TEntity?>(entry.Original);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Error while searching any entities with the given ID", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindAsync(TKey key, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					if (!entities.TryGetValue(key, out var entity))
						return Task.FromResult<TEntity?>(null);

					return Task.FromResult<TEntity?>(entity.Entity);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Error while searching any entities with the given ID", ex);
			}
		}

		/// <summary>
		/// Maps the given field name to an expression that can select
		/// a field from an entity.
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to map.
		/// </param>
		/// <returns>
		/// Returns an expression that can select the field from an entity.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the mapping is not supported by the repository.
		/// </exception>
		protected virtual Expression<Func<TEntity, object?>> MapField(string fieldName) {
			if (fieldMapper == null)
				throw new NotSupportedException("No field mapper was provided");

			return fieldMapper.MapField(fieldName);
		}

		/// <inheritdoc/>
		public Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> request, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				_lock.EnterReadLock();
				try {
					var entitySet = request.ApplyQuery(entities.Values.Select(x => x.Entity).AsQueryable());
					var itemCount = entitySet.Count();
					var items = entitySet
						.Skip(request.Offset)
						.Take(request.Size)
						.ToList();

					var result = new PageResult<TEntity>(request, itemCount, items);
					return Task.FromResult(result);
				} finally {
					_lock.ExitReadLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Unable to retrieve the page", ex);
			}
		}

		/// <summary>
		/// Updates an existing entity in the repository.
		/// </summary>
		/// <remarks>
		/// This method acquires an exclusive write lock before mutating the
		/// internal store, so it is safe to call concurrently from multiple threads.
		/// </remarks>
		/// <inheritdoc/>
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			try {
				var entityId = GetEntityId(entity);
				if (entityId == null)
					return Task.FromResult(false);

				_lock.EnterWriteLock();
				try {
					if (!entities.TryGetValue(entityId, out var entry))
						return Task.FromResult(false);

					entry.Update(entity);
					return Task.FromResult(true);
				} finally {
					_lock.ExitWriteLock();
				}
			} catch (Exception ex) when (ex is not RepositoryException) {
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}

		/// <summary>
		/// Disposes the repository and releases all the resources.
		/// </summary>
		/// <param name="disposing">
		/// The flag indicating if the repository is disposing.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					_lock.EnterWriteLock();
					try {
						entities.Clear();
					} finally {
						_lock.ExitWriteLock();
					}
					_lock.Dispose();
				}

				entities = null!;
				disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		class Entry {
			public Entry(TEntity entity) {
				Entity = entity;
				Original = Clone(entity);
			}

			public TEntity Original { get; private set; }

			public TEntity Entity { get; private set; }

			public void Update(TEntity entity) {
				Original = Clone(entity);
				Entity = entity;
			}

			private static TEntity Clone(TEntity entity) {
				var cloneMethod = typeof(TEntity)
					.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

				return (TEntity)cloneMethod!.Invoke(entity, new object[0])!;
			}
		}
	}
}
