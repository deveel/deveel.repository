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
	/// <typeparam name="TContext">
	/// The type of <see cref="DbContext"/> used to manage the entities
	/// </typeparam>
    public class EntityRepositoryProvider<TEntity, TContext> : IRepositoryProvider<TEntity>, IDisposable 
        where TContext : DbContext
        where TEntity : class {
        private readonly DbContextOptions<TContext> options;
        private readonly IEnumerable<IMultiTenantStore<TenantInfo>> tenantStores;
        private readonly ILoggerFactory loggerFactory;

        private IDictionary<string, TContext>? contexts;
        private IDictionary<string, EntityRepository<TEntity>>? repositories;
        private bool disposedValue;

        public EntityRepositoryProvider(DbContextOptions<TContext> options, IEnumerable<IMultiTenantStore<TenantInfo>> tenantStores, ILoggerFactory? loggerFactory = null) {
            this.options = options;
            this.tenantStores = tenantStores;
            this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

		/// <summary>
		/// Destroys the instance of the provider.
		/// </summary>
        ~EntityRepositoryProvider() {
            Dispose(disposing: false);
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
        public virtual async Task<EntityRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default) {
            // TODO: move the cache here ...

			foreach (var store in tenantStores) {
                var tenant = await store.TryGetAsync(tenantId);

                if (tenant == null)
					tenant = await store.TryGetByIdentifierAsync(tenantId);

				if (tenant != null)
					return CreateRepository(tenant);
            }

            throw new RepositoryException($"The tenant '{tenantId}' was not found");
        }

		async Task<IRepository<TEntity>> IRepositoryProvider<TEntity>.GetRepositoryAsync(string tenantId, CancellationToken cancellationToken)
			=> await GetRepositoryAsync(tenantId, cancellationToken);

        private TContext ConstructDbContext(TenantInfo tenantInfo) {
            if (contexts == null || !contexts.TryGetValue(tenantInfo.Id, out var dbContext)) {
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                
				var ctor = typeof(TContext).GetConstructor(bindingFlags, new[] { typeof(ITenantInfo), typeof(DbContextOptions<TContext>) });
                if (ctor == null)
                    throw new RepositoryException($"The DbContext of type '{typeof(TContext)}' has invalid constructor for a repository provider");

                dbContext = (TContext)ctor.Invoke(new object[] { tenantInfo, options });

                if (contexts == null)
                    contexts = new Dictionary<string, TContext>();
            }

            return dbContext;
        }

        private EntityRepository<TEntity> CreateRepository(TenantInfo tenant) {
            try {
                if (repositories == null || !repositories.TryGetValue(tenant.Id, out var repository)) {
                    var dbContext = ConstructDbContext(tenant);

                    repository = CreateRepository(dbContext, tenant);

                    if (repositories == null)
                        repositories = new Dictionary<string, EntityRepository<TEntity>>();

                    repositories.Add(tenant.Id, repository);
                }

                return repository;
            } catch(RepositoryException) {
                throw;
            } catch (Exception ex) {
                throw new RepositoryException("Could not construct the repository", ex);
            }
        }

        protected virtual ILogger CreateLogger() {
            return loggerFactory.CreateLogger<EntityRepository<TEntity>>();
        }

        protected virtual EntityRepository<TEntity> CreateRepository(TContext dbContext, ITenantInfo tenantInfo) {
            var logger = CreateLogger();
            return new EntityRepository<TEntity>(dbContext, tenantInfo, logger);
        }

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
                foreach(var repository in repositories) {
                    repository.Value?.Dispose();
                }
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
	}
}
