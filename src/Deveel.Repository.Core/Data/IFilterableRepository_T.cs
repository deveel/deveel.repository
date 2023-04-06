using System;

namespace Deveel.Data {
    public interface IFilterableRepository<TEntity> : IRepository<TEntity>, IFilterableRepository where TEntity : class {
        /// <summary>
        /// Finds the first item in the repository that matches the given filtering condition
        /// </summary>
        /// <param name="filter">The filter used to identify the item</param>
        /// <param name="cancellationToken"></param>
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
        new Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

        new Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    }
}
