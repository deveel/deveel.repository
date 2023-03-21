using System;

namespace Deveel.Data {
	/// <summary>
	/// A page of entities from a repository, obtained from a query
	/// </summary>
	/// <seealso cref="IRepository.GetPageAsync(RepositoryPageRequest, CancellationToken)"/>
	public class RepositoryPage {
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
		public RepositoryPage(RepositoryPageRequest request, int totalItems, IEnumerable<object>? items = null) {
			if (totalItems < 0)
				throw new ArgumentOutOfRangeException(nameof(totalItems), "The number of total items must be zero or more");

			Request = request ?? throw new ArgumentNullException(nameof(request));
			TotalItems = totalItems;
			Items = items;
		}

		/// <summary>
		/// Gets a reference to the request
		/// </summary>
		public RepositoryPageRequest Request { get; }

		/// <summary>
		/// Gets a count of the total items in the repository
		/// for the context of the request
		/// </summary>
		public int TotalItems { get; }

		/// <summary>
		/// Gets a list of items included in the page
		/// </summary>
		public IEnumerable<object>? Items { get; set; }

		/// <summary>
		/// Gets a count of the total available pages
		/// that can be requested from the repository
		/// </summary>
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / Request.Size);

		public RepositoryPage<TEntity> As<TEntity>() where TEntity : class
			=> new RepositoryPage<TEntity>(Request, TotalItems, Items?.Cast<TEntity>());

		/// <summary>
		/// Creates an empty page result 
		/// </summary>
		/// <param name="request">The original page request</param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage"/> that
		/// contains no results and no pages.
		/// </returns>
		public static RepositoryPage Empty(RepositoryPageRequest request) => new RepositoryPage(request, 0);
	}
}