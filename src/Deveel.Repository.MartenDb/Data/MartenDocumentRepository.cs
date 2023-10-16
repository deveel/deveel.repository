using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

using Marten;
using Marten.Schema;

using Microsoft.Extensions.Options;

namespace Deveel.Data {
	public class MartenDocumentRepository<TEntity> : IRepository<TEntity>,
		IFilterableRepository<TEntity>,
		IPageableRepository<TEntity>
		where TEntity : class {
		private readonly IDocumentStore store;
		private MemberInfo? keyMember;
		private PropertyInfo? versionProperty;

		public MartenDocumentRepository(IDocumentStore store, IOptions<MartenDocumentOptions>? options = null) {
			this.store = store;
			Options = options?.Value ?? new MartenDocumentOptions();
		}

		protected MartenDocumentOptions Options { get; }

		protected void ThrowIfReadOnly() {
			if (Options.ReadOnly)
				throw new RepositoryException("The repository is read-only");
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

		public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfReadOnly();

			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				session.Insert(entity);

				await session.SaveChangesAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while adding an entity", ex);
			} finally {
				session?.Dispose();
			}
		}

		public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfReadOnly();

			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				session.Insert(entities);

				await session.SaveChangesAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while adding a range of entities", ex);
			} finally {
				session?.Dispose();
			}
		}

		Task<long> IFilterableRepository<TEntity>.CountAsync(IQueryFilter filter, CancellationToken cancellationToken) 
			=> CountAsync(AssertExpression(filter), cancellationToken);

		public async Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				IQueryable<TEntity> querySet = session.Query<TEntity>();
				if (filter != null)
					querySet = querySet.Where(filter);
				
				return await querySet.CountAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while counting entities", ex);
			} finally {
				session?.Dispose();
			}
		}

		Task<bool> IFilterableRepository<TEntity>.ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken)
			=> ExistsAsync(AssertExpression(filter), cancellationToken);

		public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				IQueryable<TEntity> querySet = session.Query<TEntity>();
				if (filter != null)
					querySet = querySet.Where(filter);

				return await querySet.AnyAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while checking for entities", ex);
			} finally {
				session?.Dispose();
			}
		}

		Task<IList<TEntity>> IFilterableRepository<TEntity>.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) 
			=> FindAllAsync(AssertExpression(filter), cancellationToken);

		public async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				IQueryable<TEntity> querySet = session.Query<TEntity>();
				if (filter != null)
					querySet = querySet.Where(filter);

				return (await querySet.ToListAsync(cancellationToken)).ToList();
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while finding entities", ex);
			} finally {
				session?.Dispose();
			}
		}

		Task<TEntity?> IFilterableRepository<TEntity>.FindAsync(IQueryFilter filter, CancellationToken cancellationToken) 
			=> FindAsync(AssertExpression(filter), cancellationToken);

		public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				IQueryable<TEntity> querySet = session.Query<TEntity>();
				if (filter != null)
					querySet = querySet.Where(filter);

				return await querySet.FirstOrDefaultAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while finding an entity", ex);
			} finally {
				session?.Dispose();
			}
		}

		public async Task<TEntity?> FindByKeyAsync(object key, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				if (key is string keyString)
					return await session.LoadAsync<TEntity>(keyString, cancellationToken);
				if (key is Guid keyGuid)
					return await session.LoadAsync<TEntity>(keyGuid, cancellationToken);
				if (key is int keyInt)
					return await session.LoadAsync<TEntity>(keyInt, cancellationToken);

				throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while finding an entity by key", ex);
			} finally {
				session?.Dispose();
			}
		}

		public virtual object? GetEntityKey(TEntity entity) {
			if (keyMember == null) {
				var docType = store.Options.FindOrResolveDocumentType(typeof(TEntity));

				if (docType == null)
					throw new RepositoryException($"The entity '{typeof(TEntity).Name}' is not a valid document type");

				keyMember = docType.IdMember;

				if (keyMember == null)
					throw new RepositoryException($"The entity '{typeof(TEntity).Name}' does not have an identity property");
			}

			if (keyMember is PropertyInfo property)
				return property.GetValue(entity);
			if (keyMember is FieldInfo field)
				return field.GetValue(entity);

			throw new RepositoryException($"The key member '{keyMember.Name}' is not a valid property or field");
		}

		public async Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfReadOnly();

			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				var key = GetEntityKey(entity);

				if (key == null)
					return false;

				if (key is string keyString) {
					var existing = await session.LoadAsync<TEntity>(keyString, cancellationToken);
					if (existing == null)
						return false;

					session.Delete<TEntity>(keyString);
				} else if (key is Guid keyGuid) {
					var existing = await session.LoadAsync<TEntity>(keyGuid, cancellationToken);
					if (existing == null)
						return false;

					session.Delete<TEntity>(keyGuid);
				} else if (key is int keyInt) {
					var existing = await session.LoadAsync<TEntity>(keyInt, cancellationToken);
					if (existing == null)
						return false;

					session.Delete<TEntity>(keyInt);
				} else {
					throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
				}

				session.SaveChanges();

				return true;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while removing an entity", ex);
			} finally {
				session?.Dispose();
			}
		}

		public async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfReadOnly();

			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				foreach (var entity in entities) {
					var key = GetEntityKey(entity);

					if (key == null)
						throw new RepositoryException($"The entity '{typeof(TEntity)}' has no key");

					if (key is string keyString) {
						var existing = await session.LoadAsync<TEntity>(keyString, cancellationToken);
						if (existing == null)
							throw new RepositoryException($"The entity '{typeof(TEntity)}' with key '{keyString}' does not exist");

						session.Delete<TEntity>(keyString);
					} else if (key is Guid keyGuid) {
						var existing = await session.LoadAsync<TEntity>(keyGuid, cancellationToken);
						if (existing == null)
							throw new RepositoryException($"The entity '{typeof(TEntity)}' with key '{keyGuid}' does not exist");

						session.Delete<TEntity>(keyGuid);
					} else if (key is int keyInt) {
						var existing = await session.LoadAsync<TEntity>(keyInt, cancellationToken);
						if (existing == null)
							throw new RepositoryException($"The entity '{typeof(TEntity)}' with key '{keyInt}' does not exist");

						session.Delete<TEntity>(keyInt);
					} else {
						throw new RepositoryException($"The key '{key}' is not a valid string or GUID");
					}
				}

				await session.SaveChangesAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while removing a range of entities", ex);
			} finally {
				session?.Dispose();
			}
		}

		public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfReadOnly();

			IDocumentSession? session = null;

			try {
				session = await store.LightweightSerializableSessionAsync(cancellationToken);

				var key = GetEntityKey(entity);

				if (key == null)
					return false;

				TEntity? existing = null;
				if (key is string keyString)
					existing = await session.LoadAsync<TEntity>(keyString, cancellationToken);
				if (key is Guid keyGuid)
					existing = await session.LoadAsync<TEntity>(keyGuid, cancellationToken);
				if (key is int keyInt)
					existing = await session.LoadAsync<TEntity>(keyInt, cancellationToken);

				if (existing == null)
					return false;

				session.Update(entity);

				await session.SaveChangesAsync(cancellationToken);

				return true;
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while updating an entity", ex);
			} finally {
				session?.Dispose();
			}
		}

		public async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query, CancellationToken cancellationToken = default) {
			IQuerySession? session = null;

			try {
				session = await store.QuerySerializableSessionAsync(cancellationToken);

				IQueryable<TEntity> querySet = session.Query<TEntity>();
				if (query.Filter != null) {
					querySet = query.Filter.Apply(querySet);
				}

				var total = await querySet.CountAsync(cancellationToken);

				if (query.ResultSorts != null) {
					foreach (var sort in query.ResultSorts) {
						querySet = sort.Apply(querySet);
					}
				}

				querySet = querySet.Skip(query.Offset);

				if (query.Size > 0)
					querySet = querySet.Take(query.Size);

				return new PageResult<TEntity>(query, total, querySet.ToList());
			} catch (Exception ex) {
				throw new RepositoryException("Unknown error while getting a page of entities", ex);
			} finally {
				session?.Dispose();
			}
		}
	}
}
