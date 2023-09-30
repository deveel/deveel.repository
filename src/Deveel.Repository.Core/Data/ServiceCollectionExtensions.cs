using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
	/// <summary>
	/// Extensions for the <see cref="IServiceCollection"/> to register
	/// repositories and providers.
	/// </summary>
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Registers a repository of the given type in the service collection.
		/// </summary>
		/// <typeparam name="TRepository">
		/// The type of the repository to register.
		/// </typeparam>
		/// <param name="services">
		/// The service collection to register the repository.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the repository in the service collection.
		/// </param>
		/// <returns>
		/// Returns the same <see cref="IServiceCollection"/> to allow chaining.
		/// </returns>
		public static IServiceCollection AddRepository<TRepository>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			=> services.AddRepository(typeof(TRepository), lifetime);

		public static IServiceCollection AddRepository(this IServiceCollection services, Type repositoryType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
			if (repositoryType is null) 
				throw new ArgumentNullException(nameof(repositoryType));

			if (!repositoryType.IsClass || repositoryType.IsAbstract)
				throw new ArgumentException($"The type '{repositoryType}' is not a valid repository type", nameof(repositoryType));

			var baseTypes = repositoryType.GetInterfaces()
				.Where(x => x.IsGenericType && typeof(IRepository<>).IsAssignableFrom(x.GetGenericTypeDefinition()))
				.ToList();

			if (baseTypes.Count == 0)
				throw new ArgumentException($"The type '{repositoryType}' is not a valid repository type", nameof(repositoryType));

			foreach (var baseType in baseTypes) {
				var entityType = GetEntityType(baseType);

				if (entityType == null)
					throw new RepositoryException($"Unable to determine the entity of the repository '{repositoryType}'");

				if (typeof(IRepository<>).MakeGenericType(entityType).IsAssignableFrom(repositoryType))
					services.TryAdd(new ServiceDescriptor(typeof(IRepository<>).MakeGenericType(entityType), repositoryType, lifetime));

				if (typeof(IQueryableRepository<>).MakeGenericType(entityType).IsAssignableFrom(repositoryType))
					services.TryAdd(new ServiceDescriptor(typeof(IQueryableRepository<>).MakeGenericType(entityType), repositoryType, lifetime));
				if (typeof(IFilterableRepository<>).MakeGenericType(entityType).IsAssignableFrom(repositoryType))
					services.TryAdd(new ServiceDescriptor(typeof(IFilterableRepository<>).MakeGenericType(entityType), repositoryType, lifetime));
				if (typeof(IPageableRepository<>).MakeGenericType(entityType).IsAssignableFrom(repositoryType))
					services.TryAdd(new ServiceDescriptor(typeof(IPageableRepository<>).MakeGenericType(entityType), repositoryType, lifetime));
			}

			services.Add(new ServiceDescriptor(repositoryType, repositoryType, lifetime));

			return services;
		}

		private static Type? GetEntityType(Type serviceType) {
			var genericTypes = serviceType.GenericTypeArguments;

			if (genericTypes.Length == 1 && genericTypes[0].IsClass)
				return genericTypes[0];

			var intefaces = genericTypes.Where(x => x.IsInterface);
			var classes = genericTypes.Where(x => x.IsClass);

			var inheritedTypes = classes.Where(x => intefaces.Any(y => y.IsAssignableFrom(x))).ToList();

			if (inheritedTypes.Count == 0)
				return null;

			if (inheritedTypes.Count > 1)
				throw new InvalidOperationException("Ambiguous reference in the definition of the repository");

			return inheritedTypes[0];
		}

        public static IServiceCollection AddRepositoryProvider<TProvider>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProvider : class
			=> services.AddRepositoryProvider(typeof(TProvider), lifetime);

		public static IServiceCollection AddRepositoryProvider(this IServiceCollection services, Type providerType, ServiceLifetime lifetime = ServiceLifetime.Singleton) {
			if (providerType is null)
				throw new ArgumentNullException(nameof(providerType));

			var baseTypes = providerType.GetInterfaces()
				.Where(x => x.IsGenericType && typeof(IRepositoryProvider<>).IsAssignableFrom(x.GetGenericTypeDefinition()))
				.ToList();

			if (baseTypes.Count == 0)
				throw new ArgumentException($"The type '{providerType}' is not a valid repository provider type", nameof(providerType));

			foreach (var baseType in baseTypes) {
				var entityType = GetEntityType(baseType);

				if (entityType == null)
					throw new RepositoryException($"Unable to determine the entity of the repository provider '{providerType}'");

				if (typeof(IRepositoryProvider<>).MakeGenericType(entityType).IsAssignableFrom(providerType))
					services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<>).MakeGenericType(entityType), providerType, lifetime));
			}

			services.Add(new ServiceDescriptor(providerType, providerType, lifetime));

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

		/// <summary>
		/// Registers a singleton <see cref="ISystemTime"/> service of the
		/// given <typeparamref name="TTime"/> type.
		/// </summary>
		/// <typeparam name="TTime">
		/// The type of the <see cref="ISystemTime"/> implementation.
		/// </typeparam>
		/// <param name="services">
		/// The <see cref="IServiceCollection"/> to add the service to.
		/// </param>
		/// <returns>
		/// Returns the <see cref="IServiceCollection"/> so that additional calls can be chained.
		/// </returns>
		public static IServiceCollection AddSystemTime<TTime>(this IServiceCollection services)
			where TTime : class, ISystemTime {
			services.TryAddSingleton<ISystemTime, TTime>();
			services.AddSingleton<TTime>();
			return services;
		}

		/// <summary>
		/// Registers a singleton instance of <see cref="ISystemTime"/> of the
		/// given <typeparamref name="TTime"/> type.
		/// </summary>
		/// <typeparam name="TTime">
		/// The type of the <see cref="ISystemTime"/> implementation.
		/// </typeparam>
		/// <param name="services">
		/// The <see cref="IServiceCollection"/> to add the service to.
		/// </param>
		/// <returns>
		/// Returns the <see cref="IServiceCollection"/> so that additional calls can be chained.
		/// </returns>
		public static IServiceCollection AddSystemTime<TTime>(this IServiceCollection services, TTime time)
			where TTime : class, ISystemTime {
			services.TryAddSingleton<ISystemTime>(time);
			services.AddSingleton(time);
			return services;
		}

		/// <summary>
		/// Registers the default <see cref="ISystemTime"/> service implementation
		/// </summary>
		/// <param name="services">
		/// The <see cref="IServiceCollection"/> to add the service to.
		/// </param>
		/// <returns>
		/// Returns the <see cref="IServiceCollection"/> so that additional calls can be chained.
		/// </returns>
		public static IServiceCollection AddSystemTime(this IServiceCollection services)
			=> services.AddSystemTime<SystemTime>();
    }
}