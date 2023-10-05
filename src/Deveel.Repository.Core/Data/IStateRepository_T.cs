// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Deveel.Data {
	/// <summary>
	/// A repository that provides the capability of persisting the
	/// states of typed entities
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity managed by the repository</typeparam>
	/// <typeparam name="TStatus">The status code of the states of an entity</typeparam>
	public interface IStateRepository<TEntity, TStatus> : IRepository<TEntity> 
		where TEntity : class {
		/// <summary>
		/// Gets the listing of the states of the entity
		/// </summary>
		/// <param name="entity">The entity that holds the states</param>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a list of <see cref="EntityStateInfo{TStatus}"/> that are the states
		/// currently held by the <paramref name="entity"/> given.
		/// </returns>
		Task<IList<EntityStateInfo<TStatus>>> GetStatesAsync(TEntity entity, CancellationToken cancellationToken = default);

		/// <summary>
		/// Adds a new state to the entity
		/// </summary>
		/// <param name="entity">The entity that will holds the state</param>
		/// <param name="stateInfo">The new state to be added</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task AddStateAsync(TEntity entity, EntityStateInfo<TStatus> stateInfo, CancellationToken cancellationToken = default);

		/// <summary>
		/// Removes a state from the entity
		/// </summary>
		/// <param name="entity">The entity that holds the state to be removed</param>
		/// <param name="stateInfo">The state to be removed</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task RemoveStateAsync(TEntity entity, EntityStateInfo<TStatus> stateInfo, CancellationToken cancellationToken = default);
	}
}
