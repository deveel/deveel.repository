using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that can be filtered to retrieve a subset of
	/// the entities it contains.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The strongly typed entity that is stored in the repository
	/// </typeparam>
    public interface IFilterableRepository<TEntity> : IRepository<TEntity>, IFilterableRepository where TEntity : class {
        /// <summary>
        /// Finds the first item in the repository that matches the given filtering condition
        /// </summary>
        /// <param name="filter">The filter used to identify the item</param>
        /// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
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
        new Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

		/// <summary>
		/// Finds all the items in the repository that match the given filtering condition
		/// </summary>
		/// <param name="filter">
		/// The filter used to identify the items to be returned
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a list of items in the repository that match the given filtering condition,
		/// or an empty list if none of the items matches the condition.
		/// </returns>
        new Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    }
}
