using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class
            => services.AddEntityRepository<EntityRepository<TEntity>, TEntity>(lifetime);

        public static IServiceCollection AddEntityRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TRepository : EntityRepository<TEntity>
            where TEntity : class {
            services.AddRepository<TRepository, TEntity>(lifetime);
            services.Add(new ServiceDescriptor(typeof(EntityRepository<TEntity>), typeof(TRepository), lifetime));

            if (typeof(TRepository) != typeof(EntityRepository<TEntity>))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return services;
        }

        public static IServiceCollection AddEntityFacadeRepository<TRepository, TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TEntity : class, TFacade
            where TFacade : class
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
            where TEntity : class, TFacade
            where TFacade : class
            => services.AddEntityFacadeRepository<EntityRepository<TEntity, TFacade>, TEntity, TFacade>(lifetime);


        public static IServiceCollection AddEntityRepositoryProvider<TContext, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            where TEntity : class
            => services.AddEntityRepository<EntityRepository<TEntity>, TEntity>(lifetime);


        public static IServiceCollection AddEntityRepositoryProvider<TProvider, TContext, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            where TProvider : EntityRepositoryProvider<TEntity, TContext>
            where TEntity : class {
            services.AddRepositoryProvider<TProvider, TEntity>(lifetime);
            services.Add(new ServiceDescriptor(typeof(EntityRepositoryProvider<TEntity, TContext>), typeof(TProvider), lifetime));

            if (typeof(TProvider) != typeof(EntityRepositoryProvider<TEntity, TContext>))
                services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return services;
        }

        public static IServiceCollection AddEntityFacadeRepositoryProvider<TContext, TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
            where TEntity : class, TFacade
            where TFacade : class
            => services.AddEntityFacadeRepositoryProvider<EntityRepositoryProvider<TEntity, TFacade, TContext>, TContext, TEntity, TFacade>(lifetime);


        public static IServiceCollection AddEntityFacadeRepositoryProvider<TProvider, TContext, TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
              where TContext : DbContext
              where TProvider : EntityRepositoryProvider<TEntity, TFacade, TContext>
              where TEntity : class, TFacade
              where TFacade : class {
            services.AddRepositoryProvider<TProvider, TEntity>(lifetime);
            services.AddRepositoryProvider<TProvider, TFacade>(lifetime);

            services.Add(new ServiceDescriptor(typeof(EntityRepositoryProvider<TEntity, TFacade, TContext>), typeof(TProvider), lifetime));
            services.Add(new ServiceDescriptor(typeof(EntityRepositoryProvider<TEntity, TContext>), typeof(TProvider), lifetime));

            if (typeof(TProvider) != typeof(EntityRepositoryProvider<TEntity, TFacade, TContext>))
                services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return services;
        }

    }
}