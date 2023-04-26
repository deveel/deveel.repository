using System;

namespace Deveel.Data {
	/// <summary>
	/// A repository that provides the capability of persisting the
	/// states of entities
	/// </summary>
	/// <typeparam name="TStatus">
	/// The status code of the states of an entity
	/// </typeparam>
	public interface IStateRepository<TStatus> : IRepository {
		/// <summary>
		/// Gets the listing of the states of the entity
		/// </summary>
		/// <param name="entity">
		/// The entity that holds the states
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a list of <see cref="EntityStateInfo{TStatus}"/> that are the states
		/// currently held by the <paramref name="entity"/> given.
		/// </returns>
		Task<IList<EntityStateInfo<TStatus>>> GetStatesAsync(object entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Adds a new state to the entity
		/// </summary>
		/// <param name="entity">The entity that will holds the state</param>
		/// <param name="stateInfo">The new state to be added</param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a task that returns when the operation is completed
		/// </returns>
		Task AddStateAsync(object entity, EntityStateInfo<TStatus> stateInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes a given state from the entity
		/// </summary>
		/// <param name="entity">
		/// The entity that holds the state to be removed
		/// </param>
		/// <param name="stateInfo">
		/// An object that describes the state to be removed
		/// </param>
		/// <param name="cancellationToken"></param>
		/// <remarks>
		/// While adding new states to an entity is a logical operation,
		/// to implement a state machine, it might also be useful to
		/// have functions to remove existing states from an entity: to
		/// identify state to be removed, implementations of the repository
		/// might use different evaluation strategies, depending on the
		/// implementation. The recommended approach to identify a state
		/// to be removed is to match the status and time-stamp given
		/// by the <see cref="EntityStateInfo{TStatus}"/> object passed as argument.
		/// </remarks>
		/// <returns>
		/// Returns a task that returns when the operation is completed
		/// </returns>
		Task RemoveStateAsync(object entity, EntityStateInfo<TStatus> stateInfo, CancellationToken cancellationToken= default);
	}
}
