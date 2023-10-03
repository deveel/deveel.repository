using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
    public static class ServiceCollectionExtensions {
		public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services, Action<ITenantInfo?, MongoConnectionBuilder> connectionBuilder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : class, IMongoDbContext {
			return services.AddMongoDbContext<TContext>((provider, builder) => connectionBuilder(provider.GetService<ITenantInfo>(), builder), lifetime);
		}

		public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services, Action<MongoConnectionBuilder> connectionBuilder, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : class, IMongoDbContext {
			return services.AddMongoDbContext<TContext>((IServiceProvider provider, MongoConnectionBuilder builder) => connectionBuilder(builder), lifetime);
		}
		
		private static void AddConnection<TContext>(IServiceCollection services, Action<IServiceProvider, MongoConnectionBuilder>? configure, ServiceLifetime lifetime)
			where TContext : class, IMongoDbContext {
			Func<IServiceProvider, MongoConnectionBuilder<TContext>> builderFactory = provider => {
				var builder = new MongoConnectionBuilder();
				configure?.Invoke(provider, builder);
				return new MongoConnectionBuilder<TContext>(builder);
			};

			Func<IServiceProvider, IMongoDbConnection<TContext>> connectionFactory = provider => {
				var builder = provider.GetRequiredService<MongoConnectionBuilder<TContext>>();
				return new MongoDbConnection<TContext>(builder.Connection);
			};

			services.TryAdd(new ServiceDescriptor(typeof(MongoConnectionBuilder<TContext>), builderFactory, lifetime));

			services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection<TContext>), builderFactory, lifetime));
			services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), connectionFactory, lifetime));
		}

		public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, MongoConnectionBuilder>? connectionBuilder = null, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : class, IMongoDbContext {

			AddConnection<TContext>(services, connectionBuilder, lifetime);

			if (typeof(MongoDbTenantContext).IsAssignableFrom(typeof(TContext))) {
				var contextFactory = new Func<IServiceProvider, IMongoDbTenantContext>(provider => {
					var builder = provider.GetRequiredService<MongoConnectionBuilder<TContext>>();
					var tenantInfo = provider.GetRequiredService<ITenantInfo>();

					return BuildTenantContext<TContext>(builder, tenantInfo);
				});

				services.TryAdd(new ServiceDescriptor(typeof(TContext), contextFactory, lifetime));

				if (typeof(MongoDbTenantContext) != typeof(TContext))
					services.TryAdd(new ServiceDescriptor(typeof(MongoDbTenantContext), provider => provider.GetRequiredService<TContext>(), lifetime));

				services.TryAdd(new ServiceDescriptor(typeof(IMongoDbContext), provider => provider.GetRequiredService<TContext>(), lifetime));
				services.TryAdd(new ServiceDescriptor(typeof(IMongoDbTenantContext), provider => provider.GetRequiredService<TContext>(), lifetime));
			} else {

				services.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), lifetime));
				services.TryAdd(new ServiceDescriptor(typeof(IMongoDbContext), typeof(TContext), lifetime));

				if (typeof(IMongoDbTenantContext).IsAssignableFrom(typeof(TContext)))
					services.TryAdd(new ServiceDescriptor(typeof(IMongoDbTenantContext), typeof(TContext), lifetime));

				if (typeof(MongoDbContext).IsAssignableFrom(typeof(TContext)))
					services.TryAdd(new ServiceDescriptor(typeof(MongoDbContext), typeof(TContext), lifetime));

				if (typeof(TContext) != typeof(MongoDbContext))
					services.TryAdd(new ServiceDescriptor(typeof(TContext), typeof(TContext), lifetime));
			}

			return services;
		}

		private static IMongoDbTenantContext BuildTenantContext<TContext>(MongoConnectionBuilder<TContext> builder, ITenantInfo tenantInfo) where TContext : class, IMongoDbContext {
			if (typeof(TContext) == typeof(MongoDbTenantContext))
				return new MongoDbTenantContext(builder.Connection, tenantInfo.Id);

			var ctor1 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(ITenantInfo) });
			if (ctor1 != null)
				return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { builder.Connection.ForContext<TContext>(), tenantInfo });

			var ctor2 = typeof(TContext).GetConstructor(new Type[] { typeof(IMongoDbConnection<TContext>), typeof(string) });
			if (ctor2 != null)
				return (IMongoDbTenantContext)Activator.CreateInstance(typeof(TContext), new object[] { builder.Connection.ForContext<TContext>(), tenantInfo.Id });

			throw new NotSupportedException($"Cannot create '{typeof(TContext)}' MongoDB Context");
		}
	}
}
