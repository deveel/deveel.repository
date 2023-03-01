using System;

namespace Deveel.Data {
    /// <summary>
    /// The contract defining a repository of entities, accessible
    /// for read and write operations
    /// </summary>
    /// <typeparam name="TEntity">The type of entity handled by the repository</typeparam>
    public interface IRepository<TEntity> : IRepository where TEntity : class, IEntity {
        /// <summary>
        /// Creates a new entity in the repository
        /// </summary>
        /// <param name="entity">The entity to create</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns the unique identifier of the entity created.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while creating the entity
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        Task<string> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<IList<string>> CreateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        Task<IList<string>> CreateAsync(IDataTransaction transaction, IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);


        /// <summary>
        /// Creates a new entity in the repository
        /// </summary>
        /// <param name="transaction">A transaction that isolates the access
        /// to the data store used by the repository</param>
        /// <param name="entity">The entity to create</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        /// Returns the unique identifier of the entity created.
        /// </returns>
        /// <exception cref="RepositoryException">
        /// Thrown if it an error occurred while creating the entity
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <paramref name="entity"/> is <c>null</c>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the provided <paramref name="transaction"/> is not compatible
        /// with the underlying storage of the repository
        /// </exception>
        /// <seealso cref="IDataTransactionFactory"/>
        Task<string> CreateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken = default);

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
        /// Updates an existing entity in the repository
        /// </summary>
        /// <param name="transaction">A transaction that isolates the access
        /// to the data store used by the repository</param>
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
        /// <exception cref="ArgumentException">
        /// Thrown if the provided <paramref name="transaction"/> is not compatible
        /// with the underlying storage of the repository
        /// </exception>
        /// <seealso cref="IDataTransactionFactory"/>
        Task<bool> UpdateAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity from the repository
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
        Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity from the repository
        /// </summary>
        /// <param name="transaction">A transaction that isolates the access
        /// to the data store used by the repository</param>
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
        /// <exception cref="ArgumentException">
        /// Thrown if the provided <paramref name="transaction"/> is not compatible
        /// with the underlying storage of the repository
        /// </exception>
        /// <seealso cref="IDataTransactionFactory"/>
        Task<bool> DeleteAsync(IDataTransaction transaction, TEntity entity, CancellationToken cancellationToken = default);

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