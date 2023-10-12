namespace Deveel.Data.Caching {
	/// <summary>
	/// Provides a contract for caching entities of a given type.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity to cache.
	/// </typeparam>
	public interface IEntityCache<TEntity> where TEntity : class {
		/// <summary>
		/// Gets the given entity from the cache, if available,
		/// and uses the given factory to create the entity if to
		/// then cache it.
		/// </summary>
		/// <param name="cacheKey">
		/// The unique key used to identify the entity in the cache.
		/// </param>
		/// <param name="valueFactory">
		/// A function that is used to create the entity to cache,
		/// if this was not found in the cache.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns the entity from the cache, if available, or
		/// <c>null</c> if the entity was not found in the cache
		/// and the factory was not able to create it.
		/// </returns>
		Task<TEntity?> GetOrSetAsync(string cacheKey, Func<Task<TEntity?>> valueFactory, CancellationToken cancellationToken = default);

		/// <summary>
		/// Sets the given entity in the cache.
		/// </summary>
		/// <param name="entity">
		/// The instance of the entity to be cached.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <remarks>
		/// This method will generate all possible keys for the given
		/// entity and will cache it with all of them.
		/// </remarks>
		/// <returns>
		/// Returns a task that when completed will cache the entity.
		/// </returns>
		/// <seealso cref="IEntityCacheKeyGenerator{TEntity}"/>
		Task SetAsync(TEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes the entity from the cache.
		/// </summary>
		/// <param name="entity">
		/// The entity to be removed from the cache.
		/// </param>
		/// <remarks>
		/// This method will generate all possible keys for the given
		/// entity, and will remove all of them from the cache.
		/// </remarks>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns a task that when completed will remove the entity.
		/// </returns>
		/// <seealso cref="IEntityCacheKeyGenerator{TEntity}"/>
		Task RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
	}
}
