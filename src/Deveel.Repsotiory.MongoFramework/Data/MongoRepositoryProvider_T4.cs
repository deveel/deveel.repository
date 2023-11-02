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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of <see cref="IRepositoryProvider{TEntity}"/> that
	/// is able to create a <see cref="MongoRepository{TEntity}"/> for a given
	/// entity type and tenant.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the <see cref="IMongoDbContext"/> to be used to create the
	/// instances of <see cref="MongoRepository{TEntity}"/> for a given tenant.
	/// </typeparam>
	/// <typeparam name="TEntity">
	/// The type of the entity to be managed by the repository.
	/// </typeparam>
	/// <typeparam name="TKey">
	/// The type of the key of the entity to be managed by the repository.
	/// </typeparam>
	/// <typeparam name="TTenantInfo">
	/// The type of the tenant information to be used to create the context
	/// </typeparam>
	public class MongoRepositoryProvider<TContext, TEntity, TKey, TTenantInfo> : MongoRepositoryProviderBase<TContext, TEntity, TTenantInfo>,
		IRepositoryProvider<TEntity, TKey>
		where TContext : class, IMongoDbContext
		where TTenantInfo : class, ITenantInfo, new()
		where TEntity : class {

		/// <summary>
		/// Constructs the provider with a given set of stores to be used to
		/// resolve the tenant information and connection string.
		/// </summary>
		/// <param name="stores">
		/// The set of stores to be used to resolve the tenant information
		/// and connection string.
		/// </param>
		/// <param name="loggerFactory">
		/// A factory to create the logger to be used by the repository.
		/// </param>
		public MongoRepositoryProvider(
			IEnumerable<IMultiTenantStore<TTenantInfo>>? stores = null,
			ILoggerFactory? loggerFactory = null) : base(stores, loggerFactory) {
		}

		async Task<IRepository<TEntity, TKey>> IRepositoryProvider<TEntity, TKey>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken)
			=> await GetRepositoryAsync(tenantId, cancellationToken);

		/// <summary>
		/// Gets an instance of <see cref="MongoRepository{TEntity}"/> for
		/// the given tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to be used to create the repository.
		/// </param>
		/// <param name="cancellationToken">
		/// A token to cancel the operation.
		/// </param>
		/// <returns></returns>
		/// <exception cref="RepositoryException"></exception>
		public async Task<MongoRepository<TEntity, TKey>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default) {
			return await GetRepositoryAsync<MongoRepository<TEntity, TKey>>(tenantId, cancellationToken);
		}

		internal override object CreateRepositoryInternal(TContext context) => CreateRepository(context);

		/// <summary>
		/// Creates an instance of <see cref="MongoRepository{TEntity}"/> for
		/// the given context.
		/// </summary>
		/// <param name="context">
		/// The context to be used to create the repository.
		/// </param>
		/// <param name="logger">
		/// A logger to be used by the repository.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="MongoRepository{TEntity}"/> that
		/// can be used to manage the entity of type <typeparamref name="TEntity"/>.
		/// </returns>
		protected virtual MongoRepository<TEntity, TKey> CreateRepository(TContext context, ILogger logger) {
			return new MongoRepository<TEntity, TKey>(context, logger);
		}

		/// <summary>
		/// Creates a repository for a given context.
		/// </summary>
		/// <param name="context">
		/// The context to be used to create the repository.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="MongoRepository{TEntity}"/> that
		/// is used to manage the entity of type <typeparamref name="TEntity"/>.
		/// </returns>
		protected virtual MongoRepository<TEntity, TKey> CreateRepository(TContext context) {
			return CreateRepository(context, CreateLogger());
		}
	}
}
