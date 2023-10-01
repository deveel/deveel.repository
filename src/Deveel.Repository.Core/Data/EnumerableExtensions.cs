namespace Deveel.Data {
	/// <summary>
	/// Extensions for the <see cref="IEnumerable{T}"/> to provide
	/// the capability to wrap the collection into a <see cref="IRepository{TEntity}"/>.
	/// </summary>
	public static class EnumerableExtensions {
		/// <summary>
		/// Makes the given collection of entities a <see cref="IRepository{TEntity}"/>.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entities in the collection.
		/// </typeparam>
		/// <param name="entities">
		/// The collection of entities to wrap.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IRepository{TEntity}"/> that wraps
		/// the given collection.
		/// </returns>
		public static IRepository<TEntity> AsRepository<TEntity>(this IEnumerable<TEntity> entities) where TEntity : class {
			return new RepositoryWrapper<TEntity>(entities);
		}
	}
}
