using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MongoFramework;

namespace Deveel.Data {
	public static class ServiceCollectionExtensions {
		#region AddMongoContext

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services) where TContext : MongoDbContext {
			services.AddSingleton<IMongoDbConnection, MongoDbConfiguredConnection>();
			services.AddSingleton<MongoDbContext, TContext>();
			services.AddSingleton<TContext>();

			return services;
		}

		public static IServiceCollection AddMongoContext(this IServiceCollection services)
			=> services.AddMongoContext<MongoDbContext>();

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services, string sectionName) 
			where TContext : MongoDbContext {
			services.AddOptions<MongoDbConnectionOptions>()
				.Configure<IConfiguration>((options, config) => config.GetSection(sectionName)?.Bind(options));

			return services.AddMongoContext<TContext>();
		}

		public static IServiceCollection AddMongoContext(this IServiceCollection services, string sectionName)
			=> services.AddMongoContext<MongoDbContext>(sectionName);

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services, Action<MongoDbConnectionOptions> configure)
			where TContext : MongoDbContext {
			services.AddOptions<MongoDbConnectionOptions>()
				.Configure(configure);

			return services.AddMongoContext<TContext>();
		}

		public static IServiceCollection AddMongoContext(this IServiceCollection services, Action<MongoDbConnectionOptions> configure)
			=> services.AddMongoContext<MongoDbContext>(configure);


		#endregion

		#region AddMultiTenantMongoContext

		public static IServiceCollection AddMongoTenantContext<TContext>(this IServiceCollection services)
			where TContext : MongoPerTenantContext {

			services.AddScoped<MongoPerTenantContext, TContext>();
			services.AddScoped<MongoDbTenantContext, TContext>();
			services.AddScoped<TContext>();

			return services;
		}

		public static IServiceCollection AddMongoTenantContext(this IServiceCollection services)
			=> services.AddMongoTenantContext<MongoPerTenantContext>();

		#endregion

		#region AddMongoRepository<T>

		public static IServiceCollection AddMongoRepository<TRepository, TEntity>(this IServiceCollection services)
			where TEntity : class, IDataEntity
			where TRepository : MongoRepository<TEntity>
			=> services
				.AddRepository<TRepository>(ServiceLifetime.Singleton)
				.AddSingleton<IRepository, TRepository>()
				.AddSingleton<IRepository<TEntity>, TRepository>();

		public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services)
			where TEntity : class, IDataEntity
			=> services.AddMongoRepository<MongoRepository<TEntity>, TEntity>();

		#endregion

		#region AddMongoFacadeRepository<TEntity,TFacade>

		public static IServiceCollection AddMongoFacadeRepository<TRepository, TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			where TRepository : MongoRepository<TEntity, TFacade>
			=> services
				.AddRepository<TRepository, TEntity>(ServiceLifetime.Singleton)
				.AddRepository<TRepository, TFacade>(ServiceLifetime.Singleton)
				.AddSingleton<IRepository, TRepository>()
				.AddSingleton<IRepository<TEntity>, TRepository>()
				.AddSingleton<IRepository<TFacade>, TRepository>();

		public static IServiceCollection AddMongoFacadeRepository<TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			=> services
				.AddMongoFacadeRepository<MongoRepository<TEntity, TFacade>, TEntity, TFacade>();


		#endregion

		#region AddMongoRepositoryProvider<TEntity>

		public static IServiceCollection AddMongoRepositoryProvider<TProvider, TEntity>(this IServiceCollection services)
			where TEntity : class, IDataEntity
			where TProvider : MongoRepositoryProvider<TEntity>
			=> services
				.AddRepositoryProvider<TProvider, TEntity>(ServiceLifetime.Singleton)
				.AddSingleton<IRepositoryProvider, TProvider>()
				.AddSingleton<IRepositoryProvider<TEntity>, TProvider>();

		public static IServiceCollection AddMongoRepositoryProvider<TEntity>(this IServiceCollection services)
			where TEntity : class, IDataEntity
			=> services.AddMongoRepositoryProvider<MongoRepositoryProvider<TEntity>, TEntity>();


		#endregion

		#region AddMongoFacadeRepositoryProvider<TEntity, TFacade>

		public static IServiceCollection AddMongoFacadeRepositoryProvider<TProvider, TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			where TProvider : MongoRepositoryProvider<TEntity, TFacade>
			=> services
				.AddRepositoryProvider<TProvider, TEntity>()
				.AddRepositoryProvider<TProvider, TFacade>()
				.AddSingleton<IRepositoryProvider, TProvider>()
				.AddSingleton<IRepositoryProvider<TEntity>, TProvider>()
				.AddSingleton<IRepositoryProvider<TFacade>, TProvider>();

		public static IServiceCollection AddMongoFacadeRepositoryProvider<TEntity, TFacade>(this IServiceCollection services)
			where TEntity : class, TFacade, IDataEntity
			where TFacade : class, IDataEntity
			=> services.AddMongoFacadeRepositoryProvider<MongoRepositoryProvider<TEntity, TFacade>, TEntity, TFacade>();

		#endregion


	}
}
