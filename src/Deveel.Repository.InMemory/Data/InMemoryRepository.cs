﻿using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Deveel.Data
{
	public class InMemoryRepository<TEntity> : IRepository<TEntity>, IQueryableRepository<TEntity>, IPageableRepository<TEntity>
		where TEntity : class, IEntity {
		private readonly List<TEntity> entities;
		private readonly IEntityFieldMapper<TEntity>? fieldMapper;

		public InMemoryRepository(IEnumerable<TEntity>? list = null, IEntityFieldMapper<TEntity>? fieldMapper = null) {
			entities = list == null ? new List<TEntity>() : new List<TEntity>(list);
			this.fieldMapper = fieldMapper;
		}

		bool IRepository.SupportsFilters => true;

		Type IRepository.EntityType => typeof(TEntity);

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => entities.AsQueryable();

		public IReadOnlyList<TEntity> Entities => entities.AsReadOnly();

		private static TEntity Assert(IEntity entity) {
			if (entity == null)
				throw new ArgumentNullException(nameof(entity));

			if (!(entity is TEntity t))
				throw new ArgumentException($"The type '{entity.GetType()}' is not assignable from {typeof(TEntity)}", nameof(entity));

			return t;
		}

		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				return Task.FromResult(entities.AsQueryable().LongCount(lambda));
			} catch (Exception ex) {
				throw new RepositoryException("Could not count the entities", ex);
			}
		}

		public Task<string> CreateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var id = Guid.NewGuid().ToString();
				if (!entity.TrySetMemberValue("Id", id))
					throw new RepositoryException("Unable to set the ID of the entity");

				entities.Add(entity);

				return Task.FromResult(id);
			} catch (RepositoryException) {

				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not create the entity", ex);
			}
		}

		public Task<IList<string>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var result = new List<string>();

				foreach (var item in entities) {
					var id = Guid.NewGuid().ToString();
					if (!item.TrySetMemberValue("Id", id))
						throw new RepositoryException("Unable to set the ID of the entity");

					this.entities.Add(item);

					result.Add(id);
				}

				return Task.FromResult<IList<string>>(result);
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not add the entities to the repository", ex);
			}
		}

		Task<IList<string>> IRepository<TEntity>.CreateAsync(IDataTransaction transaction, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
			=> throw new NotSupportedException();

		Task<string> IRepository<TEntity>.CreateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		Task<string> IRepository.CreateAsync(IEntity entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<IList<string>> IRepository.CreateAsync(IEnumerable<IEntity> entities, CancellationToken cancellationToken)
			=> CreateAsync(entities.Select(Assert), cancellationToken);

		Task<IList<string>> IRepository.CreateAsync(IDataTransaction transaction, IEnumerable<IEntity> entities, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		Task<string> IRepository.CreateAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		public Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				return Task.FromResult(entities.Remove(entity));
			} catch (RepositoryException) {

				throw;
			} catch(Exception ex) {
				throw new RepositoryException("Could not delete the entity", ex);
			}
		}

		Task<bool> IRepository<TEntity>.DeleteAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		Task<bool> IRepository.DeleteAsync(IEntity entity, CancellationToken cancellationToken) 
			=> DeleteAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository.DeleteAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().Any(lambda);
				return Task.FromResult(result);
			} catch(Exception ex) {
				throw new RepositoryException("Could not check if any entities exist in the repository", ex);
			}
		}

		public Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().Where(lambda).ToList();
				return Task.FromResult<IList<TEntity>>(result);
			} catch (Exception ex) {

				throw new RepositoryException("Error while trying to find all the entities in the repository matching the filter", ex);
			}
		}

		public Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var lambda = filter.AsLambda<TEntity>();
				var result = entities.AsQueryable().FirstOrDefault(lambda);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching for any entities in the repository matching the filter", ex);
			}
		}

		public Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			if (string.IsNullOrWhiteSpace(id)) 
				throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));

			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entity = entities.FirstOrDefault(x => x.Id == id);
				return Task.FromResult(entity);
			} catch (Exception ex) {
				throw new RepositoryException("Error while searching any entities with the given ID", ex);
			}
		}
		Task<IEntity?> IRepository.FindByIdAsync(IDataTransaction transaction, string id, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");

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

		public Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var entitySet = entities.AsQueryable();
				if (request.Filter != null)
					entitySet = entitySet.Where(request.Filter);

				if (request.SortBy != null) {
					foreach (var sort in request.SortBy) {
						if (sort.Ascending) {
							entitySet = entitySet.OrderBy(MapField(sort.Field));
						} else {
							entitySet = entitySet.OrderByDescending(MapField(sort.Field));
						}
					}
				}

				var itemCount = entitySet.Count();
				var items = entitySet.Skip(request.Offset).Take(request.Size);

				var result = new RepositoryPage<TEntity>(request, itemCount,items);
				return Task.FromResult(result);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to retrieve the page", ex) ;
			}
		}


		async Task<RepositoryPage> IPageableRepository.GetPageAsync(RepositoryPageRequest request, CancellationToken cancellationToken) {
			var pageRequest = new RepositoryPageRequest<TEntity>(request.Page, request.Size) {
				Filter = request.Filter?.AsLambda<TEntity>()
			};

			var result = await GetPageAsync(pageRequest, cancellationToken);

			return new RepositoryPage(request, result.TotalItems, result.Items?.Cast<IEntity>());
		}
		
		public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			cancellationToken.ThrowIfCancellationRequested();

			try {
				var oldIndex = entities.FindIndex(x => x.Id == entity.Id);
				if (oldIndex < 0)
					return Task.FromResult(false);

				entities[oldIndex] = entity;
				return Task.FromResult(true);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}
		
		Task<bool> IRepository<TEntity>.UpdateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");
		
		Task<bool> IRepository.UpdateAsync(IEntity entity, CancellationToken cancellationToken) 
			=> UpdateAsync(Assert(entity), cancellationToken);
		
		Task<bool> IRepository.UpdateAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported for in-memory repositories");
		
		async Task<IList<IEntity>> IRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			return (await FindAllAsync(filter, cancellationToken)).Cast<IEntity>().ToList();
		}
		
		async Task<IEntity?> IRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> await FindAsync(filter, cancellationToken);
		
		async Task<IEntity?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);
	}
}
