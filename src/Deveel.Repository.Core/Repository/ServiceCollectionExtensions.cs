using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Deveel.Data;
using Deveel.States;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Repository {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddStore<TStore>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TStore : class, IRepository {
			services.TryAdd(new ServiceDescriptor(typeof(IRepository), typeof(TStore), lifetime));
			services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));

			return services;
		}

		public static IServiceCollection AddStore<TStore, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TStore : class, IRepository<TEntity>
			where TEntity : class, IEntity {
			services.TryAdd(new ServiceDescriptor(typeof(IRepository), typeof(TStore), lifetime));
			services.TryAdd(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TStore), lifetime));
			services.Add(new ServiceDescriptor(typeof(TStore), typeof(TStore), lifetime));

			return services;
		}

		public static IServiceCollection AddStoreFacade<TEntity, TInterface>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TEntity : class, IEntity, TInterface
			where TInterface : class, IEntity {
			services.TryAdd(new ServiceDescriptor(typeof(IRepository<TInterface>), typeof(FacadeRepository<TEntity, TInterface>), lifetime));

			return services;
		}

		public static IServiceCollection AddStoreProvider<TProvider>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProvider : class, IRepositoryProvider {

			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
			services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

			return services;
		}

		public static IServiceCollection AddStoreProvider<TProvider, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProvider : class, IRepositoryProvider<TEntity>
			where TEntity : class, IEntity {
			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

			services.Add(new ServiceDescriptor(typeof(TProvider), typeof(TProvider), lifetime));

			return services;
		}

		public static IServiceCollection AddStoreProviderFacade<TEntity, TInterface>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TEntity : class, TInterface
			where TInterface : class, IEntity {
			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider), typeof(FacadeRepositoryProvider<TEntity, TInterface>), lifetime));
			services.TryAdd(new ServiceDescriptor(typeof(IRepositoryProvider<TInterface>), typeof(FacadeRepositoryProvider<TEntity, TInterface>), lifetime));

			return services;
		}
	}
}
