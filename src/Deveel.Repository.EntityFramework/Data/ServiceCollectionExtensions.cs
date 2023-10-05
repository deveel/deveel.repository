using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddEntityRepository(this IServiceCollection services, Type entityType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
			var repositoryType = typeof(EntityRepository<>).MakeGenericType(entityType);
			return services.AddRepository(repositoryType, lifetime);
		}

		public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TEntity : class
			=> services.AddEntityRepository(typeof(TEntity), lifetime);
    }
}