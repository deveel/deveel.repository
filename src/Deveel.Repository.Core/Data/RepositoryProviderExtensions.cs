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
	/// Extends the <see cref="IRepositoryProvider{TEntity}"/> interface
	/// with further methods to resolve a repository.
	/// </summary>
	/// <seealso cref="IRepositoryProvider{TEntity}"/>
	public static class RepositoryProviderExtensions {
		/// <summary>
		/// Synchronously resolves the repository for the given tenant.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="provider">
		/// The instance of the provider that resolves the repository.
		/// </param>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is resolved.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IRepository{TEntity}"/> that
		/// is isolated for the given tenant.
		/// </returns>
		public static IRepository<TEntity> GetRepository<TEntity>(this IRepositoryProvider<TEntity> provider, string tenantId)
			where TEntity : class
			=> provider.GetRepositoryAsync(tenantId).GetAwaiter().GetResult();
	}
}
