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

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

namespace Deveel.Data {
	/// <summary>
	/// A default implementation of <see cref="IRepositoryTenantResolver"/>
	/// that uses the registered instances of <see cref="IMultiTenantStore{TTenantInfo}"/>
	/// to resolve the tenant information.	
	/// </summary>
	/// <typeparam name="TTenantInfo">
	/// The type of the tenant information object to resolve.
	/// </typeparam>
	public class RepositoryTenantResolver<TTenantInfo> : IRepositoryTenantResolver where TTenantInfo : class, ITenantInfo, new() {
		private readonly IEnumerable<IMultiTenantStore<TTenantInfo>>? stores;

		/// <summary>
		/// Constructs the resolver by using the given stores.
		/// </summary>
		/// <param name="stores">
		/// The list of stores to use to resolve the tenant information.
		/// </param>
		public RepositoryTenantResolver(IEnumerable<IMultiTenantStore<TTenantInfo>>? stores) {
			this.stores = stores;
		}

		/// <inheritdoc/>
		public async Task<ITenantInfo?> FindTenantAsync(string tenantId, CancellationToken cancellationToken = default) {
			try {
				if (stores == null)
					return null;

				foreach (var store in stores) {
					var tenant = await store.TryGetAsync(tenantId);
					if (tenant != null)
						return tenant;

					tenant = await store.TryGetByIdentifierAsync(tenantId);

					if (tenant != null)
						return tenant;

					cancellationToken.ThrowIfCancellationRequested();
				}

				return null;
			} catch (Exception ex) {
				// TODO: should we log this?
				throw new RepositoryException("Unable to resolve the tenant because of an error", ex);
			}
		}
	}
}
