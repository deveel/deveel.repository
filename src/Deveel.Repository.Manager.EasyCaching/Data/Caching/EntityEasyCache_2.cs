﻿using EasyCaching.Core;

using Microsoft.Extensions.Options;

namespace Deveel.Data.Caching {
	/// <summary>
	/// An implementation of <see cref="IEntityCache{TEntity}"/> that
	/// uses the EasyCaching library to store the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity to cache.
	/// </typeparam>
	/// <typeparam name="TCached">
	/// The type of the cached version of the entity.
	/// </typeparam>
	public class EntityEasyCache<TEntity, TCached> : IEntityCache<TEntity> 
		where TEntity : class 
		where TCached : class {
		private readonly IEasyCachingProvider cacheProvider;
		private readonly EntityCacheOptions<TEntity>? options;
		private readonly IEntityCacheKeyGenerator<TEntity>? keyGenerator;
		private readonly IEntityEasyCacheConverter<TEntity, TCached>? converter;

		/// <summary>
		/// Constructs the cache with the given provider and options.
		/// </summary>
		/// <param name="cacheProvider">
		/// The EasyCaching provider to use to store the entities.
		/// </param>
		/// <param name="options">
		/// A set of options to configure the entity cache.
		/// </param>
		/// <param name="keyGenerator">
		/// A generator to create the keys for the entities.
		/// </param>
		/// <param name="converter">
		/// A service to convert the entity to and from the cached version.
		/// </param>
		public EntityEasyCache(IEasyCachingProvider cacheProvider, 
			IOptions<EntityCacheOptions<TEntity>>? options = null, 
			IEntityCacheKeyGenerator<TEntity>? keyGenerator = null,
			IEntityEasyCacheConverter<TEntity, TCached>? converter = null) {
			this.cacheProvider = cacheProvider;
			this.options = options?.Value;
			this.keyGenerator = keyGenerator;
			this.converter = converter;
		}

		/// <summary>
		/// Gets the expiration time for the cached entities.
		/// </summary>
		protected virtual TimeSpan Expiration => options?.Expiration ?? TimeSpan.FromMinutes(5);

		/// <summary>
		/// Generates all the possible keys for the given entity
		/// </summary>
		/// <param name="entity">
		/// The entity to generate the keys for.
		/// </param>
		/// <remarks>
		/// The default implementation of this method will use the
		/// instance of <see cref="IEntityCacheKeyGenerator{TEntity}"/>
		/// availble in the execution context.
		/// </remarks>
		/// <returns>
		/// Returns an array of strings that are the keys
		/// for the given entity in the cache.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the key generation is not supported by the cache.
		/// </exception>
		protected virtual string[] GenerateCacheKeys(TEntity entity) {
			if (keyGenerator == null)
				throw new NotSupportedException("The key generator is not defined for this cache");

			return keyGenerator.GenerateAllKeys(entity);
		}

		/// <summary>
		/// Attempts to convert the given entity to a version
		/// that can be cached.
		/// </summary>
		/// <param name="entity">
		/// The entity instance to convert.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TCached"/>
		/// that is the result of the conversion.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the conversion is not supported by the cache.
		/// </exception>
		protected virtual TCached ConvertToCached(TEntity entity) {
			if (converter != null)
				return converter.ConvertToCached(entity);

			if (entity is TCached cached)
				return cached;

			throw new NotSupportedException("The converter is not defined for this cache");
		}

		/// <summary>
		/// Attempts to convert back the given cached version of the entity
		/// to the original entity.
		/// </summary>
		/// <param name="cached">
		/// The instance of the cached version of the entity.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/>
		/// that is the result of the conversion.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// Thrown if the conversion is not supported by the cache.
		/// </exception>
		protected virtual TEntity ConvertFromCached(TCached cached) {
			if (converter != null)
				return converter.ConvertFromCached(cached);

			if (cached is TEntity entity)
				return entity;

			throw new NotSupportedException("The converter is not defined for this cache");
		}

		private Func<Task<TCached>> Factory(Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken) {
			return async () => {
				var entity = await valueFactory();
				if (entity == null)
					return null;

				return ConvertToCached(entity);
			};
		}

		/// <inheritdoc/>
		public async Task<TEntity?> GetOrSetAsync(string cacheKey, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default) {
			try {
				var factory = Factory(valueFactory, cancellationToken);

				var result = await cacheProvider.GetAsync<TCached>(cacheKey, factory, Expiration, cancellationToken);

				if (result.HasValue)
					return ConvertFromCached(result.Value);

				return null;
			} catch (Exception ex) {

				throw new RepositoryException("Could not get or set the entity in the cache", ex);
			}
		}

		/// <inheritdoc/>
		public async Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) {
			try {
				var keys = GenerateCacheKeys(entity);

				await cacheProvider.RemoveAllAsync(keys, cancellationToken);
			} catch (Exception ex) {
				throw new RepositoryException("Unable to remove the entity from the cache", ex);
			}
		}

		/// <inheritdoc/>
		public async Task SetAsync(TEntity entity, CancellationToken cancellationToken = default) {
			try {
				var keys = GenerateCacheKeys(entity);

				foreach (var key in keys) {
					await cacheProvider.SetAsync(key, entity, Expiration, cancellationToken);
				}
			} catch (Exception ex) {
				throw new RepositoryException("Unable to set the entity in the cache", ex);
			}
		}
	}
}