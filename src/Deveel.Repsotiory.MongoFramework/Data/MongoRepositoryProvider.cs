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
	/// <typeparam name="TTenantInfo">
	/// The type of the tenant information to be used to create the context
	/// </typeparam>
    public class MongoRepositoryProvider<TContext, TEntity, TTenantInfo> : IRepositoryProvider<TEntity>, IDisposable 
		where TContext : class, IMongoDbContext
		where TTenantInfo : class, ITenantInfo, new()
		where TEntity : class {
		private readonly IEnumerable<IMultiTenantStore<TTenantInfo>>? stores;
		private bool disposedValue;

		private IDictionary<string, MongoRepository<TEntity>>? repositories;

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
			ILoggerFactory? loggerFactory = null) {
			this.stores = stores;
			LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		/// <summary>
		/// Gets the factory to create the logger to be used by the repository.
		/// </summary>
		protected ILoggerFactory LoggerFactory { get; }

		/// <summary>
		/// Attempts to resolve the tenant information for a given tenant ID.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant to be resolved (that can
		/// be the ID or the identifier of the tenant).
		/// </param>
		/// <param name="cancellationToken">
		/// A token to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TTenantInfo"/> that
		/// is identified by the given <paramref name="tenantId"/>, or
		/// <c>null</c> if no tenant information could be resolved.
		/// </returns>
		protected virtual async Task<TTenantInfo?> GetTenantInfoAsync(string tenantId, CancellationToken cancellationToken) {
			if (stores == null)
				return null;

			foreach(var store in stores) {
				// TODO: making the IRepositoryProvider to be async
				var tenantInfo = await store.TryGetAsync(tenantId);

				if (tenantInfo == null)
					tenantInfo = await store.TryGetByIdentifierAsync(tenantId);

				if (tenantInfo != null) {
					return tenantInfo;
				}
			}

			return null;
		}

		/// <summary>
		/// Creates a logger to be used by the repository.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="ILogger"/> that is used
		/// to log messages from the repository.
		/// </returns>
		protected virtual ILogger CreateLogger() {
			return LoggerFactory.CreateLogger(typeof(MongoRepository<TEntity>));
		}

		/// <summary>
		/// Creates an instance of <typeparamref name="TContext"/> that
		/// can be used to create the repository for a given tenant.
		/// </summary>
		/// <param name="connection">
		/// The connection to the MongoDB database.
		/// </param>
		/// <param name="tenantInfo">
		/// The information about the tenant to be used to create the context.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TContext"/> that
		/// can be used to create the repository for a given tenant.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown when either the <paramref name="connection"/> or the
		/// <paramref name="tenantInfo"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown when the <typeparamref name="TContext"/> has no
		/// suitable constructor to be created.
		/// </exception>
		protected virtual TContext? CreateContext(IMongoDbConnection connection, TTenantInfo tenantInfo) {
			ArgumentNullException.ThrowIfNull(connection, nameof(connection));
			ArgumentNullException.ThrowIfNull(tenantInfo, nameof(tenantInfo));

			if (typeof(TContext) == typeof(MongoDbTenantContext))
				return (new MongoDbTenantContext(connection, tenantInfo.Id)) as TContext;
			if (typeof(TContext) == typeof(MongoDbContext))
				return new MongoDbContext(connection) as TContext;

			var ctor1 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(IMultiTenantContext<TTenantInfo>) });
			if (ctor1 != null)
				return Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), CreateTenantContext(tenantInfo) }) as TContext;

			var ctor2 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(string) });
			if (ctor2 != null)
				return Activator.CreateInstance(typeof(TContext), new object[] { connection.ForContext<TContext>(), tenantInfo.Id! }) as TContext;

			throw new NotSupportedException($"Cannot create '{typeof(TContext)}' MongoDB Context");
        }

		/// <summary>
		/// Creates an instance of <see cref="IMultiTenantContext{TTenantInfo}"/>
		/// for a given tenant.
		/// </summary>
		/// <param name="tenantInfo"></param>
		/// <returns>
		/// Returns an instance of <see cref="IMultiTenantContext{TTenantInfo}"/>
		/// that can be used to create the repository for a given tenant.
		/// </returns>
		protected virtual IMultiTenantContext<TTenantInfo> CreateTenantContext(TTenantInfo tenantInfo) {
			return new MultiTenantContext<TTenantInfo> {
				TenantInfo = tenantInfo
			};
		}

		async Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken) 
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
		public async Task<MongoRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default) {
			try {
				if (repositories == null)
					repositories = new Dictionary<string, MongoRepository<TEntity>>();

				if (!repositories.TryGetValue(tenantId, out var repository)) {
					var tenantInfo = await GetTenantInfoAsync(tenantId, cancellationToken);

					if (tenantInfo == null)
						throw new RepositoryException($"Unable to find any tenant for the ID '{tenantId}' - cannot construct a context");

					var connection = MongoDbConnection.FromConnectionString(tenantInfo.ConnectionString);

					// var tenantContext = CreateTenantContext(tenantInfo);
					var context = CreateContext(connection, tenantInfo);

					if (context == null)
						throw new RepositoryException($"Unable to create the Mongo DB Context");

					repository = CreateRepository(context);

					repositories[tenantId] = repository;
				}

				return repository;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException($"Unabe to create the repository for tenant '{tenantId}'", ex);
			}
		}

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
		protected virtual MongoRepository<TEntity> CreateRepository(TContext context, ILogger logger) {
			return new MongoRepository<TEntity>(context, logger);
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
        protected virtual MongoRepository<TEntity> CreateRepository(TContext context) {
            return CreateRepository(context, CreateLogger());
        }

		/// <summary>
		/// Disposes the provider and all the repositories created by it.
		/// </summary>
		/// <param name="disposing">
		/// A flag indicating if the provider is disposing.
		/// </param>
        protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					DisposeRepositories();
				}

				repositories = null;
				disposedValue = true;
			}
		}

		private void DisposeRepositories() {
			if (repositories != null) {
				foreach (var repository in repositories.Values) { 
					if (repository is IDisposable disposable)
						disposable.Dispose();
				}

				repositories.Clear();
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
