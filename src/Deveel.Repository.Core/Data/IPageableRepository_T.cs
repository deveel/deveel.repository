using System;

namespace Deveel.Data {
    public interface IPageableRepository<TEntity> : IRepository<TEntity>, IPageableRepository where TEntity : class {

        /// <summary>
        /// Gets a page of items from the repository
        /// </summary>
        /// <param name="request">The request to obtain a given page from the repository. This
        /// object provides the number of the page, the size of the items to return, filters and
        /// sorting order.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns an instance of <see cref="RepositoryPage"/> that provides the
        /// page items and a count of total items.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if an error occurred while retrieving the page
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the filters or the sorting capabilities are not provided by the
        /// implementation of the repository
        /// </exception>
        /// <seealso cref="RepositoryPage"/>
        /// <seealso cref="SupportsPaging"/>
        /// <seealso cref="SupportsFilters"/>
        Task<RepositoryPage<TEntity>> GetPageAsync(RepositoryPageRequest<TEntity> request, CancellationToken cancellationToken = default);
    }
}
