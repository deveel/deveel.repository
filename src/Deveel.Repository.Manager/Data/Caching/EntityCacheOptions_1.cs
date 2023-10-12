namespace Deveel.Data.Caching {
	/// <summary>
	/// A strongly-typed set of options for the caching of entities.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public sealed class EntityCacheOptions<TEntity> : EntityCacheOptions where TEntity : class {
	}
}
