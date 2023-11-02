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

namespace Deveel.Data {
	/// <summary>
	/// Represents an provider of strongly-typed repositories that
	/// are isolating the entities of a given tenant
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity handled by the repository instances
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of key used to identify the entities
	/// managed by the repository instances
	/// </typeparam>
	public interface IRepositoryProvider<TEntity, TKey> where TEntity : class {
		/// <summary>
		/// Gets a repository instance that is isolating the entities
		/// for a tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is provided.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IRepository{TEntity}"/> that
		/// is isolating the entities for the given tenant.
		/// </returns>
		Task<IRepository<TEntity, TKey>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
	}
}
