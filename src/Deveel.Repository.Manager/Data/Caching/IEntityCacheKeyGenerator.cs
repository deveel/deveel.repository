namespace Deveel.Data.Caching {
	/// <summary>
	/// A service that is used to generate the keys for caching
	/// a given entity.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity to cache.
	/// </typeparam>
	public interface IEntityCacheKeyGenerator<TEntity> where TEntity : class {
		/// <summary>
		/// Generates the given cache key using the given primary
		/// key of the entity.
		/// </summary>
		/// <param name="key">
		/// The instance of the primary key of the entity.
		/// </param>
		/// <returns>
		/// Returns a string that is the key to be used to cache
		/// or return the entity.
		/// </returns>
		string GenerateKey(object key);

		/// <summary>
		/// Generates the keys for the given entity.
		/// </summary>
		/// <param name="entity">
		/// The instance of the entity to generate the keys for.
		/// </param>
		/// <returns>
		/// Returns an array of strings that are the keys to be used
		/// to cache the entity.
		/// </returns>
		string[] GenerateAllKeys(TEntity entity);

		// TODO: We should provide a mechanism to generate a key
		//       form a filter expression
	}
}
