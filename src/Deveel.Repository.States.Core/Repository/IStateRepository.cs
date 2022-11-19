using System;

namespace Deveel.Repository {
	/// <summary>
	/// A repository that provides the capability of persisting the
	/// states of entities
	/// </summary>
	/// <typeparam name="TStatus">The status code of the states of an entity</typeparam>
	public interface IStateRepository<TStatus> : IRepository {
		/// <summary>
		/// Gets the listing of the states of the entity
		/// </summary>
		/// <param name="entity">The entity that holds the states</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a list of <see cref="StateInfo{TStatus}"/> that are the states
		/// currently held by the <paramref name="entity"/> given.
		/// </returns>
		Task<IList<StateInfo<TStatus>>> GetStatesAsync(IEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Adds a new state to the entity
		/// </summary>
		/// <param name="entity">The entity that will holds the state</param>
		/// <param name="stateInfo">The new state to be added</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task AddStateAsync(IEntity entity, StateInfo<TStatus> stateInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes a state from the entity
		/// </summary>
		/// <param name="entity">The entity that holds the state to be removed</param>
		/// <param name="stateInfo">The state to be removed</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task RemoveStateAsync(IEntity entity, StateInfo<TStatus> stateInfo, CancellationToken cancellationToken= default);
	}
}
