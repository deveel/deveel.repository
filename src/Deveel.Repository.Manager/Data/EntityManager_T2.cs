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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Deveel.Data.Caching;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	/// <summary>
	/// A service that provides a set of operations to manage a specific
	/// entities in a repository.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the service.
	/// </typeparam>
	public class EntityManager<TEntity, TKey> : IDisposable, IAsyncDisposable where TEntity : class {
		private bool disposedValue;

		/// <summary>
		/// Constructs the service with the given repository.
		/// </summary>
		/// <param name="repository">
		/// The repository that is providing the data access to the entities.
		/// </param>
		/// <param name="validator">
		/// An optional service used to validate the entity before it is added
		/// or updated in the repository.
		/// </param>
		/// <param name="cache">
		/// An optional service used to cache the entities
		/// for faster access.
		/// </param>
		/// <param name="systemTime">
		/// A service used to get the current system time.
		/// </param>
		/// <param name="errorFactory">
		/// An optional factory used to create errors specific
		/// for the entity manager.
		/// </param>
		/// <param name="services">
		/// The services used to resolve the dependencies of the manager.
		/// </param>
		/// <param name="loggerFactory">
		/// A factory used to create a logger for the manager.
		/// </param>
		public EntityManager(
			IRepository<TEntity, TKey> repository,
			IEntityValidator<TEntity, TKey>? validator = null,
			IEntityCache<TEntity>? cache = null,
			ISystemTime? systemTime = null,
			IOperationErrorFactory<TEntity>? errorFactory = null,
			IServiceProvider? services = null,
			ILoggerFactory? loggerFactory = null) {
			ArgumentNullException.ThrowIfNull(repository, nameof(repository));

			Repository = repository;
			Time = systemTime ?? SystemTime.Default;
			EntityCache = cache;
			EntityValidator = validator;
			ErrorFactory = errorFactory;
			Services = services;
			Logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
		}

		/// <summary>
		/// Destructs the instance of the service.
		/// </summary>
		~EntityManager() {
			Dispose(disposing: false);
		}

		/// <summary>
		/// Gets the instance of the service provider used to resolve
		/// the dependencies of the service.
		/// </summary>
		protected IServiceProvider? Services { get; private set; }

		/// <summary>
		/// Gets the instance of the service used to get the current
		/// time of the system.
		/// </summary>
		protected ISystemTime Time { get; }

		/// <summary>
		/// Gets the instance of the factory used to create errors
		/// </summary>
		protected IOperationErrorFactory? ErrorFactory { get; }

		/// <summary>
		/// Gets an instance of the cache used to store entities
		/// </summary>
		protected IEntityCache<TEntity>? EntityCache { get; }

		/// <summary>
		/// Gets an instance of the generator used to create the
		/// keys for caching entities.
		/// </summary>
		protected IEntityCacheKeyGenerator<TEntity>? EntityCacheKeyGenerator
			=> Services?.GetService<IEntityCacheKeyGenerator<TEntity>>();

		/// <summary>
		/// Gets the service used to validate the entity before
		/// it is added or updated in the repository.
		/// </summary>
		protected IEntityValidator<TEntity, TKey>? EntityValidator { get; }

		/// <summary>
		/// Gets the logger used to log messages from the service.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		/// Gets the repository that is providing the data access
		/// to the entities.
		/// </summary>
		protected IRepository<TEntity, TKey> Repository { get; private set; }

		/// <summary>
		/// Gets the cancellation token used to cancel operations
		/// </summary>
		protected virtual CancellationToken CancellationToken
			=> Services?.GetService<IOperationCancellationSource>()?.Token ?? default;

		/// <summary>
		/// Gets a value indicating if the repository is multi-tenant
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		public virtual bool IsMultiTenant {
			get {
				ThrowIfDisposed();
				return (Repository is IMultiTenantRepository<TEntity> multiTenant) &&
					!String.IsNullOrWhiteSpace(multiTenant.TenantId);
			}
		}

		/// <summary>
		/// When the repository is multi-tenant, gets the identifier
		/// of the tenant that is being managed by the service.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		public virtual string? TenantId {
			get {
				ThrowIfDisposed();

				if (Repository is IMultiTenantRepository<TEntity> multiTenant)
					return multiTenant.TenantId;

				return null;
			}
		}

		/// <summary>
		/// Gets a value indicating if the repository supports paging
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		public virtual bool SupportsPaging {
			get {
				ThrowIfDisposed();

				return (Repository is IPageableRepository<TEntity>);
			}
		}

		/// <summary>
		/// Gets the repository that supports paging
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support paging
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		protected virtual IPageableRepository<TEntity> PageableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IPageableRepository<TEntity> pageable))
					throw new NotSupportedException("The repository does not support paging");

				return pageable;
			}
		}

		/// <summary>
		/// Gets a value indicating if the repository supports queries
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		public virtual bool SupportsQueries {
			get {
				ThrowIfDisposed();

				return (Repository is IQueryableRepository<TEntity>);
			}
		}

		/// <summary>
		/// When the repository supports queries, gets the queryable
		/// instance of the repository.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support queries
		/// </exception>
		protected virtual IQueryable<TEntity> Entities {
			get {
				ThrowIfDisposed();

				if (!(Repository is IQueryableRepository<TEntity> queryable))
					throw new NotSupportedException("The repository does not support queries");

				return queryable.AsQueryable();
			}
		}

		/// <summary>
		/// Gets a value indicating if the repository supports filters
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Thrown when the service has been disposed.
		/// </exception>
		public virtual bool SupportsFilters {
			get {
				ThrowIfDisposed();

				return (Repository is IFilterableRepository<TEntity>);
			}
		}

		/// <summary>
		/// Gets the repository that supports filters
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filters
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// Thrown when the service has been disposed.
		/// </exception>
		protected virtual IFilterableRepository<TEntity> FilterableRepository {
			get {
				ThrowIfDisposed();

				if (!(Repository is IFilterableRepository<TEntity> filterable))
					throw new NotSupportedException("The repository does not support filters");

				return filterable;
			}
		}

		// shortcut to the logger method
		private void LogUnknownError(Exception ex) {
			Logger.LogUnknownError(typeof(TEntity), ex);
		}

		private void LogEntityUnknownError(object? entityKey, Exception ex) {
			Logger.LogEntityUnknownError(typeof(TEntity), entityKey, ex);
		}

		private void LogEntityNotFound(object? entityKey) {
			Logger.LogEntityNotFound(typeof(TEntity), entityKey);
		}

		/// <summary>
		/// Ensures that a cancellation token is available
		/// for a cancellable operation.
		/// </summary>
		/// <param name="cancellationToken">
		/// The token that was provided by the caller.
		/// </param>
		/// <remarks>
		/// This method checks if the given cancellation token
		/// passed to an operation is <c>null</c>, and if so,
		/// attempts to resolve a cancellation token from the
		/// context.
		/// </remarks>
		/// <returns>
		/// Returns the cancellation token to be used for 
		/// an operation.
		/// </returns>
		protected CancellationToken GetCancellationToken(CancellationToken? cancellationToken)
			=> cancellationToken ?? CancellationToken;

		private static string GenerateCacheKeyFrom(object key) {
			var typeName = typeof(TEntity).Name.ToLowerInvariant();
			// TODO: support combined keys
			return $"{typeName}:{key}";
		}

		/// <summary>
		/// Generates a cache key for the given entity primary key.
		/// </summary>
		/// <param name="key">
		/// The primary key of the entity.
		/// </param>
		/// <returns>
		/// Returns a string that is the key to be used to cache
		/// entities in the cache.
		/// </returns>
		protected virtual string GenerateCacheKey(object key) {
			var generator = EntityCacheKeyGenerator;
			if (generator == null)
				return GenerateCacheKeyFrom(key);

			return generator.GenerateKey(key);
		}

		/// <summary>
		/// Checks if the service has been disposed and
		/// eventually throws an exception.
		/// </summary>
		/// <exception cref="ObjectDisposedException">
		/// Throws when the service has been disposed.
		/// </exception>
		protected void ThrowIfDisposed() {
			if (disposedValue)
				throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Actively disposes the service.
		/// </summary>
		/// <param name="disposing">
		/// Indicates whether the service is being disposed.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects)
				}

				Repository = null;
				Services = null;
				disposedValue = true;
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public ValueTask DisposeAsync() {
			Dispose(disposing: false);
			GC.SuppressFinalize(this);

			return default;
		}

		/// <summary>
		/// Creates an error object with the given code and message.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of error.
		/// </param>
		/// <param name="message">
		/// An optional message that describes the error.
		/// </param>
		/// <remarks>
		/// The default implementation of this method uses the
		/// <see cref="ErrorFactory"/> to create the error object,
		/// if any is available, otherwise it creates a new instance
		/// of <see cref="Deveel.OperationError"/>.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="IOperationError"/> that
		/// describes the error.
		/// </returns>
		protected virtual IOperationError OperationError(string errorCode, string? message = null)
			=> ErrorFactory?.CreateError(errorCode, message) ?? new OperationError(errorCode, message);

		/// <summary>
		/// Creates a validation error object with the given 
		/// code and validation results.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of validation error.
		/// </param>
		/// <param name="validationResults">
		/// The list of validation results that describe the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IValidationError"/> that
		/// represents a validation error.
		/// </returns>
		protected virtual IValidationError ValidationError(string errorCode, IList<ValidationResult> validationResults)
			=> ErrorFactory?.CreateValidationError(errorCode, validationResults) ?? new EntityValidationError(errorCode, validationResults);

		/// <summary>
		/// Creates an error object with the given code and message
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of error.
		/// </param>
		/// <param name="message">
		/// A message that describes the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
		/// <see cref="OperationError(string, string?)"/>
		protected OperationResult Fail(string errorCode, string? message = null)
			=> OperationResult.Fail(OperationError(errorCode, message));

		/// <summary>
		/// Creates an error object from the given operation exception.
		/// </summary>
		/// <param name="error">
		/// The exception that caused the error.
		/// </param>
		/// <remarks>
		/// The default implementation of this method uses the
		/// information from the given exception to create an error
		/// invoking the <see cref="Fail(string, string?)"/> method.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
		/// <seealso cref="Fail(string, string?)"/>
		protected OperationResult Fail(OperationException error)
			=> Fail(error.ErrorCode, error.Message);

		/// <summary>
		/// Creates an error object from a validation error.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of validation error.
		/// </param>
		/// <param name="validationResults">
		/// The list of validation results that describe the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed validation of an entity.
		/// </returns>
		/// <see cref="ValidationError(string, IList{ValidationResult})"/>
		protected OperationResult ValidationFailed(string errorCode, IList<ValidationResult> validationResults)
			=> OperationResult.Fail(ValidationError(errorCode, validationResults));

		/// <summary>
		/// Creates a result object that represents a successful operation.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a successful operation.
		/// </returns>
		protected OperationResult Success() => OperationResult.Success;

		/// <summary>
		/// Creates a result object that represents an operation that
		/// has not modified the state of the entity.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents an operation that has not modified the state of
		/// an entity.
		/// </returns>
		protected OperationResult NotModified() => OperationResult.NotModified;

		/// <summary>
		/// Validates the given entity before it is added or updated
		/// </summary>
		/// <param name="entity">
		/// The entity to be validated.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the validation operation.
		/// </param>
		/// <returns>
		/// Returns a list of <see cref="ValidationResult"/> that
		/// describe the validation errors.
		/// </returns>
		protected virtual async Task<IList<ValidationResult>> ValidateAsync(TEntity entity, CancellationToken cancellationToken) {
			if (EntityValidator == null)
				return new List<ValidationResult>();

			var results = new List<ValidationResult>();

			await foreach (var result in EntityValidator.ValidateAsync(this, entity, cancellationToken)) {
				if (result != null)
					results.Add(result);
			}

			return results;
		}

		/// <summary>
		/// Gets the key of the given entity
		/// </summary>
		/// <param name="entity">
		/// The entity to get the key from.
		/// </param>
		/// <remarks>
		/// The default implementation of this method uses the
		/// <see cref="IRepository{TEntity,TKey}.GetEntityKey(TEntity)"/>
		/// method to get the key of the entity.
		/// </remarks>
		/// <returns>
		/// Returns an object that represents the key of the entity,
		/// or <c>null</c> if the entity does not have a valid key.
		/// </returns>
		protected virtual TKey? GetEntityKey(TEntity entity) {
			return Repository.GetEntityKey(entity);
		}

		/// <summary>
		/// A callback invoked when an entity is being added 
		/// to the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity that is being added.
		/// </param>
		/// <remarks>
		/// When <see cref="AddRangeAsync(IEnumerable{TEntity},CancellationToken?)"/> is
		/// invoked, this method is invoked for each entity in the
		/// range of entities.
		/// </remarks>
		/// <returns>
		/// Returns the entity that is being added, after
		/// any modification done by the callback.
		/// </returns>
		protected virtual Task<TEntity> OnAddingEntityAsync(TEntity entity) {
			if (entity is IHaveTimeStamp haveTimeStamp && Time != null)
				haveTimeStamp.CreatedAtUtc = Time.UtcNow;

			return Task.FromResult(entity);
		}

		/// <summary>
		/// Generates the cache keys for the given entity.
		/// </summary>
		/// <param name="entity">
		/// The entity to generate the keys for.
		/// </param>
		/// <remarks>
		/// The default implementation of this method uses the
		/// the <see cref="IEntityCache{TEntity}.GenerateKeys(TEntity)"/>
		/// method, if any is available, otherwise it returns an
		/// empty array, that means that the entity will not be
		/// cached.
		/// </remarks>
		/// <returns>
		/// Returns an array of strings that are the keys that
		/// are used to identify the entity in the cache.
		/// </returns>
		protected virtual string[] GenerateCacheKeys(TEntity entity) {
			if (EntityCache == null)
				return Array.Empty<string>();

			return EntityCache.GenerateKeys(entity);
		}

		private async Task SetToCacheAsync(TEntity entity, CancellationToken cancellationToken) {
			if (EntityCache == null)
				return;

			try {
				// TODO: optimize this
				var keys = GenerateCacheKeys(entity);
				await EntityCache.SetAsync(keys, entity, cancellationToken);
			} catch (Exception ex) {
				Logger.LogEntityNotCached(ex, typeof(TEntity), GetEntityKey(entity));
			}
		}

		private async Task EvictAsync(TEntity entity, CancellationToken cancellationToken) {
			if (EntityCache == null)
				return;

			try {
				var keys = GenerateCacheKeys(entity);
				await EntityCache.RemoveAsync(keys, cancellationToken);
			} catch (Exception ex) {
				Logger.LogEntityNotEvicted(ex, typeof(TEntity), GetEntityKey(entity));
			}
		}

		/// <summary>
		/// Attempts to get the entity with the given entity key from the cache,
		/// and eventually uses the given factory to create the entity
		/// to return.
		/// </summary>
		/// <param name="key">
		/// The key of the entity to get from the cache.
		/// </param>
		/// <param name="valueFactory">
		/// A function that is used to create the entity to cache,
		/// when this was not found in the cache.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// This overload of the method uses the <see cref="GenerateCacheKey(object)"/>
		/// to generate the cache key for the given entity key, and
		/// it is better suited to be used by the <see cref="FindByKeyAsync(object, CancellationToken?)"/>
		/// methods and its overridden implementations.
		/// </remarks>
		/// <returns>
		/// Returns the entity from the cache, if available, or
		/// it returns <c>null</c> if the entity was not found
		/// in the cache and the factory was not able to create it.
		/// </returns>
		protected async Task<TEntity?> GetOrSetByKeyAsync(object key, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken) {
			return await GetOrSetAsync(GenerateCacheKey(key), valueFactory, cancellationToken);
		}

		/// <summary>
		/// Attempts to get the entity with the given cache key from the cache,
		/// and eventually uses the given factory to create the entity
		/// to be cached and returned.
		/// </summary>
		/// <param name="cacheKey">
		/// The cache key of the entity to get from the cache.
		/// </param>
		/// <param name="valueFactory">
		/// A function that is used to create the entity to cache,
		/// when this was not found in the cache.
		/// </param>
		/// <remarks>
		/// This method should be used with care for the key management, 
		/// since when it cannot find an item in cache with the given key,
		/// it will invoke the factory to create the entity to cache: this
		/// means that the entity will be surviving a removal of the entity
		/// from the repository, if the key is not then made available
		/// from an instance of <see cref="IEntityCacheKeyGenerator{TEntity}"/>.
		/// </remarks>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected async Task<TEntity?> GetOrSetAsync(string cacheKey, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken) {
			if (EntityCache == null)
				return await valueFactory();

			try {
				return await EntityCache.GetOrSetAsync(cacheKey, valueFactory, cancellationToken);
			} catch (Exception ex) {
				Logger.LogEntityNotCached(ex, typeof(TEntity), null);
				return await valueFactory();
			}
		}

		/// <summary>
		/// Adds the given entity to the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity to be added.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// describes the result of the operation.
		/// </returns>
		public virtual async Task<OperationResult> AddAsync(TEntity entity, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			try {
				Logger.LogAddingEntity(typeof(TEntity));

				var token = GetCancellationToken(cancellationToken);

				var validation = await ValidateAsync(entity, token);
				if (validation != null && validation.Count > 0) {
					Logger.LogEntityNotValid(typeof(TEntity));
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnAddingEntityAsync(entity);

				await Repository.AddAsync(entity, token);

				Logger.LogEntityAdded(GetEntityKey(entity)!);

				await SetToCacheAsync(entity, token);

				return Success();
			} catch (Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError);
			}
		}

		/// <summary>
		/// Adds the given range of entities to the repository.
		/// </summary>
		/// <param name="entities">
		/// The range of entities to be added.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// The default implementation of this method attempts to
		/// validate each entity in the range before adding it to
		/// the repository, and if any validation error is found,
		/// the method returns a failure result.
		/// </remarks>
		/// <returns>
		/// Returns a result object that describes the result of the
		/// operation.
		/// </returns>
		/// <seealso cref="IRepository{TEntity}.AddRangeAsync(IEnumerable{TEntity}, CancellationToken)"/>
		public virtual async Task<OperationResult> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken? cancellationToken = null) {
			try {
				Logger.LogAddingEntityRange(typeof(TEntity));

				var token = GetCancellationToken(cancellationToken);

				var toBeAdded = new List<TEntity>();
				foreach (var entity in entities) {
					var item = await OnAddingEntityAsync(entity);

					var validation = await ValidateAsync(item, token);
					if (validation != null && validation.Count > 0)
						return ValidationFailed(EntityErrorCodes.NotValid, validation);

					toBeAdded.Add(item);
				}

				await Repository.AddRangeAsync(toBeAdded, token);

				Logger.LogEntityRangeAdded();

				foreach (var item in entities) {
					await SetToCacheAsync(item, token);
				}

				return Success();
			} catch (Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError);
			}
		}

		/// <summary>
		/// A callback invoked when an entity is being updated
		/// </summary>
		/// <param name="entity">
		/// The entity that is being updated.
		/// </param>
		/// <returns>
		/// Returns the entity that is being updated, after
		/// the callback has modified it.
		/// </returns>
		protected virtual Task<TEntity> OnUpdatingEntityAsync(TEntity entity) {
			if (entity is IHaveTimeStamp haveTimeStamp && Time != null)
				haveTimeStamp.UpdatedAtUtc = Time.UtcNow;

			return Task.FromResult(entity);
		}

		/// <summary>
		/// Checks if the given entities are equal.
		/// </summary>
		/// <param name="existing">
		/// The entity already in the repository.
		/// </param>
		/// <param name="other">
		/// The entity provided by the caller for update.
		/// </param>
		/// <remarks>
		/// <para>
		/// This method is used by the update operation to check
		/// if the entity in the repository is the same as the one
		/// provided by the caller, and eventually skip the update
		/// on the repository, to ensure idempotency.
		/// </para>
		/// <para>
		/// The default implementation of this method tries to
		/// use the <see cref="IEquatable{T}.Equals(T)"/> method
		/// to check if the entities are equal, otherwise it uses
		/// the native <see cref="object.Equals(object)"/> method.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns <c>true</c> if the entities are equal, otherwise
		/// it returns <c>false</c>.
		/// </returns>
		protected virtual bool AreEqual(TEntity existing, TEntity other) {
			if (existing == null && other == null)
				return true;
			if (existing == null)
				return false;

			if (Repository is IEqualityComparer<TEntity> comparer)
				return comparer.Equals(existing, other);

			if (typeof(IEquatable<TEntity>).IsAssignableFrom(typeof(TEntity)))
				return ((IEquatable<TEntity>)existing).Equals(other);

			return existing.Equals(other);
		}

		/// <summary>
		/// Updates the given entity in the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity to be updated.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// <para>
		/// The default implementation of this method first
		/// attempts to find an entity in the repository with
		/// the same key of the given entity, and if found, it
		/// verifies the equality of the two entities using the
		/// <see cref="AreEqual(TEntity, TEntity)"/> function.
		/// If the two entities are equal, the method returns
		/// quickly an instance of <see cref="OperationResult"/>
		/// that indicates the entity was not modified.
		/// </para>
		/// <para>
		/// The method can reurn a failure result if the entity 
		/// does not have an associated key, or if the entity is 
		/// not found in the repository.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns a result object that describes the result of the
		/// update operation.
		/// </returns>
		public virtual async Task<OperationResult> UpdateAsync(TEntity entity, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			var entityKey = GetEntityKey(entity);

			try {
				if (entityKey == null)
					return Fail(EntityErrorCodes.NotValid, "The entity does not have a valid key");

				Logger.LogUpdatingEntity(typeof(TEntity), entityKey);

				var token = GetCancellationToken(cancellationToken);

				var existing = await FindByKeyAsync(entityKey, token);
				if (existing == null) {
					LogEntityNotFound(entityKey);
					return Fail(EntityErrorCodes.NotFound);
				}

				if (AreEqual(existing, entity)) {
					Logger.LogEntityNotModified(typeof(TEntity), entityKey);
					return NotModified();
				}

				var validation = await ValidateAsync(entity, token);
				if (validation != null && validation.Count > 0) {
					Logger.LogEntityNotValid(typeof(TEntity));
					return ValidationFailed(EntityErrorCodes.NotValid, validation);
				}

				entity = await OnUpdatingEntityAsync(entity);

				if (!await Repository.UpdateAsync(entity, token)) {
					Logger.LogEntityNotModified(typeof(TEntity), entityKey);
					return NotModified();
				}

				Logger.LogEntityUpdated(entityKey);

				await SetToCacheAsync(entity, token);

				return Success();
			} catch (Exception ex) {
				LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError);
			}
		}

		/// <summary>
		/// Removes the given entity from the repository.
		/// </summary>
		/// <param name="entity">
		/// The entity to be removed.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// <para>
		/// The default implementation of this method first tries
		/// to find an entity in the repository with the same key
		/// and if not found, it returns a failure result.
		/// </para>
		/// <para>
		/// If the entity is found, the method attempts to remove
		/// it from the repository, and if the operation is not
		/// performed by the repository, it returns a result that
		/// indicates the entity was not modified.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// describes the result of the operation.
		/// </returns>
		public virtual async Task<OperationResult> RemoveAsync(TEntity entity, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			var entityKey = GetEntityKey(entity);

			try {
				if (entityKey == null)
					return Fail(EntityErrorCodes.NotValid, "The entity does not have a valid key");

				var token = GetCancellationToken(cancellationToken);

				Logger.LogRemovingEntity(typeof(TEntity), entityKey);

				var found = await FindByKeyAsync(entityKey, token);
				if (found == null) {
					LogEntityNotFound(entityKey);
					return Fail(EntityErrorCodes.NotFound);
				}

				if (!await Repository.RemoveAsync(found, token)) {
					Logger.LogEntityNotRemoved(typeof(TEntity), entityKey);
					return NotModified();
				}

				await EvictAsync(entity, token);

				return Success();
			} catch (Exception ex) {
				LogEntityUnknownError(entityKey, ex);
				return Fail(EntityErrorCodes.UnknownError);
			}
		}

		/// <summary>
		/// Removes the given range of entities from the repository.
		/// </summary>
		/// <param name="entities">
		/// The range of entities to be removed.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// describes the result of the operation.
		/// </returns>
		public virtual async Task<OperationResult> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			try {
				Logger.LogRemovingEntityRange();

				var token = GetCancellationToken(cancellationToken);

				// TODO: should we check for the entities to be valid?

				await Repository.RemoveRangeAsync(entities, token);

				Logger.LogEntityRangeRemoved();

				foreach (var entity in entities) {
					await EvictAsync(entity, token);
				}

				return Success();
			} catch (Exception ex) {
				LogUnknownError(ex);
				return Fail(EntityErrorCodes.UnknownError);
			}
		}

		/// <summary>
		/// Finds an entity in the repository with the given key.
		/// </summary>
		/// <param name="key">
		/// The key of the entity to be found.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that
		/// is identified by the given key, or <c>null</c> if no entity
		/// was found for the given key.
		/// </returns>
		/// <exception cref="OperationException">
		/// Thrown when an unknown error occurs while looking for the entity.
		/// </exception>
		// TODO: Is there any use case for using OperationResult<TEntity> here
		//       instead of returning an entity?
		public virtual async Task<TEntity?> FindByKeyAsync(TKey key, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			ArgumentNullException.ThrowIfNull(key, nameof(key));

			try {
				Logger.LogFindingEntityByKey(typeof(TEntity), key);

				var token = GetCancellationToken(cancellationToken);
				var result = await GetOrSetByKeyAsync(key, () => Repository.FindByKeyAsync(key, token), token);

				if (result == null) {
					Logger.LogEntityNotFound(typeof(TEntity), key);
				} else {
					Logger.LogEntityFoundByKey(typeof(TEntity), key);
				}

				return result;
			} catch (Exception ex) {
				LogEntityUnknownError(key, ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		/// <summary>
		/// Finds an entity in the repository that matches the given filter.
		/// </summary>
		/// <param name="query">
		/// The query to be used to look for the entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the first instance of <typeparamref name="TEntity"/> that
		/// matches the given filter, or <c>null</c> if no entity was found.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filters.
		/// </exception>
		/// <exception cref="OperationException">
		/// Thrown when an unknown error occurs while looking for the entity.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown when the given filter is <c>null</c>.
		/// </exception>
		/// <seealso cref="IFilterableRepository{TEntity}.FindAsync(IQuery, CancellationToken)"/>
		// TODO: Is there any use case for using OperationResult<TEntity> here
		//       instead of returning the entity?
		public virtual async Task<TEntity?> FindFirstAsync(IQuery query, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			try {
				Logger.LogFindingFirstEntityByQuery(typeof(TEntity));

				return await FilterableRepository.FindAsync(query, GetCancellationToken(cancellationToken));
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		/// <summary>
		/// Finds the first entity in the repository that matches the 
		/// given filter expression.
		/// </summary>
		/// <param name="filter">
		/// The filter expression to be used to look for the entity.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the first instance of <typeparamref name="TEntity"/> that
		/// mathces the given filter, or <c>null</c> if no entity was found.
		/// </returns>
		/// <seealso cref="FindFirstAsync(IQuery, CancellationToken?)"/>
		public Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken? cancellationToken = null)
			=> FindFirstAsync(filter == null ? Query.Empty : new QueryBuilder<TEntity>().Where(filter), cancellationToken);

		/// <summary>
		/// Finds all the entities in the repository that match the given filter.
		/// </summary>
		/// <param name="query">
		/// The query to be used to look for the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a list of <typeparamref name="TEntity"/> that match the
		/// given filter.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filters.
		/// </exception>
		/// <exception cref="OperationException">
		/// Thrown when an unknown error occurs while looking for the entities.
		/// </exception>
		// TODO: Is there any use case for using OperationResult<IList<TEntity>> here
		//       instead of returning a list of entities?
		public virtual async Task<IList<TEntity>> FindAllAsync(IQuery query, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			try {
				Logger.LogFindingAllEntitiesByQuery(typeof(TEntity));

				return await FilterableRepository.FindAllAsync(query, GetCancellationToken(cancellationToken));
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		/// <summary>
		/// Finds all the entities in the repository that match the given 
		/// filter expression.
		/// </summary>
		/// <param name="filter">
		/// The filter expression to be used to look for the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// This method is a shortcut to the <see cref="FindAllAsync(IQuery,CancellationToken?)"/>
		/// using an instance of <see cref="ExpressionQueryFilter{TEntity}"/> as
		/// argument.
		/// </remarks>
		/// <returns>
		/// Returns a list of <typeparamref name="TEntity"/> that match the
		/// given filter.
		/// </returns>
		/// <seealso cref="FindAllAsync(IQuery,CancellationToken?)"/>
		/// <seealso cref="ExpressionQueryFilter{TEntity}"/>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filters.
		/// </exception>
		public Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken? cancellationToken = null)
			=> FindAllAsync(Query.Where(filter), cancellationToken);

		/// <summary>
		/// Counts the number of entities in the repository that match
		/// the given filter.
		/// </summary>
		/// <param name="filter">
		/// The filter to be used to look for the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the number of entities that match the given filter.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown when the repository does not support filters.
		/// </exception>
		/// <exception cref="OperationException">
		/// Thrown when an unknown error occurs while looking for the entities.
		/// </exception>
		public virtual Task<long> CountAsync(IQueryFilter filter, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			if (!SupportsFilters)
				throw new NotSupportedException("The repository does not support filters");

			ArgumentNullException.ThrowIfNull(filter, nameof(filter));

			try {
				Logger.LogCountingEntities(typeof(TEntity));

				return FilterableRepository.CountAsync(filter, GetCancellationToken(cancellationToken));
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}

		/// <summary>
		/// Counts the number of entities in the repository that match
		/// the given filter expression.
		/// </summary>
		/// <param name="filter">
		/// The filter expression to be used to look for the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// <para>
		/// This method is a shortcut to the <see cref="CountAsync(IQueryFilter,CancellationToken?)"/>
		/// overload, using a <see cref="ExpressionQueryFilter{TEntity}"/> as
		/// argument of the method.
		/// </para>
		/// <para>
		/// When the given filter is <c>null</c>, the method uses an
		/// instance of <see cref="QueryFilter.Empty"/> as argument,
		/// that has the effect to count all the entities in the repository.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns the number of entities that match the given filter.
		/// </returns>
		public Task<long> CountAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken? cancellationToken = null)
			=> CountAsync(filter == null ? QueryFilter.Empty : QueryFilter.Where(filter), cancellationToken);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="query"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="OperationException"></exception>
		public virtual async Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query, CancellationToken? cancellationToken = null) {
			ThrowIfDisposed();

			if (!SupportsPaging)
				throw new NotSupportedException("The repository does not support paging");

			try {
				// TODO: log this operation

				return await PageableRepository.GetPageAsync(query, GetCancellationToken(cancellationToken));
			} catch (Exception ex) {
				LogUnknownError(ex);
				throw new OperationException(EntityErrorCodes.UnknownError, "Could not look for the entity", ex);
			}
		}
	}
}
