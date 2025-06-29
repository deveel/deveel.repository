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

using Microsoft.EntityFrameworkCore;

namespace Deveel.Data {
	/// <summary>
	/// Provides functions to construct tenant-specific
	/// configurations for a <see cref="DbContext"/>.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	public interface IDbContextOptionsFactory<TContext> where TContext : DbContext {
		/// <summary>
		/// Create a set of options for the given tenant.
		/// </summary>
		/// <param name="tenantInfo">
		/// The information about the tenant to create the options for.
		/// </param>
		/// <remarks>
		/// The implementation of the <see cref="ITenantInfo"/> instance
		/// provides by default a connection string, that can be used to
		/// configure the options to connect to the database.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="DbContextOptions{TContext}"/>
		/// that is specific for the given tenant.
		/// </returns>
		DbContextOptions<TContext> Create(DbTenantInfo tenantInfo);
	}
}
