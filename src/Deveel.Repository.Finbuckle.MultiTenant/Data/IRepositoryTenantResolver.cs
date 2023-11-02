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

using Finbuckle.MultiTenant;

namespace Deveel.Data {
	/// <summary>
	/// A service that resolves the tenant information
	/// from a given identifier.
	/// </summary>
	public interface IRepositoryTenantResolver {
		/// <summary>
		/// Finds the tenant information from the given identifier.
		/// </summary>
		/// <param name="tenantId">
		/// The unique identifier of the tenant to find.
		/// </param>
		/// <param name="cancellationToken">
		/// A token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="ITenantInfo"/> that
		/// describes the tenant identified by the given identifier,
		/// or <c>null</c> if no tenant was found.
		/// </returns>
		Task<ITenantInfo?> FindTenantAsync(string tenantId, CancellationToken cancellationToken = default);
	}
}
