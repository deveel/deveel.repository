using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Data {
	class RepositoryWrapper<TEntity> : IFilterableRepository<TEntity> where TEntity : class {
		private readonly IEnumerable<TEntity> entities;

		public RepositoryWrapper(IEnumerable<TEntity> entities) {
			this.entities = entities ?? throw new ArgumentNullException(nameof(entities));
		}

		private bool IsMutable => (entities is IList<TEntity> list && !list.IsReadOnly) || 
			(entities is ICollection<TEntity> collection && !collection.IsReadOnly);

		public Type EntityType => typeof(TEntity);

		private void AssertMutable() {
			if (!IsMutable)
				throw new NotSupportedException("The repository is readonly");
		}

		private void AddEntity(TEntity entity) {
			if (entities is IList<TEntity> list) {
				list.Add(entity);
			} else if (entities is ICollection<TEntity> collection) {
				collection.Add(entity);
			} else {
				throw new NotSupportedException("The repository is readonly");
			}
		}

		string? IRepository.GetEntityId(object entity)
			=> GetEntityId((TEntity)entity);

		public string? GetEntityId(TEntity entity) {
			var idMembers = EntityType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
				.Where(x => x.Name == "Id" || x.Name == "ID")
				.ToList();

			if (idMembers.Count == 0)
				throw new NotSupportedException("The entity does not have an Id property");

			if (idMembers.Count > 1)
				throw new NotSupportedException("The entity has more than one Id property");

			var idMember = idMembers[0];

			if (idMember is PropertyInfo propertyInfo) {
				return (string?)propertyInfo.GetValue(entity);
			} else if (idMember is FieldInfo fieldInfo) {
				return (string?)fieldInfo.GetValue(entity);
			} else {
				throw new NotSupportedException("The entity Id is not supported");
			}
		}

		Task<string> IRepository.AddAsync(object entity, CancellationToken cancellationToken)
			=> AddAsync((TEntity)entity, cancellationToken);

		public Task<string> AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			AssertMutable();

			var id = SetId(entity);

			AddEntity(entity);

			return Task.FromResult<string>(id);
		}

		private string SetId(TEntity entity) {
			var idMembers = EntityType.GetMembers(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
				.Where(x => x.Name == "Id" || x.Name == "ID")
				.ToList();

			if (idMembers.Count == 0)
				throw new NotSupportedException("The entity does not have an Id property");

			if (idMembers.Count > 1)
				throw new NotSupportedException("The entity has more than one Id property");

			var idMember = idMembers[0];
			var entityId = Guid.NewGuid().ToString("N");

			if (idMember is PropertyInfo propertyInfo) {
				propertyInfo.SetValue(entity, entityId);
			} else if (idMember is FieldInfo fieldInfo) {
				fieldInfo.SetValue(entity, entityId);
			} else {
				throw new NotSupportedException("The entity Id is not supported");
			}

			return entityId;
		}

		Task<IList<string>> IRepository.AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken)
			=> AddRangeAsync(entities.Cast<TEntity>(), cancellationToken);

		public Task<IList<string>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			AssertMutable();

			var result = new List<string>();

			foreach (var entity in entities) {
				var id = SetId(entity);
				AddEntity(entity);

				result.Add(id);
			}

			return Task.FromResult<IList<string>>(result);
		}

		public Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			var entity = entities.FirstOrDefault(x => GetEntityId(x) == id);
			return Task.FromResult<TEntity?>(entity);
		}

		async Task<object?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		Task<bool> IRepository.RemoveAsync(object entity, CancellationToken cancellationToken)
			=> RemoveAsync((TEntity)entity, cancellationToken);

		public Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			AssertMutable();

			var id = GetEntityId(entity);
			if (id == null)
				return Task.FromResult(false);

			var found = entities.FirstOrDefault(x => GetEntityId(x) == id);
			if (found == null)
				return Task.FromResult(false);

			if (entities is IList<TEntity> list) {
				list.Remove(found);
			} else if (entities is ICollection<TEntity> collection) {
				collection.Remove(found);
			} else {
				throw new NotSupportedException("The repository is readonly");
			}

			return Task.FromResult(true);
		}

		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			AssertMutable();

			var id = GetEntityId(entity);
			if (id == null)
				return Task.FromResult(false);

			var found = entities.FirstOrDefault(x => GetEntityId(x) == id);
			if (found == null)
				return Task.FromResult(false);

			if (entities is IList<TEntity> list) {
				var index = list.IndexOf(found);
				list[index] = entity;
			} else if (entities is ICollection<TEntity> collection) {
				collection.Remove(found);
				collection.Add(entity);
			} else {
				throw new NotSupportedException("The repository is readonly");
			}

			return Task.FromResult(true);
		}

		Task<bool> IRepository.UpdateAsync(object entity, CancellationToken cancellationToken)
			=> UpdateAsync((TEntity)entity, cancellationToken);

		public Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			TEntity? result;

			if (entities is IQueryable<TEntity> queryable) {
				result = queryable.FirstOrDefault(filter.AsLambda<TEntity>());
			} else {
				result = entities.FirstOrDefault(filter.AsLambda<TEntity>().Compile());
			}

			return Task.FromResult(result);
		}

		public Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			IEnumerable<TEntity> result;

			if (entities is IQueryable<TEntity> queryable) {
				result = queryable.Where(filter.AsLambda<TEntity>());
			} else {
				result = entities.Where(filter.AsLambda<TEntity>().Compile());
			}

			return Task.FromResult<IList<TEntity>>(result.ToList());
		}

		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			bool result;

			if (entities is IQueryable<TEntity> queryable) {
				result = queryable.Any(filter.AsLambda<TEntity>());
			} else {
				result = entities.Any(filter.AsLambda<TEntity>().Compile());
			}

			return Task.FromResult(result);
		}

		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			long result;

			if (entities is IQueryable<TEntity> queryable) {
				result = queryable.LongCount(filter.AsLambda<TEntity>());
			} else {
				result = entities.LongCount(filter.AsLambda<TEntity>().Compile());
			}

			return Task.FromResult(result);
		}

		async Task<object?> IFilterableRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken) 
			=> await FindAsync(filter, cancellationToken);

		async Task<IList<object>> IFilterableRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> (await FindAllAsync(filter, cancellationToken)).Cast<object>().ToList();
	}
}
