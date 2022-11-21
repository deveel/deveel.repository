using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddRepository<TRepository>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : class, IRepository {
            services.TryAdd(new ServiceDescriptor(typeof(IRepository), typeof(TRepository), lifetime));
            services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return services;
        }

        public static IServiceCollection AddRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : class, IRepository<TEntity>
            where TEntity : class, IEntity {
            services.TryAdd(new ServiceDescriptor(typeof(IRepository), typeof(TRepository), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));
            services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return services;
        }

        public static IServiceCollection AddRepositoryFacade<TEntity, TInterface>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class, IEntity, TInterface
            where TInterface : class, IEntity {
            services.TryAdd(new ServiceDescriptor(typeof(IRepository<TInterface>), typeof(FacadeRepository<TEntity, TInterface>), lifetime));

            return services;
        }

        public static IServiceCollection AddRepositoryProvider<TProvider>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : class, IRepositoryProvider {

            services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
            services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return services;
        }

        public static IServiceCollection AddRepositoryProvider<TProvider, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : class, IRepositoryProvider<TEntity>
            where TEntity : class, IEntity {
            services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

            services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return services;
        }

        public static IServiceCollection AddRepositoryProviderFacade<TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TEntity : class, TFacade
            where TFacade : class, IEntity {
            services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(FacadeRepositoryProvider<TEntity, TFacade>), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), typeof(FacadeRepositoryProvider<TEntity, TFacade>), lifetime));

            return services;
        }

        public static IServiceCollection AddRepositoryController<TController>(this IServiceCollection services, Action<RepositoryControllerOptions>? configure = null)
            where TController : class, IRepositoryController {

            var options = services.AddOptions<RepositoryControllerOptions>();

            if (configure != null)
                options.Configure(configure);

            services.AddSingleton<IRepositoryController, TController>();
            services.AddSingleton<TController>();

            return services;
        }

        public static IServiceCollection AddRepositoryController(this IServiceCollection services, Action<RepositoryControllerOptions>? configure = null)
            => services.AddRepositoryController<DefaultRepositoryController>(configure);
    }
}