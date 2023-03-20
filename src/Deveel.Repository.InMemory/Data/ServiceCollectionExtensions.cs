using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
		#region AddInMemoryRepository<T>

		public static IServiceCollection AddInMemoryRepository<TRepository, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TEntity : class, IDataEntity
			where TRepository : InMemoryRepository<TEntity> {
			services.AddRepository<TRepository>(lifetime);
			services.Add(new ServiceDescriptor(typeof(IRepository), typeof(TRepository), lifetime));
			services.Add(new ServiceDescriptor(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));

			return services;
		}

		public static IServiceCollection AddInMemoryRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TEntity : class, IDataEntity
			=> services.AddInMemoryRepository<InMemoryRepository<TEntity>, TEntity>(lifetime);

		#endregion

		#region AddInMemoryFacadeRepository<TEntity,TFacade>

		public static IServiceCollection AddInMemoryFacadeRepository<TRepository, TEntity, TFacade>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			where TRepository : InMemoryRepository<TEntity, TFacade> {
			services
				.AddRepository<TRepository, TEntity>(lifetime)
				.AddRepository<TRepository, TFacade>(lifetime);

			services.Add(ServiceDescriptor.Describe(typeof(IRepository), typeof(TRepository), lifetime));
			services.Add(ServiceDescriptor.Describe(typeof(IRepository<TEntity>), typeof(TRepository), lifetime));
			services.Add(ServiceDescriptor.Describe(typeof(IRepository<TFacade>), typeof(TRepository), lifetime));

			return services;
		}

		public static IServiceCollection AddInMemoryFacadeRepository<TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			=> services
				.AddInMemoryFacadeRepository<InMemoryRepository<TEntity, TFacade>, TEntity, TFacade>();


		#endregion

		#region AddInMemoryRepositoryProvider<TEntity>

		public static IServiceCollection AddInMemoryRepositoryProvider<TProvider, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TEntity : class, IDataEntity
			where TProvider : InMemoryRepositoryProvider<TEntity> {
			services.AddRepositoryProvider<TProvider, TEntity>(lifetime);

			services.Add(ServiceDescriptor.Describe(typeof(IRepositoryProvider), typeof(TProvider), lifetime));
			services.Add(ServiceDescriptor.Describe(typeof(IRepositoryProvider<TEntity>), typeof(TProvider), lifetime));

			return services;
		}

		public static IServiceCollection AddInMemoryRepositoryProvider<TEntity>(this IServiceCollection services)
			where TEntity : class, IDataEntity
			=> services.AddInMemoryRepositoryProvider<InMemoryRepositoryProvider<TEntity>, TEntity>();

		#endregion

		#region AddInMemoryFacadeRepositoryProvider<TEntity, TFacade>

		//public static IServiceCollection AddInMemoryFacadeRepositoryProvider<TProvider, TEntity, TFacade>(this IServiceCollection services)
		//	where TEntity : class, TFacade, IEntity
		//	where TFacade : class, IEntity
		//	where TProvider : InMemoryRepositoryProvider<TEntity, TFacade>
		//	=> services
		//		.AddRepositoryProvider<TProvider, TEntity>()
		//		.AddRepositoryProvider<TProvider, TFacade>()
		//		.AddSingleton<IRepositoryProvider, TProvider>()
		//		.AddSingleton<IRepositoryProvider<TEntity>, TProvider>()
		//		.AddSingleton<IRepositoryProvider<TFacade>, TProvider>();

		//public static IServiceCollection AddInMemoryFacadeRepositoryProvider<TEntity, TFacade>(this IServiceCollection services)
		//	where TEntity : class, TFacade, IEntity
		//	where TFacade : class, IEntity
		//	=> services.AddInMemoryFacadeRepositoryProvider<InMemoryRepositoryProvider<TEntity, TFacade>, TEntity, TFacade>();

		#endregion

	}
}
