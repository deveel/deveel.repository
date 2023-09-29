using System;

namespace Deveel.Data {
    /// <summary>
    /// The contract defining a repository of entities, accessible
    /// for read and write operations
    /// </summary>
    /// <typeparam name="TEntity">The type of entity handled by the repository</typeparam>
    public interface IRepository<TEntity> : IRepository where TEntity : class {
		/// <summary>
		/// Gets the unique identifier of the entity given
		/// </summary>
		/// <param name="entity">
		/// The instance of the entity to get the identifier of
		/// </param>
		/// <returns>
		/// Returns a string that is the unique identifier of the entity
		/// within the repository, or <c>null</c> if the entity is not
		/// identified.
		/// </returns>
		string? GetEntityId(TEntity entity);

        /// <summary>
        /// Adds a new entity into the repository
        /// </summary>
        /// <param name="entity">The entity to be added</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns the unique identifier of the entity added.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while adding the entity
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        Task<string> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Adds a list of entities in the repository in one single operation
		/// </summary>
		/// <param name="entities">The enumeration of the entities to be added</param>
		/// <param name="cancellationToken"></param>
		/// <remarks>
		/// <para>
		/// The operation is intended to be <c>all-or-nothing</c> fashion, where it
		/// will succeed only if all the items in the list will be created. Anyway, the
		/// underlying storage system might have persisted some of the items before a
		/// failure: to prevent the scenario of a partial creation of the set, the
		/// callers should consider the 
		/// <see cref="ITransactionalRepository{TEntity}.AddRangeAsync(IDataTransaction, IEnumerable{TEntity}, CancellationToken)"/>
		/// overload, where transactions are available.
		/// </para>
		/// </remarks>
		/// <returns>
		/// Returns an ordered list of the unique identifiers of the entiies created
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown if it an error occurred while adding one or more entities
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the provided list of <paramref name="entities"/> is <c>null</c>
		/// </exception>
		Task<IList<string>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);


        /// <summary>
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="entity">The entity instance to be updated</param>
        /// <param name="cancellationToken"></param>
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
        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity from the repository
        /// </summary>
        /// <param name="entity">The entity to be deleted</param>
        /// <param name="cancellationToken"></param>
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
        Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);

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
        new Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}