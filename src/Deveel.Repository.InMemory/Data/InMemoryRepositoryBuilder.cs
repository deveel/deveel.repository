using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
    public sealed class InMemoryRepositoryBuilder<TEntity> where TEntity : class {
        private readonly IServiceCollection services;
        private readonly ServiceLifetime lifetime;

        internal InMemoryRepositoryBuilder(IServiceCollection services, ServiceLifetime lifetime) {
            this.services = services;
            this.lifetime = lifetime;

            RegisterRepository();
        }

        private void RegisterRepository() {
			services.AddRepository<InMemoryRepository<TEntity>>(lifetime);
        }

        public InMemoryRepositoryBuilder<TEntity> Use<TRepository>()
            where TRepository : InMemoryRepository<TEntity> {
            services.AddRepository<TRepository>(lifetime);
            services.Add(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));

            return this;
        }

        public InMemoryRepositoryBuilder<TEntity> UseProvider<TProvider>()
            where TProvider : InMemoryRepositoryProvider<TEntity> {
            services.AddRepositoryProvider<TProvider>(lifetime);

            services.Add(ServiceDescriptor.Describe(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

            return this;
        }
    }
}
