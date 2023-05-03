﻿using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
    public sealed class MongoRepositoryBuilder<TContext, TEntity> 
        where TContext : class, IMongoDbContext
        where TEntity : class {
        private readonly IServiceCollection services;
        private readonly ServiceLifetime lifetime;

        internal MongoRepositoryBuilder(IServiceCollection services, ServiceLifetime lifetime) {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
            this.lifetime = lifetime;
            TryAddDefault();
        }

        private void TryAddDefault() {
            services.TryAdd(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(MongoRepository<TContext, TEntity>), lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(MongoRepository<TContext, TEntity>), typeof(MongoRepository<TContext, TEntity>), lifetime));
        }

        public MongoRepositoryBuilder<TContext, TEntity> Use<TRepository>()
            where TRepository : MongoRepository<TContext, TEntity> {

            services.RemoveAll<IRepository<TEntity>>();
            services.AddRepository<TRepository, TEntity>(lifetime);

            services.TryAdd(new ServiceDescriptor(typeof(MongoRepository<TContext, TEntity>), typeof(TRepository), lifetime));


            if (typeof(TRepository) != typeof(MongoRepository<TContext, TEntity>))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithProvider<TTenantInfo, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTenantInfo : class, ITenantInfo, new()
            where TProvider : MongoRepositoryProvider<TEntity, TTenantInfo> {
            services.AddRepositoryProvider<TProvider, TEntity>(lifetime);

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : MongoRepositoryProvider<TEntity, TenantInfo>
            => WithProvider<TenantInfo, TProvider>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultProvider<TTenantInfo>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTenantInfo : class, ITenantInfo, new()
            => WithProvider<TTenantInfo, MongoRepositoryProvider<TEntity, TTenantInfo>>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultProvider(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            => WithDefaultProvider<TenantInfo>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithFacadeProvider<TTenantInfo, TFacade, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFacade : class
            where TTenantInfo : class, ITenantInfo, new() {

            if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
                throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

            var facadeProviderType = typeof(MongoRepositoryProvider<,,>)
                .MakeGenericType(typeof(TEntity), typeof(TFacade), typeof(TTenantInfo));

            if (!facadeProviderType.IsAssignableFrom(typeof(TProvider)))
                throw new ArgumentException($"The provider of type '{typeof(TProvider)}' is not assignable from '{facadeProviderType}'");

            services.AddRepositoryProvider(facadeProviderType, lifetime);
            services.Add(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), facadeProviderType, lifetime));

            if (typeof(TProvider) != facadeProviderType)
                services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithFacadeProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            => WithFacadeProvider<TenantInfo, TEntity, TProvider>(lifetime);


        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultFacadeProvider<TTenantInfo, TFacade>(ServiceLifetime lifetime = ServiceLifetime.Singleton) 
            where TFacade : class
            where TTenantInfo : class, ITenantInfo, new() {
            if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
                throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

            var facadeProviderType = typeof(MongoRepositoryProvider<,,>)
                .MakeGenericType(typeof(TEntity), typeof(TFacade), typeof(TTenantInfo));

            services.AddRepositoryProvider(facadeProviderType, lifetime);
            services.Add(new ServiceDescriptor(facadeProviderType, facadeProviderType, lifetime));
            services.Add(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), facadeProviderType, lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultFacadeProvider<TFacade>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFacade : class
            => WithDefaultFacadeProvider<TenantInfo, TFacade>(lifetime);

        // TODO: Add support for facades?

        public MongoRepositoryBuilder<TContext, TEntity> WithFacade<TFacade>()
            where TFacade : class {
            if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
                throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

            var facadeType = typeof(MongoRepository<,,>)
                .MakeGenericType(typeof(TContext), typeof(TEntity), typeof(TFacade));

            services.AddRepository(facadeType, lifetime);
            services.Add(new ServiceDescriptor(typeof(IRepository<TFacade>), facadeType, lifetime));

            return this;
        }
    }
}
