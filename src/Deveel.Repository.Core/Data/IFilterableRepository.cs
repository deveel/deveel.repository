using System;

namespace Deveel.Data {
    public interface IFilterableRepository : IRepository {
        /// <summary>
        /// Determines if at least one item in the repository exists for the
        /// given filtering conditions
        /// </summary>
        /// <param name="filter">The filter used to identify the items</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns <c>true</c> if at least one item in the inventory matches the given
        /// filter, otherwise returns <c>false</c>
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the repository does not support filtering
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="filter"/> is not supported by the repository
        /// </exception>
        /// <seealso cref="SupportsFilters" />
        Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts the number of items in the repository matching the given 
        /// filtering conditions
        /// </summary>
        /// <param name="filter">The filter used to identify the items</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Passing a <c>null</c> filter or passing <see cref="QueryFilter.Empty"/> as
        /// argument is equivalent to ask the repository not to use any filter, returning the 
        /// total count of all items int the inventory.
        /// </remarks>
        /// <returns>
        /// Returns the total count of items matching the given filtering conditions
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the repository does not support filtering
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="filter"/> is not supported by the repository
        /// </exception>
        /// <seealso cref="SupportsFilters" />
        Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds the first item in the repository that matches the given filtering condition
        /// </summary>
        /// <param name="filter">The filter used to identify the item</param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// Passing a <c>null</c> filter or passing <see cref="QueryFilter.Empty"/> as
        /// argument is equivalent to ask the repository not to use any filter, returning the first
        /// item from the inventory.
        /// </remarks>
        /// <returns>
        /// Returns the first item in the repository that matches the given filtering condition,
        /// or <c>null</c> if none of the items matches the condition.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the repository does not support filtering
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="filter"/> is not supported by the repository
        /// </exception>
        /// <seealso cref="SupportsFilters" />
        Task<IEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all the items in the repository that match the given
        /// filtering condition
        /// </summary>
        /// <param name="filter">The filter used to identify the items</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns a list of items from the repository that match the given filtering condition.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the repository does not support filtering
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="filter"/> is not supported by the repository
        /// </exception>
        /// <seealso cref="SupportsFilters" />
        Task<IList<IEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    }
}
