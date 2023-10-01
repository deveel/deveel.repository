using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
        //public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        //    where TEntity : class
        //    => services.AddEntityRepository<EntityRepository<TEntity>, TEntity>(lifetime);

        //public static IServiceCollection AddEntityRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        //    where TRepository : EntityRepository<TEntity>
        //    where TEntity : class {
        //    services.AddRepository<TRepository>(lifetime);

        //    if (typeof(TRepository) != typeof(EntityRepository<TEntity>))
        //        services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

        //    return services;
        //}


        //public static IServiceCollection AddEntityRepositoryProvider<TContext, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        //    where TContext : DbContext
        //    where TEntity : class
        //    => services.AddRepositoryProvider<EntityRepository<TEntity>>(lifetime);


        //public static IServiceCollection AddEntityRepositoryProvider<TProvider, TContext, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        //    where TContext : DbContext
        //    where TProvider : EntityRepositoryProvider<TEntity, TContext>
        //    where TEntity : class {
        //    services.AddRepositoryProvider<TProvider>(lifetime);
        //    services.Add(new ServiceDescriptor(typeof(EntityRepositoryProvider<TEntity, TContext>), typeof(TProvider), lifetime));

        //    if (typeof(TProvider) != typeof(EntityRepositoryProvider<TEntity, TContext>))
        //        services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

        //    return services;
        //}
    }
}