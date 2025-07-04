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
using Finbuckle.MultiTenant.EntityFrameworkCore;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	/// <summary>
	/// A repository that uses an <see cref="DbContext"/> to access the data
	/// of the entities.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of the entity managed by the repository.
	/// </typeparam>
	public class EntityRepository<TEntity> : EntityRepository<TEntity, object>,
        IRepository<TEntity>,
        IFilterableRepository<TEntity>,
        IQueryableRepository<TEntity>,
		IPageableRepository<TEntity>,
		ITrackingRepository<TEntity>
        where TEntity : class {

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/>.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="logger">
		/// A logger used to log the operations of the repository.
		/// </param>
		/// <remarks>
		/// When the given <paramref name="context"/> implements the <see cref="IMultiTenantDbContext"/>
		/// the repository will use the tenant information to access the data.
		/// </remarks>
        public EntityRepository(DbContext context, ILogger<EntityRepository<TEntity>>? logger = null)
            : base(context, (context as IMultiTenantDbContext)?.TenantInfo, logger) {
        }

		/// <summary>
		/// Constructs the repository using the given <see cref="DbContext"/> for
		/// the given tenant.
		/// </summary>
		/// <param name="context">
		/// The <see cref="DbContext"/> used to access the data of the entities.
		/// </param>
		/// <param name="tenantInfo">
		/// The information about the tenant that the repository will use to access the data.
		/// </param>
		/// <param name="logger">
		/// The logger used to log the operations of the repository.
		/// </param>
		public EntityRepository(DbContext context, ITenantInfo? tenantInfo, ILogger<EntityRepository<TEntity>>? logger = null)
            : base(context, tenantInfo, (ILogger?) logger) {
        }

        internal EntityRepository(DbContext context, ITenantInfo? tenantInfo, ILogger? logger = null) 
			: base(context, tenantInfo, logger) {
        }
	}
}
