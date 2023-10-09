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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;

namespace Deveel.Data {
    public class InMemoryRepository<TEntity> : 
		IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository<TEntity>
		where TEntity : class {
		// TODO: use a dictionary for faster access
		private readonly SortedList<string, TEntity> entities;
		private readonly IEntityFieldMapper<TEntity>? fieldMapper;

		public InMemoryRepository(IEnumerable<TEntity>? list = null, ISystemTime? systemTime = null, IEntityFieldMapper<TEntity>? fieldMapper = null) {
			entities = list == null ? new SortedList<string, TEntity>() : new SortedList<string, TEntity>(list.ToDictionary(x => (string)GetEntityKey(x)!, y => y));
			SystemTime = systemTime ?? Deveel.Data.SystemTime.Default;
			this.fieldMapper = fieldMapper;
		}

		protected InMemoryRepository(string tenantId, IEnumerable<TEntity>? list = null, ISystemTime? systemTime = null, IEntityFieldMapper<TEntity>? fieldMapper = null)
			: this(list, systemTime, fieldMapper) {
			TenantId = tenantId;
		}

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => entities.Values.AsQueryable();

		public IReadOnlyList<TEntity> Entities => entities.Values.ToList().AsReadOnly();

		string? IMultiTenantRepository<TEntity>.TenantId => TenantId;

		protected virtual string? TenantId { get; }

		protected ISystemTime SystemTime { get; }

		/// <inheritdoc/>
		public virtual object? GetEntityKey(TEntity entity) {
			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			return GetEntityId(entity);
		}

		private string? GetEntityId(TEntity entity) {
			if (!entity.TryGetMemberValue("Id", out object? idValue))
				return null;

			string? id;

			if (idValue is string v) {
				id = v;
			} else {
				id = Convert.ToString(idValue, CultureInfo.InvariantCulture);
			}

			// TODO: try some other members?

			return id;
		}

		/// <inheritdoc/>
		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				return Task.FromResult(entities.Values.AsQueryable().LongCount(lambda));
			} catch (Exception ex) {
				throw new RepositoryException("Could not count the entities", ex);
			}
		}

		/// <inheritdoc/>
		public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var id = Guid.NewGuid().ToString();
				if (!entity.TrySetMemberValue("Id", id))
					throw new RepositoryException("Unable to set the ID of the entity");

				if (entity is IHaveTimeStamp hasTime)
					hasTime.CreatedAtUtc = SystemTime.UtcNow;

				entities.Add(id, entity);

				return Task.CompletedTask;
			} catch (RepositoryException) {

				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not create the entity", ex);
			}
		}

		/// <inheritdoc/>
		public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				foreach (var item in entities) {
					var id = Guid.NewGuid().ToString();
					if (!item.TrySetMemberValue("Id", id))
						throw new RepositoryException("Unable to set the ID of the entity");

					if (item is IHaveTimeStamp hasTime)
						hasTime.CreatedAtUtc = SystemTime.UtcNow;

					this.entities.Add(id, item);
				}

				return Task.CompletedTask;
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not add the entities to the repository", ex);
			}
		}

		/// <inheritdoc/>
		public Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entityId = GetEntityId(entity);
				if (entityId == null)
					return Task.FromResult(false);

				return Task.FromResult(entities.Remove(entityId));
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not delete the entity", ex);
			}
		}

		/// <inheritdoc/>
		public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();
			
			ArgumentNullException.ThrowIfNull(entities, nameof(entities));

			try {
				var toRemove = entities.ToList();

				// if any of the entities is not in the list, we throw an exception
				foreach (var entity in toRemove) {
					var id = GetEntityId(entity);
					if (id == null)
						throw new RepositoryException("The entity does not have an ID");

					if (!this.entities.TryGetValue(id, out var existing))
						throw new RepositoryException("The entity is not in the repository");
				}

				foreach (var entity in toRemove) {
					var id = GetEntityId(entity);
					if (id == null)
						throw new RepositoryException("The entity does not have an ID");

					if (!this.entities.Remove(id))
						throw new RepositoryException("The entity was not removed from the repository");
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
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.Values.AsQueryable().Any(lambda);
				return Task.FromResult(result);
			} catch(Exception ex) {
				throw new RepositoryException("Could not check if any entities exist in the repository", ex);
			}
		}

		/// <inheritdoc/>
		public Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.Values.AsQueryable().Where(lambda).ToList();
				return Task.FromResult<IList<TEntity>>(result);
			} catch (Exception ex) {

				throw new RepositoryException("Error while trying to find all the entities in the repository matching the filter", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.Values.AsQueryable().FirstOrDefault(lambda);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching for any entities in the repository matching the filter", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindByKeyAsync(object key, CancellationToken cancellationToken = default) {
			ArgumentNullException.ThrowIfNull(key, nameof(key));

			cancellationToken.ThrowIfCancellationRequested();

			try {
                if (!(key is string s))
                    throw new RepositoryException("The key must be a string");

				if (!entities.TryGetValue(s, out var entity))
					return Task.FromResult<TEntity?>(null);
				
				return Task.FromResult<TEntity?>(entity);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching any entities with the given ID", ex);
			}
		}

		private Expression<Func<TEntity, object>> MapField(IFieldRef fieldRef) {
			if (fieldRef is ExpressionFieldRef<TEntity> expRef)
				return expRef.Expression;

			if (fieldRef is StringFieldRef fieldName)
				return MapField(fieldName.FieldName);

			throw new NotSupportedException();
		}

		protected virtual Expression<Func<TEntity, object>> MapField(string fieldName) {
			if (fieldMapper == null)
				throw new NotSupportedException("No field mapper was provided");

			return fieldMapper.Map(fieldName);
		}

		/// <inheritdoc/>
		public Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> request, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entitySet = entities.Values.AsQueryable();
				if (request.Filter != null)
					entitySet = request.Filter.Apply(entitySet);

				if (request.ResultSorts != null) {
					foreach (var sort in request.ResultSorts) {
						if (sort.Ascending) {
							entitySet = entitySet.OrderBy(MapField(sort.Field));
						} else {
							entitySet = entitySet.OrderByDescending(MapField(sort.Field));
						}
					}
				}

				var itemCount = entitySet.Count();
				var items = entitySet.Skip(request.Offset).Take(request.Size);

				var result = new PageResult<TEntity>(request, itemCount,items);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to retrieve the page", ex) ;
			}
		}

		/// <inheritdoc/>
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				if (!entity.TryGetMemberValue<string>("Id", out var entityId))
					return Task.FromResult(false);

				if (!entities.TryGetValue(entityId, out var _))
					return Task.FromResult(false);

				if (entity is IHaveTimeStamp hasTime)
					hasTime.UpdatedAtUtc = SystemTime.UtcNow;

				entities[entityId] = entity;
				return Task.FromResult(true);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}
		
		internal static InMemoryRepository<TEntity> Create(string tenantId, IList<TEntity>? entities = null, ISystemTime? systemTime = null, IEntityFieldMapper<TEntity>? fieldMapper = null)
			=> new InMemoryRepository<TEntity>(tenantId, entities, systemTime, fieldMapper);
	}
}
