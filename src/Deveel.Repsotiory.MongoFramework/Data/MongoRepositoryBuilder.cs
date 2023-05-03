using Finbuckle.MultiTenant;

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

        public MongoRepositoryBuilder<TContext, TEntity> OfType<TRepository>()
            where TRepository : MongoRepository<TContext, TEntity> {

            services.RemoveAll<IRepository<TEntity>>();
            services.RemoveAll<MongoRepository<TContext, TEntity>>();

            services.AddRepository<TRepository, TEntity>(lifetime);

            services.Add(new ServiceDescriptor(typeof(MongoRepository<TContext, TEntity>), typeof(TRepository), lifetime));


            if (typeof(TRepository) != typeof(MongoRepository<TContext, TEntity>))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return this;
        }

		#region Provider

		public MongoRepositoryBuilder<TContext, TEntity> WithProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProvider : MongoRepositoryProvider<TContext, TEntity> {
			services.AddRepositoryProvider<TProvider, TEntity>(lifetime);

			if (typeof(TProvider) != typeof(MongoRepositoryProvider<TContext, TEntity>))
				services.TryAdd(new ServiceDescriptor(typeof(MongoRepositoryProvider<TContext, TEntity>), typeof(TProvider), lifetime));

			return this;
		}

		public MongoRepositoryBuilder<TContext, TEntity> WithProvider(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			=> WithProvider<MongoRepositoryProvider<TContext, TEntity>>(lifetime);


		public MongoRepositoryBuilder<TContext, TEntity> WithFacadeProvider<TFacade, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TFacade : class {

			if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
				throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

			var facadeProviderType = typeof(MongoRepositoryProvider<,,>)
				.MakeGenericType(typeof(TContext), typeof(TEntity), typeof(TFacade));

			if (!facadeProviderType.IsAssignableFrom(typeof(TProvider)))
				throw new ArgumentException($"The provider of type '{typeof(TProvider)}' is not assignable from '{facadeProviderType}'");

			services.AddRepositoryProvider(facadeProviderType, lifetime);
			services.Add(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), facadeProviderType, lifetime));

			if (typeof(TProvider) != typeof(MongoRepositoryProvider<TContext, TEntity>))
				services.Add(new ServiceDescriptor(typeof(MongoRepositoryProvider<TContext, TEntity>), typeof(TProvider), lifetime));

			if (typeof(TProvider) != facadeProviderType)
				services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

			return this;
		}

		public MongoRepositoryBuilder<TContext, TEntity> WithFacadeProvider<TFacade>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TFacade : class
			=> WithFacadeProvider<TFacade, MongoRepositoryProvider<TContext, TEntity>>(lifetime);

		#endregion

			#region Tenant-Provider

		public MongoRepositoryBuilder<TContext, TEntity> WithTenantProvider<TTenantInfo, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTenantInfo : class, ITenantInfo, new()
            where TProvider : MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo> {
            services.AddRepositoryProvider<TProvider, TEntity>(lifetime);

            if (typeof(TProvider) != typeof(MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo>))
                services.TryAdd(new ServiceDescriptor(typeof(MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo>), typeof(TProvider), lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithTenantProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : MongoTenantRepositoryProvider<TContext, TEntity, TenantInfo>
            => WithTenantProvider<TenantInfo, TProvider>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultTenantProvider<TTenantInfo>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTenantInfo : class, ITenantInfo, new()
            => WithTenantProvider<TTenantInfo, MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo>>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultTenantProvider(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            => WithDefaultTenantProvider<TenantInfo>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithTenantFacadeProvider<TFacade, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFacade : class
            => WithTenantFacadeProvider<TenantInfo, TFacade, TProvider>(lifetime);

        public MongoRepositoryBuilder<TContext, TEntity> WithTenantFacadeProvider<TTenantInfo, TFacade, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFacade : class
            where TTenantInfo : class, ITenantInfo, new() {

            if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
                throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

            var facadeProviderType1 = typeof(MongoTenantRepositoryProvider<,,>)
                .MakeGenericType(typeof(TContext), typeof(TEntity), typeof(TTenantInfo));

            var facadeProviderType2 = typeof(MongoTenantRepositoryProvider<,,,>)
                .MakeGenericType(typeof(TContext), typeof(TEntity), typeof(TFacade), typeof(TTenantInfo));

            if (!facadeProviderType2.IsAssignableFrom(typeof(TProvider)))
                throw new ArgumentException($"The provider of type '{typeof(TProvider)}' is not assignable from '{facadeProviderType2}'");

            services.AddRepositoryProvider(facadeProviderType2, lifetime);
            services.Add(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), facadeProviderType2, lifetime));

            if (typeof(TProvider) != typeof(MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo>))
                services.Add(new ServiceDescriptor(typeof(MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo>), typeof(TProvider), lifetime));

            if (typeof(TProvider) != facadeProviderType2)
                services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultFacadeTenantProvider<TTenantInfo, TFacade>(ServiceLifetime lifetime = ServiceLifetime.Singleton) 
            where TFacade : class
            where TTenantInfo : class, ITenantInfo, new() {
            if (!typeof(TFacade).IsAssignableFrom(typeof(TEntity)))
                throw new ArgumentException($"The entity of type '{typeof(TEntity)}' is not assignable from '{typeof(TFacade)}'");

            var facadeProviderType = typeof(MongoTenantRepositoryProvider<,,,>)
                .MakeGenericType(typeof(TContext), typeof(TEntity), typeof(TFacade), typeof(TTenantInfo));

            services.AddRepositoryProvider(facadeProviderType, lifetime);
            services.Add(new ServiceDescriptor(facadeProviderType, facadeProviderType, lifetime));
            services.Add(new ServiceDescriptor(typeof(IRepositoryProvider<TFacade>), facadeProviderType, lifetime));

            return this;
        }

        public MongoRepositoryBuilder<TContext, TEntity> WithDefaultFacadeTenantProvider<TFacade>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TFacade : class
            => WithDefaultFacadeTenantProvider<TenantInfo, TFacade>(lifetime);

		#endregion

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
