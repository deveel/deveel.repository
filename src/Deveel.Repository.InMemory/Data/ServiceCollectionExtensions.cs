using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
		public static InMemoryRepositoryBuilder<TEntity> AddInMemoryRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TEntity : class
            => new InMemoryRepositoryBuilder<TEntity>(services, lifetime);

		public static IServiceCollection AddInMemoryRepository<TEntity>(this IServiceCollection services, Action<InMemoryRepositoryBuilder<TEntity>> configure)
			where TEntity : class {
			var builder = services.AddInMemoryRepository<TEntity>();
			configure?.Invoke(builder);

			return services;
		}
	}
}
