﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
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
				var argType = GetConceteType(repositoryType.GenericTypeArguments);

				if (argType == null)
					throw new ArgumentException($"Could not determine the entity type in '{repositoryType}'");
				
				// TODO: should we set any constraints here?
				//if (!typeof(IDataEntity).IsAssignableFrom(argType))
				//	throw new ArgumentException($"The argument type '{argType}' of the provided repository is not an entity", nameof(repositoryType));

				var compareType = typeof(IRepository<>).MakeGenericType(argType);

				if (!compareType.IsAssignableFrom(repositoryType))
					throw new ArgumentException($"The type '{repositoryType}' is not assignable from '{compareType}'", nameof(repositoryType));

				services.TryAdd(new ServiceDescriptor(compareType, repositoryType, lifetime));
			}

			services.Add(new ServiceDescriptor(repositoryType, repositoryType, lifetime));
			
			return services;
		}

		private static Type? GetConceteType(Type[] genericTypes) {
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

			services.Add(new ServiceDescriptor(typeof(IRepositoryProvider), providerType, lifetime));

			if (providerType.GenericTypeArguments.Length > 0) {
				var argType = providerType.GenericTypeArguments[0];
				// TODO: should we set any constraints here?
				//if (!typeof(IDataEntity).IsAssignableFrom(argType))
				//	throw new ArgumentException($"The argument type '{argType}' of the provided repository is not an entity", nameof(providerType));

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
    }
}