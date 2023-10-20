namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IQuery"/> to provide
	/// functions for filtering and sorting queryable
	/// objects.
	/// </summary>
	public static class QueryExtensions {
		/// <summary>
		/// Applies the query to the given <see cref="IQueryable{TEntity}"/>
		/// to filter and sort the results.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to apply the query to.
		/// </typeparam>
		/// <param name="query">
		/// The query to be applied to the queryable.
		/// </param>
		/// <param name="queryable">
		/// The queryable to apply the query to.
		/// </param>
		/// <returns>
		/// Returns the <see cref="IQueryable{TEntity}"/> that was
		/// filtered and sorted.
		/// </returns>
		public static IQueryable<TEntity> Apply<TEntity>(this IQuery query, IQueryable<TEntity> queryable)
			where TEntity : class {
			ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

			if (query.Filter != null)
				queryable = query.Filter.Apply<TEntity>(queryable);

			if (query.Sort != null)
				queryable = query.Sort.Apply<TEntity>(queryable);

			return queryable;
		}

		/// <summary>
		/// Gets a value indicating if the query has a filter.
		/// </summary>
		public static bool HasFilter(this IQuery query) => !(query.Filter?.IsEmpty() ?? true);

	}
}
