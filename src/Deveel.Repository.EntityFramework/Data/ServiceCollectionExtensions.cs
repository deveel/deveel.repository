using System;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class, IDataEntity
            => services.AddEntityRepository<EntityRepository<TEntity>, TEntity>(lifetime);

        public static IServiceCollection AddEntityRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : EntityRepository<TEntity>
            where TEntity : class, IDataEntity {
            services.AddRepository<TRepository, TEntity>(lifetime);
            services.Add(new ServiceDescriptor(typeof(EntityRepository<TEntity>), typeof(TRepository), lifetime));

            if (typeof(TRepository) != typeof(EntityRepository<TEntity>))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return services;
        }

        public static IServiceCollection AddEntityFacadeRepository<TRepository, TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class, TFacade, IDataEntity
            where TFacade : class, IDataEntity
            where TRepository : EntityRepository<TEntity, TFacade> {
            services
                .AddRepository<TRepository, TEntity>(lifetime)
                .AddRepository<TRepository, TFacade>(lifetime);

            services.Add(new ServiceDescriptor(typeof(EntityRepository<TEntity, TFacade>), typeof(TRepository), lifetime));
            services.Add(new ServiceDescriptor(typeof(EntityRepository<TEntity>), typeof(TRepository), lifetime));

            if (typeof(EntityRepository<TEntity, TFacade>) != typeof(TRepository))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return services;
        }

        public static IServiceCollection AddEntityFacadeRepository<TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class, TFacade, IDataEntity
            where TFacade : class, IDataEntity
            => services.AddEntityFacadeRepository<EntityRepository<TEntity, TFacade>, TEntity, TFacade>(lifetime);
    }
}
