using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that is capable of returning a page of items
	/// contained in the underlying storage.
	/// </summary>
    public interface IPageableRepository : IRepository {
        /// <summary>
        /// Gets a page of items from the repository
        /// </summary>
        /// <param name="request">The request to obtain a given page from the repository. This
        /// object provides the number of the page, the size of the items to return, filters and
        /// sorting order.</param>
        /// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
        /// <returns>
        /// Returns an instance of <see cref="RepositoryPage"/> that provides the
        /// page items and a count of total items.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if an error occurred while retrieving the page
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the request includes filters or sorts and filtering or sorting 
		/// capabilities are not supported by the implementation of the repository
        /// </exception>
        /// <seealso cref="RepositoryPage"/>
        Task<RepositoryPage> GetPageAsync(RepositoryPageRequest request, CancellationToken cancellationToken = default);
    }
}
