using System;

namespace Deveel.Data {
    /// <summary>
    /// The contract defining a repository of entities, accessible
    /// for read and write operations
    /// </summary>
    public interface IRepository {
        /// <summary>
        /// Gets the type of the entity managed by the repository
        /// </summary>
        Type EntityType { get; }

		/// <summary>
		/// Gets the unique identifier of the entity given
		/// instance of the entity managed by the repository
		/// </summary>
		/// <param name="entity">
		/// The instance of the entity to get the identifier of
		/// </param>
		/// <remarks>
		/// In some contexts it might be possible that an entity is not
		/// immediately associated with an identifier upon creation, such as
		/// when the repository is not persistent, or if it is executing
		/// CRUD operations in a transactional context (<see cref="ITransactionalRepository"/>):
		/// this method should help to obtain the identifier of the entity.
		/// </remarks>
		/// <returns>
		/// Returns a string that is the unique identifier of the entity
		/// within the repository, or <c>null</c> if the entity is not
		/// identified
		/// </returns>
		string? GetEntityId(object entity);

        /// <summary>
        /// Adds an entity in the repository
        /// </summary>
        /// <param name="entity">The entity to be added</param>
        /// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
        /// <returns>
        /// Returns the unique identifier of the entity added.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while creating the entity
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        Task<string> AddAsync(object entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Adds a list of entities in the repository in one single operation
		/// </summary>
		/// <param name="entities">The list of the entities to be added</param>
		/// <param name="cancellationToken"></param>
        /// <remarks>
        /// <para>
        /// The operation is intended to be <c>all-or-nothing</c> fashion, where it
        /// will succeed only if all the items in the list will be created. Anyway, the
        /// underlying storage system might have persisted some of the items before a
        /// failure: to prevent the scenario of a partial creation of the set, the
        /// callers should consider the 
		/// <see cref="ITransactionalRepository.AddRangeAsync(IDataTransaction, IEnumerable{object}, CancellationToken)"/>
        /// overload, where transactions are available.
        /// </para>
        /// </remarks>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown if it an error occurred while adding one or more entities
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the provided list of <paramref name="entities"/> is <c>null</c>
		/// </exception>
		Task<IList<string>> AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to be removed</param>
        /// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
        /// <returns>
        /// Returns <c>true</c> if the entity was successfully removed 
        /// from the repository, otherwise <c>false</c>. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while deleting the entity
        /// </exception>
        Task<bool> RemoveAsync(object entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity instance to be updated</param>
        /// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
        /// <returns>
        /// Returns <c>true</c> if the entity was found and updated in 
        /// the repository, otherwise <c>false</c>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while updating the entity
        /// </exception>
        Task<bool> UpdateAsync(object entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to find in the repository an entity with the 
        /// given unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the entity to find</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns the instance of the entity associated to the given <paramref name="id"/>,
        /// or <c>null</c> if none entity was found.
        /// </returns>
		/// <exception cref="ArgumentNullException">
		/// If the provided <paramref name="id"/> is <c>null</c> or empty
		/// </exception>
        Task<object?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}