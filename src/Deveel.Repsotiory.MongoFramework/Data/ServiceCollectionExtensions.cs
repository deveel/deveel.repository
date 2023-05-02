using Microsoft.Extensions.DependencyInjection;

using MongoFramework;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
        #region AddMongoContext

        public static MongoDbContextBuilder<TContext> AddMongoContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			            where TContext : class, IMongoDbContext {
            return new MongoDbContextBuilder<TContext>(services, lifetime);
        }

		public static MongoDbContextBuilder<MongoDbContext> AddMongoContext(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton) {
            return services.AddMongoContext<MongoDbContext>(lifetime);
        }

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services, Action<MongoDbContextBuilder<TContext>> configure)
			where TContext : class, IMongoDbContext {
			var builder = services.AddMongoContext<TContext>();
			configure?.Invoke(builder);

			return services;
		}

		public static IServiceCollection AddMongoContext(this IServiceCollection services, Action<MongoDbContextBuilder<MongoDbContext>> configure)
			=> services.AddMongoContext<MongoDbContext>(configure);


        #endregion

        #region AddMongoTenantContext

        public static MongoDbContextBuilder<TContext> AddMongoTenantContext<TContext>(this IServiceCollection services)
			where TContext : class, IMongoDbTenantContext
			=> services.AddMongoContext<TContext>(ServiceLifetime.Scoped);

		public static MongoDbContextBuilder<MongoDbTenantContext> AddMongoTenantContext(this IServiceCollection services)
			=> services.AddMongoTenantContext<MongoDbTenantContext>();

		public static IServiceCollection AddMongoTenantContext<TContext>(this IServiceCollection services, Action<MongoDbContextBuilder<TContext>> configure)
            where TContext : class, IMongoDbTenantContext
			=> services.AddMongoContext(configure);

		public static IServiceCollection AddMongoTenantContext(this IServiceCollection services, Action<MongoDbContextBuilder<MongoDbTenantContext>> configure)
			=> services.AddMongoTenantContext<MongoDbTenantContext>(configure);

        #endregion
    }
}
