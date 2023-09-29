using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	/// <summary>
	/// Implementations of this repository can provide the capability
	/// to execute CRUD operations within the scope of a transaction
	/// </summary>
	public interface ITransactionalRepository {
		/// <summary>
		/// Creates a list of entities in the repository in one single operation, within
		/// the scope of a given transaction
		/// </summary>
		/// <param name="entities">The enumeration of the entities to be created</param>
		/// <param name="transaction">The transaction scope of the operation</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns the list of the unique identifiers of the entities created.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown if it an error occurred while creating one or more entities
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the provided list of <paramref name="entities"/> is <c>null</c>
		/// </exception>
		Task<IList<string>> AddRangeAsync(IDataTransaction transaction, IEnumerable<object> entities, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a new entity in the repository
		/// </summary>
		/// <param name="transaction">A transaction that isolates the access
		/// to the data store used by the repository</param>
		/// <param name="entity">The entity to create</param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
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
		Task<string> AddAsync(IDataTransaction transaction, object entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes an entity from the repository
		/// </summary>
		/// <param name="transaction">A transaction that isolates the access
		/// to the data store used by the repository</param>
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
		/// <exception cref="ArgumentException">
		/// Thrown if the provided <paramref name="transaction"/> is not compatible
		/// with the underlying storage of the repository
		/// </exception>
		/// <seealso cref="IDataTransactionFactory"/>
		Task<bool> RemoveAsync(IDataTransaction transaction, object entity, CancellationToken cancellationToken = default);

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
		Task<bool> UpdateAsync(IDataTransaction transaction, object entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Attempts to find in the repository an entity with the 
		/// given unique identifier
		/// </summary>
		/// <param name="transaction">A transaction that isolates the access
		/// to the data store used by the repository</param>
		/// <param name="id">The unique identifier of the entity to find</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns the instance of the entity associated to the given <paramref name="id"/>,
		/// or <c>null</c> if none entity was found.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// If the provided <paramref name="id"/> is <c>null</c> or empty
		/// </exception>
		Task<object?> FindByIdAsync(IDataTransaction transaction, string id, CancellationToken cancellationToken = default);
	}
}
