using System;

namespace Deveel.Data
{
    /// <summary>
    /// The result of the request to obtain a page 
    /// of entities from a repository
    /// </summary>
    /// <seealso cref="IRepository.GetPageAsync(PageRequest, CancellationToken)"/>
    public class PaginatedResult
    {
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
        public PaginatedResult(PageRequest request, int totalItems, IEnumerable<IEntity>? items = null)
        {
            if (totalItems < 0)
                throw new ArgumentOutOfRangeException(nameof(totalItems), "The number of total items must be zero or more");

            Request = request ?? throw new ArgumentNullException(nameof(request));
            TotalItems = totalItems;
            Items = items;
        }

        /// <summary>
        /// Gets a reference to the request
        /// </summary>
        public PageRequest Request { get; }

        /// <summary>
        /// Gets a count of the total items in the repository
        /// for the context of the request
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// Gets a list of items included in the page
        /// </summary>
        public IEnumerable<IEntity>? Items { get; set; }

        /// <summary>
        /// Gets a count of the total available pages
        /// that can be requested from the repository
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / Request.Size);

        /// <summary>
        /// Creates an empty page result 
        /// </summary>
        /// <param name="request">The original page request</param>
        /// <returns>
        /// Returns an instance of <see cref="PaginatedResult"/> that
        /// contains no results and no pages.
        /// </returns>
        public static PaginatedResult Empty(PageRequest request) => new PaginatedResult(request, 0);
    }
}
