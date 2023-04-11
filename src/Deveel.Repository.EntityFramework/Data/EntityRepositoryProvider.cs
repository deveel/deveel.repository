using System;
using System.Reflection;

using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Deveel.Data {
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

        ~EntityRepositoryProvider() {
            Dispose(disposing: false);
        }

        public virtual EntityRepository<TEntity> GetRepository(string tenantId) {
            foreach (var store in tenantStores) {
                var tenant = store.TryGetAsync(tenantId)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (tenant != null)
                    return CreateRepository(tenant);
            }

            throw new RepositoryException($"The tenant '{tenantId}' was not found");
        }

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

        IRepository<TEntity> IRepositoryProvider<TEntity>.GetRepository(string tenantId)
            => GetRepository(tenantId);

        IRepository IRepositoryProvider.GetRepository(string tenantId)
            => GetRepository(tenantId);

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
