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

using System.Reflection;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
	/// <summary>
	/// A generic base class for the implementation of a repository provider
	/// for the EntityFramework library.
	/// </summary>
	/// <typeparam name="TContext">
	/// The type of the EntityFramework DbContext that is holding
	/// the context for the repository entity.
	/// </typeparam>
	/// <typeparam name="TEntity">
	/// The type of the entity that is managed by the repository.
	/// </typeparam>
	public abstract class EntityRepositoryProviderBase<TContext, TEntity> : IDisposable
		where TContext : DbContext
		where TEntity : class {
		private readonly IRepositoryTenantResolver tenantResolver;
		private readonly IDbContextOptionsFactory<TContext> optionsFactory;
		private readonly ILoggerFactory loggerFactory;

		private IDictionary<string, TContext>? contexts;
		private IDictionary<string, object>? repositories;
		private bool disposedValue;

		internal EntityRepositoryProviderBase(
			IDbContextOptionsFactory<TContext> optionsFactory,
			IRepositoryTenantResolver tenantResolver,
			ILoggerFactory? loggerFactory = null) {
			this.optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));
			this.tenantResolver = tenantResolver;
			this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
		}

		internal EntityRepositoryProviderBase(
			DbContextOptions<TContext> options,
			IRepositoryTenantResolver tenantResolver,
			ILoggerFactory? loggerFactory = null)
			: this(new SingletonDbContextOptionsFactory(options), tenantResolver, loggerFactory) {
		}
		
		/// <summary>
		/// Destroys the provider and all the repositories and contexts.
		/// </summary>
		~EntityRepositoryProviderBase() {
			Dispose(disposing: false);
		}

		internal async Task<TRepository> GetRepositoryAsync<TRepository>(string tenantId, CancellationToken cancellationToken = default) {
			// TODO: move the cache here ...

			var tenant = await tenantResolver.FindTenantAsync(tenantId, cancellationToken);
			if (tenant == null)
				throw new RepositoryException($"The tenant '{tenantId}' was not found");
			if (tenant is not DbTenantInfo dbTenant)
				throw new RepositoryException($"The tenant '{tenantId}' is not a valid DB Tenant");

			return CreateRepository<TRepository>(dbTenant);
		}

		private TContext ConstructDbContext(DbTenantInfo tenantInfo) {
			if (contexts == null || !contexts.TryGetValue(tenantInfo.Id!, out var dbContext)) {
				var options = optionsFactory.Create(tenantInfo);
				var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

				TContext? context = null;

				foreach (var ctor in typeof(TContext).GetConstructors(bindingFlags)) {
					var parameters = ctor.GetParameters();
					if (parameters.Length == 1 && parameters[0].ParameterType == typeof(DbContextOptions<TContext>)) {
						context = (TContext)ctor.Invoke(new object[] { options });
					} else if (parameters.Length == 2 &&
						typeof(ITenantInfo).IsAssignableFrom(parameters[0].ParameterType) &&
						parameters[1].ParameterType == typeof(DbContextOptions<TContext>)) {
						context = (TContext)ctor.Invoke(new object[] { tenantInfo, options });
					}
				}

				if (context == null)
					throw new RepositoryException($"Could not construct the context for tenant '{tenantInfo.Id}'");

				if (contexts == null)
					contexts = new Dictionary<string, TContext>();

				contexts[tenantInfo.Id!] = dbContext = context;
			}

			return dbContext;
		}

		private TRepository CreateRepository<TRepository>(DbTenantInfo tenant) {
			try {
				if (repositories == null || !repositories.TryGetValue(tenant.Id!, out var repository)) {
					var dbContext = ConstructDbContext(tenant);

					repository = CreateRepositoryInternal(dbContext, tenant);

					if (repositories == null)
						repositories = new Dictionary<string, object>();

					repositories.Add(tenant.Id!, repository);
				}

				return (TRepository) repository;
			} catch (RepositoryException) {
				throw;
			} catch (Exception ex) {
				throw new RepositoryException("Could not construct the repository", ex);
			}
		}

		internal abstract object CreateRepositoryInternal(TContext context, ITenantInfo tenant);

		/// <summary>
		/// Creates a logger for a repository.
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="ILogger"/> that can be used
		/// to log messages from the repository.
		/// </returns>
		protected virtual ILogger CreateLogger() {
			return loggerFactory.CreateLogger<EntityRepository<TEntity>>();
		}

		/// <summary>
		/// Dispose the provider and all the repositories and contexts
		/// </summary>
		/// <param name="disposing">
		/// Indicates if the provider is disposing.
		/// </param>
		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					DisposeRepositories();
					DisposeContexts();
				}

				repositories?.Clear();
				contexts?.Clear();

				repositories = null;
				contexts = null;
				disposedValue = true;
			}
		}

		private void DisposeContexts() {
			if (contexts != null) {
				foreach (var context in contexts) {
					context.Value?.Dispose();
				}
			}
		}

		private void DisposeRepositories() {
			if (repositories != null) {
				foreach (var repository in repositories) {
					(repository.Value as IDisposable)?.Dispose();
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private class SingletonDbContextOptionsFactory : IDbContextOptionsFactory<TContext> {
			private readonly DbContextOptions<TContext> options;

			public SingletonDbContextOptionsFactory(DbContextOptions<TContext> options) {
				this.options = options;
			}

			public DbContextOptions<TContext> Create(DbTenantInfo tenantInfo) => options;
		}
	}
}
