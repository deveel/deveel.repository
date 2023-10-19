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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MongoDB.Bson;
using MongoDB.Driver;

using MongoFramework;
using MongoFramework.Infrastructure;
using MongoFramework.Infrastructure.Mapping;
using MongoFramework.Linq;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of <see cref="IRepository{TEntity}"/> contract
	/// that uses the MongoDB system to store and retrieve data.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity that is stored in the repository.
	/// </typeparam>
	public class MongoRepository<TEntity> : IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository<TEntity>,
		IControllableRepository, 
		IAsyncDisposable, 
		IDisposable
		where TEntity : class 
	{
		private IMongoDbSet<TEntity>? _dbSet;
		private bool disposed;

		/// <summary>
		/// Constructs the repository with the given context and logger.
		/// </summary>
		/// <param name="context">
		/// The context that is used to handle the connection to the MongoDB server.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		protected internal MongoRepository(IMongoDbContext context, ILogger? logger = null) {
			Context = context;
			Logger = logger ?? NullLogger.Instance;

			if (context is IMongoDbTenantContext tenantContext)
				TenantId = tenantContext.TenantId;
		}

		/// <summary>
		/// Constructs the repository with the given context and logger.
		/// </summary>
		/// <param name="context">
		/// The context that is used to handle the connection to the MongoDB server.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		public MongoRepository(IMongoDbContext context, ILogger<MongoRepository<TEntity>>? logger = null)
			: this(context, (ILogger?)logger) {
		}

		/// <summary>
		/// Gets the context that is used to handle the connection to the MongoDB server.
		/// </summary>
		protected IMongoDbContext Context { get; }

		/// <summary>
		/// Gets the <see cref="IMongoDbSet{TEntity}"/> that is used to handle the
		/// repository operations.
		/// </summary>
		protected IMongoDbSet<TEntity> DbSet => GetEntitySet();

		/// <summary>
		/// Gets the <see cref="ILogger"/> instance that is used to log messages
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// When the underlying context is a <see cref="IMongoDbTenantContext"/>,
		/// this property returns the tenant identifier that is used to filter
		/// the data in the repository.
		/// </summary>
		protected string? TenantId { get; }

		string? IMultiTenantRepository<TEntity>.TenantId => TenantId;

		IQueryable<TEntity> IQueryableRepository<TEntity>.AsQueryable() => DbSet.AsQueryable();

		/// <summary>
		/// Gets the <see cref="IMongoCollection{TEntity}"/> instance that is used
		/// to handle the data in the repository.
		/// </summary>
		protected IMongoCollection<TEntity> Collection {
			get {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));
				return Context.Connection.GetDatabase().GetCollection<TEntity>(entityDef.CollectionName);
			}
		}

		private static string RequireString(object? value) {
			if (value is string s)
				return s;
			if (value is ObjectId id)
				return id.ToString();

			var result = Convert.ToString(value, CultureInfo.InvariantCulture);
			if (String.IsNullOrWhiteSpace(result))
				throw new InvalidOperationException($"Could not convert '{value}' to string");

			return result;
		}

		/// <summary>
		/// Throws an exception if the repository has been disposed.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Thrown when the repository has been disposed.
		/// </exception>
		protected void ThrowIfDisposed() {
			if (disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Constructs a new <see cref="IMongoDbSet{TEntity}"/> that is
		/// coherent with the context and the entity type.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="RepositoryException"></exception>
		protected virtual IMongoDbSet<TEntity> MakeEntitySet() {
			if (Context is IMongoDbTenantContext tenantContext &&
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

		object? IRepository<TEntity>.GetEntityKey(TEntity entity)
			=> GetEntityKey(entity);

		/// <summary>
		/// Gets the value of the ID property of the given entity.
		/// </summary>
		/// <param name="entity">
		/// The entity whose ID property value is to be retrieved.
		/// </param>
		/// <returns>
		/// Returns the value of the ID property of the given entity.
		/// </returns>
		protected virtual object? GetEntityKey(TEntity entity) {
			var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var idProperty = entityDef.GetIdProperty();

			if (idProperty == null)
				throw new RepositoryException($"The type '{typeof(TEntity)}' has no ID property specified");

			return entityDef.GetIdValue(entity);
		}

		/// <summary>
		/// Converts the given value to the type of the ID property of the
		/// entity managed by this repository.
		/// </summary>
		/// <param name="key">
		/// The value representing the key of the entity.
		/// </param>
		/// <returns>
		/// Returns the value converted accordingly to the type of the ID property
		/// of the entity managed by this repository, or <c>null</c> if the given
		/// key is <c>null</c> or empty.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown if the entity managed by this repository has no ID property
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown when the value cannot be converted to the type of the ID
		/// property of the entity managed by this repository.
		/// </exception>
		protected virtual object? ConvertKeyValue(object? key) {
			if (key == null)
				return null;

			var idType = key.GetType();
			var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var idProperty = entityDef.GetIdProperty();

			if (idProperty == null)
				throw new RepositoryException($"The type '{typeof(TEntity)}' has no ID property specified");

			var valueType = idProperty.PropertyInfo.PropertyType;

			if (valueType == idType)
				return key;

			if (idType == typeof(string)) {
				if (valueType == typeof(ObjectId))
					return ObjectId.Parse(key.ToString());

				if (typeof(IConvertible).IsAssignableFrom(valueType))
					return Convert.ChangeType(key, valueType, CultureInfo.InvariantCulture);
			}

			throw new NotSupportedException($"It is not possible to convert the ID to '{valueType}'");
		}

		/// <summary>
		/// Resolves a given field name to an expression that can be used
		/// to access the field in the entity.
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to be resolved.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="Expression{TDelegate}"/> that
		/// is used to access the field in the entity.
		/// </returns>
		protected virtual Expression<Func<TEntity, object>> Field(string fieldName) {
			var param = Expression.Parameter(typeof(TEntity), "x");
			var body = Expression.PropertyOrField(param, fieldName);

			return Expression.Lambda<Func<TEntity, object>>(body, param);
		}

		#region Controllable

		Task<bool> IControllableRepository.ExistsAsync(CancellationToken cancellationToken)
			=> CollectionExistsAsync(cancellationToken);

		/// <summary>
		/// Verifies if the repository exists in the underlying database.
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the repository exists in the underlying
		/// database, otherwise <c>false</c>.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when an error occurs while verifying the existence of the
		/// collection in the underlying database.
		/// </exception>
		public async Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				var options = new ListCollectionNamesOptions {
					Filter = new BsonDocument(new Dictionary<string, object> { { "name", entityDef.CollectionName } })
				};

				var collectionNames = await Context.Connection.GetDatabase().ListCollectionNamesAsync(options, cancellationToken);
				var list = await collectionNames.ToListAsync(cancellationToken);

				return list.Any();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to determine the existence of the repository", ex);
			}
		}

		async Task IControllableRepository.CreateAsync(CancellationToken cancellationToken) {
			await CreateCollectionAsync(cancellationToken);
			await CreateIndicesAsync(cancellationToken);
		}

		/// <summary>
		/// Creates the collection that is used to store the entities 
		/// of this repository in the underlying database.
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that, when completed, has created the collection.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when an error occurs while creating the collection in the
		/// database or if the collection already exists.
		/// </exception>
		public async Task CreateCollectionAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				// TODO: should we also create the indices here?
				await Context.Connection.GetDatabase()
					.CreateCollectionAsync(entityDef.CollectionName, null, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to create the repository", ex);
			}
		}

		/// <summary>
		/// Creates all the indices that are defined for the entity
		/// that is managed by this repository.
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that, when completed, has created all the indices
		/// of the repository.
		/// </returns>
		/// <exception cref="RepositoryException"></exception>
		public async Task CreateIndicesAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				foreach (var indexDef in entityDef.Indexes) {
					var keysBuilder = new IndexKeysDefinitionBuilder<TEntity>();
					var indices = new List<CreateIndexModel<TEntity>>();
					foreach (var path in indexDef.IndexPaths) {
						var fieldDef = new StringFieldDefinition<TEntity>(path.Path);
						IndexKeysDefinition<TEntity>? keysDef = null;
						if (path.IndexType == IndexType.Standard) {
							keysDef = path.SortOrder == IndexSortOrder.Descending
								? keysBuilder.Descending(fieldDef)
								: keysBuilder.Ascending(fieldDef);
						} else if (path.IndexType == IndexType.Geo2dSphere) {
							keysDef = keysBuilder.Geo2DSphere(fieldDef);
						} else if (path.IndexType == IndexType.Text) {
							keysDef = keysBuilder.Text(fieldDef);
						}

						if (keysDef != null) {
							var options = new CreateIndexOptions {
								Unique = indexDef.IsUnique,
								Name = indexDef.IndexName
							};

							var indexModel = new CreateIndexModel<TEntity>(keysDef, options);
							indices.Add(indexModel);
						}
					}

					await Collection.Indexes.CreateManyAsync(indices, cancellationToken);
				}
			} catch (Exception ex) {

				throw new RepositoryException("Unable to create the indices for the repository", ex);
			}
		}

		async Task IControllableRepository.DropAsync(CancellationToken cancellationToken) {
			await DropIndicesAsync(cancellationToken);
			await DropCollectionAsync(cancellationToken);
		}

		/// <summary>
		/// Drops all the indices that are defined for the entity
		/// that is managed by this repository.
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that, when completed, has dropped all the indices
		/// from the repository.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when an error occurs while dropping the indices.
		/// </exception>
		public async Task DropIndicesAsync(CancellationToken cancellationToken = default) {
			try {
				await Collection.Indexes.DropAllAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to drop the indices", ex);
			}
		}

		/// <summary>
		/// Drops the collection that is used to store the entities
		/// that are managed by this repository in the underlying database.
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that, when completed, has dropped the collection.
		/// </returns>
		/// <exception cref="RepositoryException"></exception>
		public async Task DropCollectionAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				await Context.Connection.GetDatabase().DropCollectionAsync(entityDef.CollectionName, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to drop the repository", ex);
			}
		}

		#endregion

		/// <summary>
		/// A callback method that is invoked before the entity is created.
		/// </summary>
		/// <param name="entity">
		/// The entity that is about to be created.
		/// </param>
		/// <returns>
		/// Returns the entity that is about to be created.
		/// </returns>
		protected virtual TEntity OnAddEntity(TEntity entity) {
			return entity;
		}

		/// <summary>
		/// Creates a new entity in the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity to be created in the repository.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the unique identifier of the created entity.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when an error occurs while creating the entity in the
		/// underlying database.
		/// </exception>
		public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			if (!String.IsNullOrWhiteSpace(TenantId)) {
				Logger.TraceCreatingForTenant(TenantId);
			} else {
				Logger.TraceCreating();
			}

			try {
				entity = OnAddEntity(entity);

				DbSet.Add(entity);
				await DbSet.Context.SaveChangesAsync(cancellationToken);

				var id = GetEntityKey(entity);

				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceCreatedForTenant(TenantId, id!);
				} else {
					Logger.TraceCreated(id!);
				}
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to create the entity", ex);
			}
		}

		/// <inheritdoc/>
		public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			try {
				entities = entities.Select(OnAddEntity);

				DbSet.AddRange(entities);
				await DbSet.Context.SaveChangesAsync(cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Could not add the list of entities", ex);
			}
		}

		#region Update

		/// <summary>
		/// A callback method that is invoked before the entity is updated.
		/// </summary>
		/// <param name="entity">
		/// The entity that is about to be updated.
		/// </param>
		/// <returns>
		/// Returns the entity that is about to be updated.
		/// </returns>
		protected virtual TEntity OnEntityUpdate(TEntity entity) {
			return entity;
		}

		/// <inheritdoc/>
		public virtual async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			var id = GetEntityKey(entity);

			if (id == null)
				throw new ArgumentException(nameof(entity), "Cannot determine the identifier of the entity");

			if (!String.IsNullOrWhiteSpace(TenantId)) {
				Logger.TraceUpdatingForTenant(TenantId, id.ToString()!);
			} else {
				Logger.TraceUpdating(id.ToString()!);
			}

			try {
				var entry = Context.ChangeTracker.GetEntryById<TEntity>(id);
				if (entry == null || entry.State == EntityEntryState.Deleted)
					return false;

				entity = OnEntityUpdate(entity);

				DbSet.Update(entity);
				var updated = entry.State == EntityEntryState.Updated;

				await Context.SaveChangesAsync(cancellationToken);

				if (updated) {
					if (!String.IsNullOrWhiteSpace(TenantId)) {
						Logger.TraceUpdatedForTenant(TenantId, id.ToString()!);
					} else {
						Logger.TraceUpdated(id.ToString()!);
					}
				}

				return updated;
			} catch (Exception ex) {
				Logger.LogUnknownEntityError(ex, id.ToString()!);
				throw new RepositoryException("Unable to update the entity", ex);
			}
		}

		#endregion

		/// <inheritdoc/>
		public virtual async Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			var entityId = GetEntityKey(entity);
			if (entityId == null)
				throw new ArgumentException("The entity does not have an ID", nameof(entity));

			var id = RequireString(entityId);

			try {
				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceDeletingForTenant(TenantId, id);
				} else {
					Logger.TraceDeleting(id);
				}

				var entry = Context.ChangeTracker.GetEntry(entity);
				if (entry == null)
					return false;

				DbSet.Remove(entity);
				await Context.SaveChangesAsync(cancellationToken);

				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceDeletedForTenant(TenantId, id);
				} else {
					Logger.TraceDeleted(id);
				}

				return entry.State == EntityEntryState.Deleted;
			} catch (Exception ex) {
				Logger.LogUnknownEntityError(ex, id);

				throw new RepositoryException("Unable to delete the entity", ex);
			}
		}

		/// <inheritdoc/>
		public async Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			try {
				if (entities.Any(x => DbSet.Context.ChangeTracker.GetEntry(x) == null))
					throw new RepositoryException("The list contains entities that are not tracked by the repository");

				DbSet.RemoveRange(entities);

				await Context.SaveChangesAsync(cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Could not delete the list of entities", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<TEntity?> FindByKeyAsync(object key, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();


			if (!String.IsNullOrWhiteSpace(TenantId)) {
				Logger.TraceFindingByIdForTenant(TenantId, key);
			} else {
				Logger.TraceFindingById(key);
			}

			try {
				var keyValue = ConvertKeyValue(key);

				var entry = Context.ChangeTracker.GetEntryById<TEntity>(keyValue);
				if (entry != null && entry.State == EntityEntryState.Deleted)
					return null;

				var result = await DbSet.FindAsync(keyValue);

				if (result == null)
					return null;


				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceFoundByIdForTenant(TenantId, key);
				} else {
					Logger.TraceFoundById(key);
				}

				return result;
			} catch (Exception ex) {
				Logger.LogUnknownEntityError(ex, key);
				throw new RepositoryException("Unable to find the entity", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<TEntity?> FindAsync(Query query, CancellationToken cancellationToken = default) {
			try {
				var entities = query.Apply(DbSet.AsQueryable());
				return await entities.FirstOrDefaultAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to find the entity", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<IList<TEntity>> FindAllAsync(Query query, CancellationToken cancellationToken = default) {
			try {
				var entities = query.Apply(DbSet.AsQueryable());
				return await entities.ToListAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to find the entities", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query, CancellationToken cancellationToken = default) {
			try {
				var entitySet = query.ApplyQuery(DbSet.AsQueryable());

				var totalCount = await entitySet.CountAsync(cancellationToken);

				entitySet = entitySet.Skip(query.Offset).Take(query.Size);

				var items = await entitySet.ToListAsync(cancellationToken);
				return new PageResult<TEntity>(query, totalCount, items);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		/// <inheritdoc/>
		public Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			try {
				var query = DbSet.AsQueryable();
				if (filter != null) {
					query = filter.Apply(query);
				}

				return query.AnyAsync(cancellationToken);
			} catch (Exception ex) {

				throw new RepositoryException("Unable to execute the query", ex);
			}
		}

		/// <inheritdoc/>
		public async Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default) {
			try {
				var query = DbSet.AsQueryable();
				if (filter != null) {
					query = filter.Apply(query);
				}

				return await query.CountAsync(cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to count the entities", ex);
			}
		}

		#region Dispose

		void IDisposable.Dispose() {
			Dispose(false);
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
			// Suppress finalization.
			GC.SuppressFinalize(this);
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

			disposed = true;
		}

		/// <summary>
		/// Disposes of the unmanaged resources used by the repository.
		/// </summary>
		/// <param name="disposing">
		/// Indicates if the repository is being disposed of.
		/// </param>
		protected virtual void Dispose(bool disposing) {
		}

		/// <summary>
		/// Performs the async cleanup of the repository.
		/// </summary>
		/// <returns>
		/// Returns a task that, when completed, has performed the cleanup.
		/// </returns>
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
