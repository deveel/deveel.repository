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
using System.Reflection;

using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of <see cref="IRepositoryProvider{TEntity}"/> that
	/// allows to create <see cref="EntityRepository{TEntity}"/> instances
	/// in a multi-tenant context.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity managed by the repository
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key of the entity managed by the repository.
	/// </typeparam>
	/// <typeparam name="TContext">
	/// The type of <see cref="DbContext"/> used to manage the entities
	/// </typeparam>
	/// <typeparam name="TTenantInfo">
	/// The type of the tenant that is is ued to resolve the context
	/// of the tenant.
	/// </typeparam>
	public class EntityRepositoryProvider<TContext, TEntity, TKey, TTenantInfo> : EntityRepositoryProviderBase<TContext, TEntity, TTenantInfo>,
		IRepositoryProvider<TEntity, TKey>
		where TContext : DbContext
		where TEntity : class
		where TTenantInfo : class, ITenantInfo, new() {

		/// <summary>
		/// Constructs the provider with the given options factory
		/// and tenant stores.
		/// </summary>
		/// <param name="optionsFactory">
		/// A service that is used to create the options for the context
		/// and a specific tenant.
		/// </param>
		/// <param name="tenantStores">
		/// A list of the available stores to retrieve the tenants from.
		/// </param>
		/// <param name="loggerFactory">
		/// A factory to create loggers for the repositories.
		/// </param>
		public EntityRepositoryProvider(
			IDbContextOptionsFactory<TContext> optionsFactory,
			IEnumerable<IMultiTenantStore<TTenantInfo>> tenantStores,
			ILoggerFactory? loggerFactory = null) : base(optionsFactory, tenantStores, loggerFactory) {
		}

		/// <summary>
		/// Constructs the provider with the given options and 
		/// tenant stores.
		/// </summary>
		/// <param name="options">
		/// The instance of options that are used to create the context
		/// for the tenants.
		/// </param>
		/// <param name="tenantStores">
		/// The list of stores to retrieve the tenants from.
		/// </param>
		/// <param name="loggerFactory">
		/// A factory to create loggers for the repositories.
		/// </param>
		public EntityRepositoryProvider(
			DbContextOptions<TContext> options,
			IEnumerable<IMultiTenantStore<TTenantInfo>> tenantStores,
			ILoggerFactory? loggerFactory = null)
			: base(options, tenantStores, loggerFactory) {

		}

		/// <summary>
		/// Gets a repository for the given tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to get the repository for
		/// </param>
		/// <param name="cancellationToken">
		/// A token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="EntityRepository{TEntity}"/> that
		/// isolates the entities of the given tenant.
		/// </returns>
		/// <exception cref="RepositoryException">
		/// Thrown when the tenant was not found or the repository could not be
		/// constructed with the given tenant.
		/// </exception>
		protected virtual async Task<EntityRepository<TEntity, TKey>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default) {
			return await base.GetRepositoryAsync<EntityRepository<TEntity, TKey>>(tenantId);
		}

		async Task<IRepository<TEntity, TKey>> IRepositoryProvider<TEntity, TKey>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken)
			=> await GetRepositoryAsync(tenantId, cancellationToken);

		/// <summary>
		/// Creates a repository for the given context and tenant.
		/// </summary>
		/// <param name="dbContext">
		/// The context to use to manage the entities.
		/// </param>
		/// <param name="tenantInfo"></param>
		/// <returns></returns>
		protected virtual EntityRepository<TEntity, TKey> CreateRepository(TContext dbContext, ITenantInfo tenantInfo) {
			var logger = CreateLogger();
			return new EntityRepository<TEntity, TKey>(dbContext, tenantInfo, logger);
		}

		internal override object CreateRepositoryInternal(TContext context, TTenantInfo tenant) => CreateRepository(context, tenant);
	}
}
