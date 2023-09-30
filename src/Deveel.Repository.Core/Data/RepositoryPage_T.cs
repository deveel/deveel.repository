using System;

namespace Deveel.Data {
	/// <summary>
	/// The strongly typed page from a repository, obtained from a query
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="RepositoryPageRequest{TEntity}"/>
	/// <seealso cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
	public class RepositoryPage<TEntity> where TEntity : class {
		/// <summary>
		/// Constructs the result referencing the original request, a count
		/// of the items in the repository and optionally a list of items in the page
		/// </summary>
		/// <param name="request">The original page request</param>
		/// <param name="totalItems">The total number of items in the context
		/// of the request given (filtered and sorted).</param>
		/// <param name="items">The list of items included in the page</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown if the number of total items is smaller than zero.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the <paramref name="request"/> is <c>null</c>.
		/// </exception>
		public RepositoryPage(RepositoryPageRequest<TEntity> request, int totalItems, IEnumerable<TEntity>? items = null){
			if (totalItems < 0)
				throw new ArgumentOutOfRangeException(nameof(totalItems), "The number of total items must be zero or more");

			Request = request ?? throw new ArgumentNullException(nameof(request));
			TotalItems = totalItems;
			Items = items;
		}

		/// <summary>
		/// Gets a reference to the request
		/// </summary>
		public RepositoryPageRequest<TEntity> Request { get; }

		/// <summary>
		/// Gets a count of the total items in the repository
		/// for the context of the request
		/// </summary>
		public int TotalItems { get; }

		/// <summary>
		/// Gets a list of items included in the page
		/// </summary>
		public IEnumerable<TEntity>? Items { get; set; }

		/// <summary>
		/// Gets a count of the total available pages
		/// that can be requested from the repository
		/// </summary>
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / Request.Size);

		/// <summary>
		/// Creates an empty page response to the given request
		/// </summary>
		/// <param name="page">
		/// The request that originated the page
		/// </param>
		/// <returns>
		/// Returns a new instance of <see cref="RepositoryPage{TEntity}"/> that
		/// represents an empty page.
		/// </returns>
		public static RepositoryPage<TEntity> Empty(RepositoryPageRequest<TEntity> page) => new RepositoryPage<TEntity>(page, 0);
	}
}