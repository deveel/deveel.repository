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
			services.AddRepository<MongoRepository<TContext, TEntity>>(lifetime);
        }

        public MongoRepositoryBuilder<TContext, TEntity> OfType<TRepository>()
            where TRepository : MongoRepository<TContext, TEntity> {

            services.RemoveAll<IRepository<TEntity>>();
			services.RemoveAll<IQueryableRepository<TEntity>>();
			services.RemoveAll<IFilterableRepository<TEntity>>();
			services.RemoveAll<IPageableRepository<TEntity>>();
            services.RemoveAll<MongoRepository<TContext, TEntity>>();

            services.AddRepository<TRepository>(lifetime);

			services.Add(new ServiceDescriptor(typeof(MongoRepository<TContext, TEntity>), typeof(TRepository), lifetime));

            if (typeof(TRepository) != typeof(MongoRepository<TContext, TEntity>))
                services.Add(new ServiceDescriptor(typeof(TRepository), typeof(TRepository), lifetime));

            return this;
        }

		#region Provider

		public MongoRepositoryBuilder<TContext, TEntity> WithProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProvider : MongoRepositoryProvider<TContext, TEntity> {
			services.AddRepositoryProvider<TProvider>(lifetime);

			if (typeof(TProvider) != typeof(MongoRepositoryProvider<TContext, TEntity>))
				services.TryAdd(new ServiceDescriptor(typeof(MongoRepositoryProvider<TContext, TEntity>), typeof(TProvider), lifetime));

			return this;
		}

		public MongoRepositoryBuilder<TContext, TEntity> WithProvider(ServiceLifetime lifetime = ServiceLifetime.Singleton)
			=> WithProvider<MongoRepositoryProvider<TContext, TEntity>>(lifetime);


		#endregion

		#region Tenant-Provider

		public MongoRepositoryBuilder<TContext, TEntity> WithTenantProvider<TTenantInfo, TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TTenantInfo : class, ITenantInfo, new()
            where TProvider : MongoTenantRepositoryProvider<TContext, TEntity, TTenantInfo> {
            services.AddRepositoryProvider<TProvider>(lifetime);

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

		#endregion
    }
}
