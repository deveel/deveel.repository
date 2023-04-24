using System;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using MongoFramework;

namespace Deveel.Data {
	public static class ServiceCollectionExtensions {
		#region AddMongoContext

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton) 
			where TContext : MongoDbContext {
			services.Add(new ServiceDescriptor(typeof(IMongoDbContext), typeof(TContext), lifetime));
			services.Add(new ServiceDescriptor(typeof(MongoDbContext), typeof(TContext), lifetime));

			if (typeof(TContext) != typeof(MongoDbContext))
				services.Add(new ServiceDescriptor(typeof(TContext), typeof(TContext), lifetime));

			return services;
		}

		public static IServiceCollection AddMongoContext(this IServiceCollection services)
			=> services.AddMongoContext<MongoDbContext>();

		public static IServiceCollection AddMongoContext<TContext>(this IServiceCollection services, string connectionString, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : MongoDbContext
			=> services.AddMongoContext<TContext>(lifetime).AddMongoConnection(connectionString);

		public static IServiceCollection AddMongoContext(this IServiceCollection services, string connectionString)
			=> services.AddMongoContext<MongoDbContext>(connectionString);

		#endregion

		#region AddMultiTenantMongoContext

		public static IServiceCollection AddMongoTenantContext<TContext>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : MongoDbTenantContext {

			if (typeof(TContext) == typeof(MongoDbTenantContext)) {
				var factory = (IServiceProvider provider) => {
					var connection = provider.GetRequiredService<IMongoDbTenantConnection>();
					return new MongoDbTenantContext(connection, connection.TenantInfo.Id);
				};

				services.Add(new ServiceDescriptor(typeof(IMongoDbContext), factory, lifetime));
				services.Add(new ServiceDescriptor(typeof(IMongoDbTenantContext), factory, lifetime));
				services.Add(new ServiceDescriptor(typeof(MongoDbContext), factory, lifetime));
				services.Add(new ServiceDescriptor(typeof(TContext), factory, lifetime));
			} else {
				services.Add(new ServiceDescriptor(typeof(IMongoDbContext), typeof(TContext), lifetime));
				services.Add(new ServiceDescriptor(typeof(IMongoDbTenantContext), typeof(TContext), lifetime));
				services.Add(new ServiceDescriptor(typeof(MongoDbContext), typeof(TContext), lifetime));
				services.Add(new ServiceDescriptor(typeof(TContext), typeof(TContext), lifetime));
			}

			return services;
		}

		public static IServiceCollection AddMongoTenantContext(this IServiceCollection services)
			=> services.AddMongoTenantContext<MongoDbTenantContext>();

		#endregion

		#region AddMongoConnection

		public static IServiceCollection AddMongoConnection<TConnection>(this IServiceCollection services)
			where TConnection : MongoDbConnection {

			services.AddSingleton<IMongoDbConnection, TConnection>();
			services.AddSingleton<MongoDbConnection, TConnection>();

			if (typeof(TConnection) != typeof(IMongoDbConnection))
				services.AddSingleton<TConnection, TConnection>();

			return services;
		}

		public static IServiceCollection AddMongoConnection(this IServiceCollection services, string connectionString) {
			if (string.IsNullOrWhiteSpace(connectionString)) 
				throw new ArgumentException($"'{nameof(connectionString)}' cannot be null or whitespace.", nameof(connectionString));

			var factory = (IServiceProvider provider) =>
				MongoDbConnection.FromConnectionString(connectionString);

			services.AddSingleton<IMongoDbConnection>(factory);
			services.AddSingleton<MongoDbConnection>(factory);

			return services;
		}

		public static IServiceCollection AddMongoTenantConnection<TTenantInfo>(this IServiceCollection services)
			where TTenantInfo : class, ITenantInfo, new() {
			services.AddSingleton<MongoDbTenantConnection, MongoDbTenantConnection<TTenantInfo>>();
			services.AddSingleton<IMongoDbTenantConnection, MongoDbTenantConnection<TTenantInfo>>();

			return services.AddMongoConnection<MongoDbTenantConnection<TTenantInfo>>();
		}

		public static IServiceCollection AddMongoTenantConnection(this IServiceCollection services) { 
			services.AddSingleton<MongoDbTenantConnection>();
			services.AddSingleton<IMongoDbTenantConnection, MongoDbTenantConnection>();

			return services.AddMongoConnection<MongoDbTenantConnection>(); 
		}

		#endregion

		#region AddMongoRepository<T>

		public static IServiceCollection AddMongoRepository<TRepository, TEntity>(this IServiceCollection services)
			where TEntity : class
			where TRepository : MongoRepository<TEntity>
			=> services
				.AddRepository<TRepository>(ServiceLifetime.Singleton)
				.AddSingleton<IRepository, TRepository>()
				.AddSingleton<IRepository<TEntity>, TRepository>();

		public static IServiceCollection AddMongoRepository<TEntity>(this IServiceCollection services)
			where TEntity : class
			=> services.AddMongoRepository<MongoRepository<TEntity>, TEntity>();

		#endregion

		#region AddMongoRepositoryProvider<TEntity>

		public static IServiceCollection AddMongoRepositoryProvider<TTenantInfo, TProvider, TEntity>(this IServiceCollection services)
			where TTenantInfo : class, ITenantInfo, new()
			where TEntity : class
			where TProvider : MongoRepositoryProvider<TEntity, TTenantInfo>
			=> services
				.AddRepositoryProvider<TProvider, TEntity>(ServiceLifetime.Singleton)
				.AddSingleton<IRepositoryProvider, TProvider>()
				.AddSingleton<IRepositoryProvider<TEntity>, TProvider>();

		public static IServiceCollection AddMongoRepositoryProvider<TTenantInfo, TEntity>(this IServiceCollection services)
			where TTenantInfo : class, ITenantInfo, new()
			where TEntity : class
			=> services.AddMongoRepositoryProvider<TTenantInfo, MongoRepositoryProvider<TEntity, TTenantInfo>, TEntity>();


		#endregion
	}
}
