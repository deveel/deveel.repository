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

using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Deveel.Data {
	/// <summary>
	/// A repository that uses the memory of the process to store
	/// the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity managed by the repository.
	/// </typeparam>
	public class InMemoryRepository<TEntity> : 
		IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository<TEntity>,
		IDisposable
		where TEntity : class {
		private SortedList<object, TEntity> entities;
		private bool disposedValue;
		private MemberInfo? idMember;
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
		/// Constructs the repository with the given list of
		/// initial entities for the given tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant that owns the entities.
		/// </param>
		/// <param name="list">
		/// A list of entities to initialize the repository with.
		/// </param>
		/// <param name="fieldMapper">
		/// A service that maps a field by name to an expression that
		/// can select the field from an entity.
		/// </param>
		protected InMemoryRepository(string tenantId, 
			IEnumerable<TEntity>? list = null,
			IFieldMapper<TEntity>? fieldMapper = null)
			: this(list, fieldMapper) {
			TenantId = tenantId;
		}

		/// <summary>
		/// Destroys the instance of the repository.
		/// </summary>
		~InMemoryRepository() {
			Dispose(disposing: false);
		}

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => Entities.AsQueryable();

		/// <summary>
		/// Gets the read-only list of entities in the repository.
		/// </summary>
		public virtual IReadOnlyList<TEntity> Entities => entities.Values.ToList().AsReadOnly();

		string? IMultiTenantRepository<TEntity>.TenantId => TenantId;

		/// <summary>
		/// Gets the identifier of the tenant that owns the entities,
		/// if any tenant is set.
		/// </summary>
		protected virtual string? TenantId { get; }

		private SortedList<object, TEntity> CopyList(IEnumerable<TEntity> source) {
			var result = new SortedList<object, TEntity>();
			foreach (var item in source) {
				var id = GetEntityId(item);
				if (id == null)
					throw new RepositoryException("The entity does not have an ID");

				result.Add(id, Clone(item));
			}

			return result;
		}

		private static TEntity Clone(TEntity entity) {
			var serializer = new DataContractSerializer(typeof(TEntity));
			using var stream = new MemoryStream();
			serializer.WriteObject(stream, entity);
			stream.Position = 0;
			return (TEntity)serializer.ReadObject(stream)!;
		}

		/// <inheritdoc/>
		public virtual object? GetEntityKey(TEntity entity) {
			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			return GetEntityId(entity);
		}

		private MemberInfo? DiscoverIdMember() {
			if (idMember == null) {
				idMember = typeof(TEntity)
					.GetMembers()
					.Where(x => Attribute.IsDefined(x, typeof(KeyAttribute))).FirstOrDefault();
			}

			return idMember;
		}

		private static object? GetIdValue(MemberInfo memberInfo, TEntity entity) {
			if (memberInfo is PropertyInfo propertyInfo)
				return propertyInfo.GetValue(entity);
			if (memberInfo is FieldInfo fieldInfo)
				return fieldInfo.GetValue(entity);

			throw new NotSupportedException($"The member {memberInfo} is not supported");
		}

		private static void SetIdValue(MemberInfo memberInfo, TEntity entity, object value) {
			if (memberInfo is PropertyInfo propertyInfo) {
				propertyInfo.SetValue(entity, value);
			} else if (memberInfo is FieldInfo fieldInfo) {
				fieldInfo.SetValue(entity, value);
			} else {
				throw new NotSupportedException($"The member {memberInfo} is not supported");
			}
		}

		private void SetEntityId(TEntity entity, object value) {
			var member = DiscoverIdMember();
			if (member == null)
				throw new RepositoryException("The entity does not have an ID");

			SetIdValue(member, entity, value);
		}

		private object? GetEntityId(TEntity entity) {
			var member = DiscoverIdMember();
			if (member == null)
				return null;

			return GetIdValue(member, entity);
		}

		/// <inheritdoc/>
		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var result = entities.Values.AsQueryable().LongCount(filter);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Could not count the entities", ex);
			}
		}

		/// <inheritdoc/>
		public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var id = Guid.NewGuid().ToString();
				SetEntityId(entity, id);

				entities.Add(id, Clone(entity));

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
					SetEntityId(item, id);

					this.entities.Add(id, Clone(item));
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
				var result = entities.Values.AsQueryable().Any(filter);
				return Task.FromResult(result);
			} catch(Exception ex) {
				throw new RepositoryException("Could not check if any entities exist in the repository", ex);
			}
		}


		/// <inheritdoc/>
		public Task<IList<TEntity>> FindAllAsync(IQuery query, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var result = query.Apply(entities.Values.AsQueryable()).ToList();

				return Task.FromResult<IList<TEntity>>(result);
			} catch (Exception ex) {

				throw new RepositoryException("Error while trying to find all the entities in the repository matching the filter", ex);
			}
		}

		/// <inheritdoc/>
		public Task<TEntity?> FindAsync(IQuery query, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var result = query.Apply(entities.Values.AsQueryable()).FirstOrDefault();
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
				var entitySet = request.ApplyQuery(entities.Values.AsQueryable());

				var itemCount = entitySet.Count();
				var items = entitySet
					.Skip(request.Offset)
					.Take(request.Size)
					.ToList();

				var result = new PageResult<TEntity>(request, itemCount,items);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to retrieve the page", ex) ;
			}
		}

		/// <inheritdoc/>
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			ArgumentNullException.ThrowIfNull(entity, nameof(entity));

			try {
				var entityId = GetEntityId(entity);
				if (entityId == null)
					return Task.FromResult(false);

				if (!entities.TryGetValue(entityId, out var existing))
					return Task.FromResult(false);

				entities[entityId] = entity;
				return Task.FromResult(true);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}

		internal static InMemoryRepository<TEntity> Create(string tenantId, IList<TEntity>? entities = null, IFieldMapper<TEntity>? fieldMapper = null)
			=> new InMemoryRepository<TEntity>(tenantId, entities, fieldMapper);

		/// <summary>
		/// Disposes the repository and releases all the resources
		/// </summary>
		/// <param name="disposing">
		/// The flag indicating if the repository is disposing.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					entities.Clear();
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
	}
}
