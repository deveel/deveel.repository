﻿using System.Reflection;

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
			where TRepository : class, IRepository
			=> services.AddRepository(typeof(TRepository), lifetime);

		public static IServiceCollection AddRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TRepository : class, IRepository<TEntity>
			where TEntity : class {
			services.AddRepository(typeof(TRepository), lifetime);
			services.TryAdd(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));

			return services;
		}

		public static IServiceCollection AddRepository(this IServiceCollection services, Type repositoryType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
			if (repositoryType is null) 
				throw new ArgumentNullException(nameof(repositoryType));

			if (!typeof(IRepository).IsAssignableFrom(repositoryType))
				throw new ArgumentException($"The type '{repositoryType}' is not assignable from '{typeof(IRepository)}'",  nameof(repositoryType));

			services.Add(new ServiceDescriptor(typeof(IRepository), repositoryType, lifetime));

			if (repositoryType.GenericTypeArguments.Length > 0) {
				var argType = GetEntityType(repositoryType);

				if (argType == null)
					throw new ArgumentException($"Could not determine the entity type in '{repositoryType}'");
				
				var compareType = typeof(IRepository<>).MakeGenericType(argType);

				if (!compareType.IsAssignableFrom(repositoryType))
					throw new ArgumentException($"The type '{repositoryType}' is not assignable from '{compareType}'", nameof(repositoryType));

				services.TryAdd(new ServiceDescriptor(compareType, repositoryType, lifetime));
			}

			services.Add(new ServiceDescriptor(repositoryType, repositoryType, lifetime));
			
			return services;
		}

		private static Type? GetEntityType(Type serviceType) {
			var entityTypeAttr = serviceType.GetCustomAttribute<EntityTypeAttribute>(true);
			if (entityTypeAttr != null)
				return entityTypeAttr.EntityType;

			var genericTypes = serviceType.GenericTypeArguments;

			var entityTypes = genericTypes.Where(x => Attribute.IsDefined(x, typeof(EntityAttribute))).ToList();
			if (entityTypes.Count > 1)
				throw new RepositoryException($"Ambigous entity type specifications: {serviceType} has multiple 'Entity' types as type argument '{String.Join(", ", entityTypes)}'");
			if (entityTypes.Count == 1)
				return entityTypes[0];

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
            where TProvider : class, IRepositoryProvider
			=> services.AddRepositoryProvider(typeof(TProvider), lifetime);

		public static IServiceCollection AddRepositoryProvider<TProvider, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProvider : class, IRepositoryProvider<TEntity>
			where TEntity : class {
			services.AddRepositoryProvider(typeof(TProvider), lifetime);
			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

			return services;
		}

		public static IServiceCollection AddRepositoryProvider(this IServiceCollection services, Type providerType, ServiceLifetime lifetime = ServiceLifetime.Singleton) {
			if (providerType is null)
				throw new ArgumentNullException(nameof(providerType));

			if (!typeof(IRepositoryProvider).IsAssignableFrom(providerType))
				throw new ArgumentException($"The type '{providerType}' is not assignable from '{typeof(IRepositoryProvider)}'", nameof(providerType));

			services.TryAddEnumerable(new ServiceDescriptor(typeof(IRepositoryProvider), providerType, lifetime));

			if (providerType.GenericTypeArguments.Length > 0) {
				var argType = GetEntityType(providerType);

				if (argType == null)
					throw new RepositoryException($"Unable to determine the entity of the provider '{providerType}'");

				var compareType = typeof(IRepositoryProvider<>).MakeGenericType(argType);

				if (!compareType.IsAssignableFrom(providerType))
					throw new ArgumentException($"The type '{providerType}' is not assignable from '{compareType}'", nameof(providerType));

				services.TryAdd(new ServiceDescriptor(compareType, providerType, lifetime));
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