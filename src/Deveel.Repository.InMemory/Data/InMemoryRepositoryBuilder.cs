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
            services.TryAdd(new ServiceDescriptor(typeof(IRepository), typeof(InMemoryRepository<TEntity>), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(InMemoryRepository<TEntity>), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(InMemoryRepository<TEntity>), typeof(InMemoryRepository<TEntity>), lifetime));
        }

        public InMemoryRepositoryBuilder<TEntity> Use<TRepository>()
            where TRepository : InMemoryRepository<TEntity> {
            services.AddRepository<TRepository>(lifetime);
            services.Add(new ServiceDescriptor(typeof(IRepository), typeof(TRepository), lifetime));
            services.Add(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));

            return this;
        }

        public InMemoryRepositoryBuilder<TEntity> UseProvider<TProvider>()
            where TProvider : InMemoryRepositoryProvider<TEntity> {
            services.AddRepositoryProvider<TProvider, TEntity>(lifetime);

            services.Add(ServiceDescriptor.Describe(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
            services.Add(ServiceDescriptor.Describe(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

            return this;
        }

        private InMemoryRepositoryBuilder<TEntity> WithFacade<TFacade>(Type facadeRepositoryType) 
            where TFacade : class {
            var repositoryType = typeof(InMemoryRepository<,>).MakeGenericType(typeof(TEntity), typeof(TFacade));

            if (!repositoryType.IsAssignableFrom(facadeRepositoryType))
                throw new ArgumentException($"The type '{facadeRepositoryType}' is not a valid facade repository");

            services.AddRepository(repositoryType, lifetime);

            services.Add(ServiceDescriptor.Describe(typeof(IRepository<TFacade>), facadeRepositoryType, lifetime));

            if (repositoryType != facadeRepositoryType)
                services.Add(ServiceDescriptor.Describe(repositoryType, repositoryType, lifetime));

            return this;
        }

        public InMemoryRepositoryBuilder<TEntity> WithFacade<TFacade, TRepository>()
            where TFacade : class {
            return WithFacade<TFacade>(typeof(TRepository));
        }

        public InMemoryRepositoryBuilder<TEntity> WithFacade<TFacade>()
            where TFacade : class
            => WithFacade<TFacade>(typeof(InMemoryRepository<,>).MakeGenericType(typeof(TEntity), typeof(TFacade)));
    }
}
