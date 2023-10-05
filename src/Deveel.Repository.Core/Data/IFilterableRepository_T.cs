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
	/// Represents a repository that can be filtered to retrieve a subset of
	/// the entities it contains.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The strongly typed entity that is stored in the repository
	/// </typeparam>
    public interface IFilterableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
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
		Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

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
		Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default);

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
        Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    }
}
