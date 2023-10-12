namespace Deveel.Data.Caching {
	/// <summary>
	/// Provides a set of options for the caching of entities.
	/// </summary>
	public class EntityCacheOptions {
		/// <summary>
		/// Gets or sets the maximum expiration
		/// time for the cached entities.
		/// </summary>
		public TimeSpan? Expiration { get; set; }
	}
}
