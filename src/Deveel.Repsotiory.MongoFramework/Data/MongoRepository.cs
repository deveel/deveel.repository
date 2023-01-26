﻿using System.Globalization;
using System.Linq.Expressions;

using Deveel.Data.Mapping;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure.Commands;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace Deveel.Data {
	public class MongoRepository<TEntity> : IRepository<TEntity>, IQueryableRepository<TEntity>, IControllableRepository, IAsyncDisposable, IDisposable
		where TEntity : class, IEntity 
	{
		private IMongoDbSet<TEntity>? _dbSet;
		private bool disposed;

		protected internal MongoRepository(MongoDbContext context, ILogger? logger = null) {
			Context = context;
			Logger = logger ?? NullLogger.Instance;
		}

		public MongoRepository(MongoDbContext context, ILogger<MongoRepository<TEntity>>? logger = null)
			: this(context, (ILogger?)logger) {
		}

		public MongoRepository(MongoPerTenantContext context, ILogger<MongoRepository<TEntity>>? logger = null)
			: this(context, (ILogger?)logger) {
		}


		protected MongoDbContext Context { get; }

		protected IMongoDbSet<TEntity> DbSet => GetEntitySet();

		protected ILogger Logger { get; }

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => DbSet.AsQueryable();

		Type IRepository.EntityType => typeof(TEntity);

		bool IRepository.SupportsPaging => true;

		bool IRepository.SupportsFilters => true;

		protected IMongoCollection<TEntity> Collection {
			get {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				return Context.Connection.GetDatabase().GetCollection<TEntity>(entityDef.CollectionName);
			}
		}

		private static string RequireString(object value) {
			if (value is string s)
				return s;

			var result = Convert.ToString(value, CultureInfo.InvariantCulture);
			if (String.IsNullOrWhiteSpace(result))
				throw new InvalidOperationException($"Could not convert '{value}' to string");

			return result;
		}

		protected void ThrowIfDisposed() {
			if (disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		protected virtual IMongoDbSet<TEntity> MakeEntitySet() {
			if (Context is MongoDbTenantContext tenantContext &&
				typeof(IHaveTenantId).IsAssignableFrom(typeof(TEntity))) {
				var dbSetType = typeof(MongoDbTenantSet<>).MakeGenericType(typeof(TEntity));
				var result = (IMongoDbSet<TEntity>?)Activator.CreateInstance(dbSetType, new object[] { Context });
				if (result == null)
					throw new RepositoryException("Unable to create any tenant set");

				return result;
			}

			return Context.Set<TEntity>();
		}

		private IMongoDbSet<TEntity> GetEntitySet() {
			if (_dbSet == null) {
				_dbSet = MakeEntitySet();
			}

			return _dbSet;
		}

		protected virtual object GetIdValue(TEntity entity) => GetIdValue(entity.Id);

		protected virtual object GetIdValue(string id) {
			var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var idProperty = entityDef.GetIdProperty();

			if (idProperty == null)
				throw new RepositoryException($"The type '{typeof(TEntity)}' has no ID property specified");

			var valueType = idProperty.PropertyType;

			if (valueType == typeof(string))
				return id;

			if (valueType == typeof(ObjectId))
				return ObjectId.Parse(id);

			if (typeof(IConvertible).IsAssignableFrom(valueType))
				return Convert.ChangeType(id, valueType, CultureInfo.InvariantCulture);

			throw new NotSupportedException($"It is not possible to convert the ID to '{valueType}'");
		}

		protected static TEntity Assert(IEntity entity) {
			if (!(entity is TEntity entityObj))
				throw new ArgumentException($"The type '{entity.GetType()}' is not assignable from '{typeof(TEntity)}'");

			return entityObj;
		}

		protected virtual FilterDefinition<TEntity> GetFilterDefinition(IQueryFilter? filter) {
			if (filter == null || filter.IsEmpty())
				return Builders<TEntity>.Filter.Empty;

			if (filter is ExpressionQueryFilter<TEntity> expr)
				return Builders<TEntity>.Filter.Where(expr.Expression);
			if (filter is MongoQueryFilter<TEntity> filterDef)
				return filterDef.Filter;

			throw new ArgumentException($"The query filter type '{filter.GetType()}' is not supported by Mongo");
		}


		protected virtual Expression<Func<TEntity, object>> Field(string fieldName) {
			var param = Expression.Parameter(typeof(TEntity), "x");
			var body = Expression.PropertyOrField(param, fieldName);

			return Expression.Lambda<Func<TEntity, object>>(body, param);
		}

		protected virtual object? GetCurrentVersion(TEntity entity) {
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var property = definition.GetVersionPropery();

			if (property == null)
				return null;

			var value = property.GetValue(entity);

			return VersionUtil.Format(value, property.Format);
		}

		protected virtual void SetNewVersion(TEntity entity) {
			var definition = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var property = definition.GetVersionPropery();

			if (property == null)
				return;

			var current = property.GetValue(entity);
			var newVersion = VersionUtil.GetNewVersion(property.Format, property.PropertyType, current);

			property.SetValue(entity, newVersion);
		}

		#region Controllable

		public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				var options = new ListCollectionNamesOptions {
					Filter = new BsonDocument(new Dictionary<string, object> { { "name", entityDef.CollectionName } })
				};

				var collectionNames = await Context.Connection.GetDatabase().ListCollectionNamesAsync(options, cancellationToken);
				var list = await collectionNames.ToListAsync(cancellationToken);

				return list.Any();
			} catch (Exception ex) {

				throw new RepositoryException("Unable to determine the existence of the repository", ex);
			}
		}

		public async Task CreateAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				// TODO: should we also create the indices here?
				await Context.Connection.GetDatabase().CreateCollectionAsync(entityDef.CollectionName, null, cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to create the repository", ex);
			}
		}

		public async Task DropAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				await Context.Connection.GetDatabase().DropCollectionAsync(entityDef.CollectionName, cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to drop the repository", ex);
			}
		}

		#endregion

		#region Create

		Task<string> IRepository<TEntity>.CreateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		public async Task<string> CreateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			try {
				SetNewVersion(entity);

				DbSet.Add(entity);
				await DbSet.Context.SaveChangesAsync(cancellationToken);

				// TODO: this is UGLY! change the IRepository to use object keys instead?
				return RequireString(GetIdValue(entity));
			} catch (Exception ex) {
				Logger.LogError(ex, "Error creating an entity");
				throw new RepositoryException("Unable to create the entity", ex);
			}
		}

		Task<string> IRepository.CreateAsync(IEntity entity, CancellationToken cancellationToken)
			=> CreateAsync(Assert(entity), cancellationToken);

		Task<string> IRepository.CreateAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		Task<IList<string>> IRepository<TEntity>.CreateAsync(IDataTransaction transaction, IEnumerable<TEntity> entities, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		Task<IList<string>> IRepository.CreateAsync(IDataTransaction transaction, IEnumerable<IEntity> entities, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		Task<IList<string>> IRepository.CreateAsync(IEnumerable<IEntity> entities, CancellationToken cancellationToken)
			=> CreateAsync(entities.Select(x => Assert(x)), cancellationToken);

		public async Task<IList<string>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			try {
				foreach (var entity in entities) {
					SetNewVersion(entity);
				}

				DbSet.AddRange(entities);
				await DbSet.Context.SaveChangesAsync(cancellationToken);

				// TODO: this is UGLY! Change the IRepository to work with object keys
				return entities.Select(x => RequireString(GetIdValue(x))).ToList();
			} catch (Exception ex) {

				throw new RepositoryException("Could not add the list of entities", ex);
			}
		}

		#endregion

		#region Update

		public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				var filter = entityDef.CreateIdFilterFromEntity(entity);
				var versionFilter = entityDef.CreateVersionFilterFromEntity(entity);

				if (versionFilter != null)
					filter = Builders<TEntity>.Filter.And(filter, versionFilter);

				if (!await ExistsAsync(filter, cancellationToken))
					return false;

				SetNewVersion(entity);

				DbSet.Update(entity);
				await Context.SaveChangesAsync(cancellationToken);

				return true;
			} catch (Exception ex) {

				throw new RepositoryException("Unable to update the entity", ex);
			}
		}

		Task<bool> IRepository<TEntity>.UpdateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		Task<bool> IRepository.UpdateAsync(IEntity entity, CancellationToken cancellationToken) 
			=> UpdateAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository.UpdateAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		#endregion

		#region Delete

		public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			try {
				var toDelete = await DbSet.FindAsync(GetIdValue(entity));

				if (toDelete == null)
					return false;

				DbSet.Remove(toDelete);
				await Context.SaveChangesAsync(cancellationToken);

				return true;
			} catch (Exception ex) {
				throw new RepositoryException("Unable to delete the entity", ex);
			}
		}

		Task<bool> IRepository<TEntity>.DeleteAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported");


		Task<bool> IRepository.DeleteAsync(IEntity entity, CancellationToken cancellationToken)
			=> DeleteAsync(Assert(entity), cancellationToken);

		Task<bool> IRepository.DeleteAsync(IDataTransaction transaction, IEntity entity, CancellationToken cancellationToken) 
			=> throw new NotSupportedException("Transactions not supported");

		#endregion

		#region FindById

		public async Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			try {
				var idValue = GetIdValue(id);

				return await DbSet.FindAsync(idValue);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to find the entity", ex);
			}
		}

		async Task<IEntity?> IRepository.FindByIdAsync(string id, CancellationToken cancellationToken)
			=> await FindByIdAsync(id, cancellationToken);

		Task<IEntity?> IRepository.FindByIdAsync(IDataTransaction transaction, string id, CancellationToken cancellationToken)
			=> throw new NotSupportedException("Transactions not supported");

		#endregion

		#region Find

		public Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> FindAsync(GetFilterDefinition(filter), cancellationToken);

		public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
			try {
				return await DbSet.Where(filter).FirstOrDefaultAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		public async Task<TEntity?> FindAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) {
			try {
				var options = new FindOptions<TEntity> { Limit = 1 };

				var result = await Collection.FindAsync(filter, options, cancellationToken);

				return await result.FirstOrDefaultAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to find the entity", ex);
			}
		}

		async Task<IEntity?> IRepository.FindAsync(IQueryFilter filter, CancellationToken cancellationToken) 
			=> await FindAsync(filter, cancellationToken);

		#endregion

		#region FindAll

		public Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> FindAllAsync(GetFilterDefinition(filter), cancellationToken);

		public async Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
			try {
				return await DbSet.Where(filter).ToListAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		public async Task<IList<TEntity>> FindAllAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) {
			try {
				var result = await Collection.FindAsync(filter, null, cancellationToken);
				return await result.ToListAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		async Task<IList<IEntity>> IRepository.FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken) {
			var result = await FindAllAsync(filter, cancellationToken);
			return result.Cast<IEntity>().ToList();
		}

		#endregion

		#region GetPage

		public async Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default) {
			try {
				var entitySet = DbSet.AsQueryable();

				if (request.Filter != null) {
					entitySet = entitySet.Where(request.Filter);
				}

				var totalCount = await entitySet.CountAsync(cancellationToken);

				entitySet = entitySet.Skip(request.Offset).Take(request.Size);

				if (request.SortBy != null) {
					foreach (var sort in request.SortBy) {
						Expression<Func<TEntity, object>> keySelector;

						if (sort.Field is StringFieldRef stringRef) {
							keySelector = Field(stringRef.FieldName);
						} else if (sort.Field is ExpressionFieldRef<TEntity> exprRef) {
							keySelector = exprRef.Expression;
						} else {
							throw new NotSupportedException();
						}

						if (sort.Ascending) {
							entitySet = entitySet.OrderBy(keySelector);
						} else {
							entitySet = entitySet.OrderByDescending(keySelector);
						}
					}
				}

				var items = await entitySet.ToListAsync(cancellationToken);
				return new RepositoryPage<TEntity>(request, totalCount, items);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		async Task<RepositoryPage> IRepository.GetPageAsync(RepositoryPageRequest request, CancellationToken cancellationToken) {
			var newRequest = new RepositoryPageRequest<TEntity>(request.Page, request.Size) {
				Filter = request.Filter?.AsLambda<TEntity>(),
				SortBy = request.SortBy
			};

			var result = await GetPageAsync(newRequest, cancellationToken);

			return new RepositoryPage(request, result.TotalItems, result.Items?.Cast<IEntity>());
		}

		#endregion

		#region Exists

		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> ExistsAsync(GetFilterDefinition(filter), cancellationToken);

		public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
			try {
				return await DbSet.Where(filter).AnyAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		public async Task<bool> ExistsAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) {
			try {
				var result = await Collection.CountDocumentsAsync(filter, null, cancellationToken);

				return result > 0;
			} catch (Exception ex) {
				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		#endregion

		#region Count

		public Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default)
			=> CountAsync(GetFilterDefinition(filter), cancellationToken);

		public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) {
			try {
				return await DbSet.Where(filter).CountAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		public async Task<long> CountAsync(FilterDefinition<TEntity> filter, CancellationToken cancellationToken) {
			try {
				return await Collection.CountDocumentsAsync(filter, null, cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		#endregion

		#region Dispose

		void IDisposable.Dispose() {
			Dispose(false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			// Suppress finalization.
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

			disposed = true;
		}

		protected virtual void Dispose(bool disposing) {
		}

		protected virtual ValueTask DisposeAsyncCore() {
			return ValueTask.CompletedTask;
		}

		async ValueTask IAsyncDisposable.DisposeAsync() {
			if (!disposed) {
				// Perform async cleanup.
				await DisposeAsyncCore();

				// Dispose of unmanaged resources.
				Dispose(false);

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
				// Suppress finalization.
				GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

				disposed = true;
			}
		}

		#endregion
	}
}