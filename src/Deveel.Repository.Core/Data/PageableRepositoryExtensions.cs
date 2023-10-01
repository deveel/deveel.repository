namespace Deveel.Data {
	/// <summary>
	/// Defines a set of extension methods for the <see cref="IPageableRepository{TEntity}"/>
	/// that allows to retrieve a page of entities from the repository.
	/// </summary>
	public static class PageableRepositoryExtensions {
		/// <summary>
		/// Gets a page of entities from the repository,
		/// given a page number and a page size
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entities are retrieved.
		/// </param>
		/// <param name="page">
		/// The number of the page to retrieve from the repository.
		/// </param>
		/// <param name="size">
		/// The size of the page to retrieve from the repository.
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation.
		/// </param>
		/// <remarks>
		/// This method is a shortcut for the invocation of
		/// <see cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>,
		/// without filtering and sorting.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage{TEntity}"/> that
		/// is the result of the query.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the given page number is less than 1, or
		/// if the given page size is less than 0.
		/// </exception>
		public static Task<RepositoryPage<TEntity>> GetPageAsync<TEntity>(this IPageableRepository<TEntity> repository, int page, int size, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.GetPageAsync(new RepositoryPageRequest<TEntity>(page, size), cancellationToken);

		/// <summary>
		/// Gets a page of entities from the repository,
		/// given the request object that defines the scope
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entities are retrieved.
		/// </param>
		/// <param name="request">
		/// The request object that defines the scope of the page to retrieve.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage{TEntity}"/> that
		/// is the result of the query.
		/// </returns>
		public static RepositoryPage<TEntity> GetPage<TEntity>(this IPageableRepository<TEntity> repository, RepositoryPageRequest<TEntity> request)
			where TEntity : class
			=> repository.GetPageAsync(request).GetAwaiter().GetResult();
	}
}
