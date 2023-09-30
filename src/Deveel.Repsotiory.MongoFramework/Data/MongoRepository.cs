using System.Globalization;
using System.Linq.Expressions;

using Microsoft.Extensions.Internal;
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
	/// <typeparam name="TContext">
	/// The type of the <see cref="IMongoDbContext"/> that is used to
	/// handling the connection to the MongoDB server.
	/// </typeparam>
	/// <typeparam name="TEntity">
	/// The type of the entity that is stored in the repository.
	/// </typeparam>
	public class MongoRepository<TContext, TEntity> : IRepository<TEntity>, 
		IQueryableRepository<TEntity>, 
		IPageableRepository<TEntity>, 
		IFilterableRepository<TEntity>,
		IMultiTenantRepository<TEntity>,
		IControllableRepository, 
		IAsyncDisposable, 
		IDisposable
		where TContext : class, IMongoDbContext
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
		/// <param name="systemTime">
		/// A service that provides the current system time.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		protected internal MongoRepository(TContext context, ISystemTime? systemTime = null, ILogger? logger = null) {
			Context = context;
			SystemTime = systemTime ?? Deveel.Data.SystemTime.Default;
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
		/// <param name="systemTime">
		/// A service that provides the current system time.
		/// </param>
		/// <param name="logger">
		/// A logger instance that is used to log messages from the repository.
		/// </param>
		public MongoRepository(TContext context, ISystemTime? systemTime = null, ILogger<MongoRepository<TContext, TEntity>>? logger = null)
			: this(context, systemTime, (ILogger?)logger) {
		}

		/// <summary>
		/// Gets the context that is used to handle the connection to the MongoDB server.
		/// </summary>
		protected TContext Context { get; }

		/// <summary>
		/// Gets the <see cref="IMongoDbSet{TEntity}"/> that is used to handle the
		/// repository operations.
		/// </summary>
		protected IMongoDbSet<TEntity> DbSet => GetEntitySet();

		/// <summary>
		/// Gets a service that provides the current system time.
		/// </summary>
		protected ISystemTime SystemTime { get; }

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

		private static string RequireString(object value) {
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

		string? IRepository<TEntity>.GetEntityId(TEntity entity) {
			var value = GetEntityId(entity);

			if (value == null)
				return null;

			if (value is string s)
				return s;
			if (value is ObjectId id)
				return id.ToEntityId();

			return Convert.ToString(value, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Gets the value of the ID property of the given entity.
		/// </summary>
		/// <param name="entity">
		/// The entity whose ID property value is to be retrieved.
		/// </param>
		/// <returns>
		/// Returns the value of the ID property of the given entity.
		/// </returns>
		protected virtual object? GetEntityId(TEntity entity) {
			var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var idProperty = entityDef.GetIdProperty();

			if (idProperty == null)
				throw new RepositoryException($"The type '{typeof(TEntity)}' has no ID property specified");

			return idProperty.PropertyInfo.GetValue(entity);
		}

		/// <summary>
		/// Converts the given string value to the type of the ID property of the
		/// entity managed by this repository.
		/// </summary>
		/// <param name="id">
		/// The string representation of the ID value.
		/// </param>
		/// <returns>
		/// Returns the value converted accordingly to the type of the ID property
		/// of the entity managed by this repository, or <c>null</c> if the given
		/// string is <c>null</c> or empty.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown if the entity managed by this repository has no ID property
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown when the value cannot be converted to the type of the ID
		/// property of the entity managed by this repository.
		/// </exception>
		protected virtual object? ConvertIdValue(string? id) {
			if (String.IsNullOrWhiteSpace(id))
				return null;

			var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

			var idProperty = entityDef.GetIdProperty();

			if (idProperty == null)
				throw new RepositoryException($"The type '{typeof(TEntity)}' has no ID property specified");

			var valueType = idProperty.PropertyInfo.PropertyType;

			if (valueType == typeof(string))
				return id;

			if (valueType == typeof(ObjectId))
				return ObjectId.Parse(id);

			if (typeof(IConvertible).IsAssignableFrom(valueType))
				return Convert.ChangeType(id, valueType, CultureInfo.InvariantCulture);

			throw new NotSupportedException($"It is not possible to convert the ID to '{valueType}'");
		}

		/// <summary>
		/// Asserts that the given entity is of the type managed by this repository.
		/// </summary>
		/// <param name="entity">
		/// The object that has to be asserted.
		/// </param>
		/// <returns>
		/// Returns an instance of the object casted to the type managed by this
		/// repository.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when the given entity is not of the type managed by this
		/// repository
		/// </exception>
		protected static TEntity Assert(object entity) {
			if (!(entity is TEntity entityObj))
				throw new ArgumentException($"The type '{entity.GetType()}' is not assignable from '{typeof(TEntity)}'");

			return entityObj;
		}

		/// <summary>
		/// Gets the MongoDB filter definition for the given query filter.
		/// </summary>
		/// <param name="filter">
		/// The query filter to be converted to a MongoDB filter definition.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="FilterDefinition{TEntity}"/> that
		/// is mapped from the given query filter.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// Thrown when the given query filter is not supported by this repository.
		/// </exception>
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

		Task IControllableRepository.CreateAsync(CancellationToken cancellationToken) {
			return CreateCollectionAsync(cancellationToken);
		}

		public async Task CreateCollectionAsync(CancellationToken cancellationToken = default) {
			try {
				var entityDef = EntityMapping.GetOrCreateDefinition(typeof(TEntity));

				// TODO: should we also create the indices here?
				await Context.Connection.GetDatabase().CreateCollectionAsync(entityDef.CollectionName, null, cancellationToken);
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to create the repository", ex);
			}
		}

		Task IControllableRepository.DropAsync(CancellationToken cancellationToken) {
			return DropCollectionAsync(cancellationToken);
		}

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

		#region Add

		/// <summary>
		/// A callback method that is invoked before the entity is created.
		/// </summary>
		/// <param name="entity">
		/// The entity that is about to be created.
		/// </param>
		/// <returns>
		/// Returns the entity that is about to be created.
		/// </returns>
		protected virtual TEntity OnCreating(TEntity entity) {
			if (entity is IHaveTimeStamp hasTime)
				hasTime.CreatedAtUtc = SystemTime.UtcNow;

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
		public async Task<string> AddAsync(TEntity entity, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			if (!String.IsNullOrWhiteSpace(TenantId)) {
				Logger.TraceCreatingForTenant(TenantId);
			} else {
				Logger.TraceCreating();
			}

			try {
				entity = OnCreating(entity);

				DbSet.Add(entity);
				await DbSet.Context.SaveChangesAsync(cancellationToken);

				// TODO: this is UGLY! change the IRepository to use object keys instead?
				var id = RequireString(GetEntityId(entity));

				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceCreatedForTenant(TenantId, id);
				} else {
					Logger.TraceCreated(id);
				}

				return id;
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Unable to create the entity", ex);
			}
		}

		public async Task<IList<string>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			try {
				entities = entities.Select(OnCreating);

				DbSet.AddRange(entities);
				await DbSet.Context.SaveChangesAsync(cancellationToken);

				// TODO: this is UGLY! Change the IRepository to work with object keys
				return entities.Select(x => RequireString(GetEntityId(x))).ToList();
			} catch (Exception ex) {
				Logger.LogUnknownError(ex);
				throw new RepositoryException("Could not add the list of entities", ex);
			}
		}

		#endregion

		#region Update

		protected virtual TEntity OnUpdating(TEntity entity) {
			if (entity is IHaveTimeStamp hasTime)
				hasTime.UpdatedAtUtc = SystemTime.UtcNow;

			return entity;
		}

		public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			var id = GetEntityId(entity);

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

				entity = OnUpdating(entity);

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

		#region Delete

		public async Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			if (entity is null) 
				throw new ArgumentNullException(nameof(entity));

			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();

			var entityId = GetEntityId(entity);
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

		#endregion

		#region FindById

		public async Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
			ThrowIfDisposed();
			cancellationToken.ThrowIfCancellationRequested();


			if (!String.IsNullOrWhiteSpace(TenantId)) {
				Logger.TraceFindingByIdForTenant(TenantId, id);
			} else {
				Logger.TraceFindingById(id);
			}

			try {
				var idValue = ConvertIdValue(id);

				var entry = Context.ChangeTracker.GetEntryById<TEntity>(idValue);
				if (entry != null && entry.State == EntityEntryState.Deleted)
					return null;

				var result = await DbSet.FindAsync(idValue);

				if (result == null)
					return null;


				if (!String.IsNullOrWhiteSpace(TenantId)) {
					Logger.TraceFoundByIdForTenant(TenantId, id);
				} else {
					Logger.TraceFoundById(id);
				}

				return result;
			} catch (Exception ex) {
				Logger.LogUnknownEntityError(ex, id);
				throw new RepositoryException("Unable to find the entity", ex);
			}
		}

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

		#endregion

		#region GetPage

		public async Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default) {
			try {
				var entitySet = DbSet.AsQueryable();

				if (request.Filter != null) {
					entitySet = request.Filter.Apply(entitySet);
				}

				var totalCount = await entitySet.CountAsync(cancellationToken);

				entitySet = entitySet.Skip(request.Offset).Take(request.Size);

				if (request.ResultSorts != null) {
					foreach (var sort in request.ResultSorts) {
						Expression<Func<TEntity, object>> keySelector;

						if (sort.Field is StringFieldRef stringRef) {
							keySelector = Field(stringRef.FieldName);
						} else if (sort.Field is ExpressionFieldRef<TEntity> exprRef) {
							keySelector = exprRef.Expression;
						} else {
							throw new NotSupportedException($"The sort of type {sort.GetType()} is not supported");
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
