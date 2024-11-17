// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MongoFramework;

namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IServiceCollection"/> to provide methods
	/// to register a <see cref="IMongoDbContext"/> in service collections.
	/// </summary>
    public static class ServiceCollectionExtensions {
		/// <summary>
		/// Adds a <see cref="IMongoDbContext"/> to the service collection.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context to register.
		/// </typeparam>
		/// <param name="services">
		/// The service collection to add the context to.
		/// </param>
		/// <param name="connectionBuilder">
		/// A delegate to a method that builds the connection string.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the context in the service collection.
		/// </param>
		/// <returns>
		/// Returns the service collection for chaining.
		/// </returns>
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
				if (builder.IsUsingTenant)
				{
					var tenantInfo = provider.GetRequiredService(builder.TenantType!) as MongoDbTenantInfo;
					if (tenantInfo == null)
						throw new InvalidOperationException("No tenant information was found in the service collection");

					return new MongoDbConnection<TContext>(MongoDbConnection.FromConnectionString(tenantInfo.ConnectionString));
				}

				return new MongoDbConnection<TContext>(builder.Connection);
			};

			services.TryAdd(new ServiceDescriptor(typeof(MongoConnectionBuilder<TContext>), builderFactory, lifetime));

			services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection<TContext>), connectionFactory, lifetime));
			services.TryAdd(new ServiceDescriptor(typeof(IMongoDbConnection), connectionFactory, lifetime));
		}

		/// <summary>
		/// Adds a <see cref="IMongoDbContext"/> to the service collection.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the context to register.
		/// </typeparam>
		/// <param name="services">
		/// The service collection to add the context to.
		/// </param>
		/// <param name="connectionBuilder">
		/// A delegate to a method that builds the connection string.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the context in the service collection.
		/// </param>
		/// <returns>
		/// Returns the service collection for chaining.
		/// </returns>
		public static IServiceCollection AddMongoDbContext<TContext>(this IServiceCollection services, Action<IServiceProvider, MongoConnectionBuilder>? connectionBuilder = null, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : class, IMongoDbContext {

			AddConnection<TContext>(services, connectionBuilder, lifetime);

			if (typeof(IMongoDbTenantContext).IsAssignableFrom(typeof(TContext))) {
				var contextFactory = new Func<IServiceProvider, IMongoDbTenantContext>(provider => {
					var builder = provider.GetRequiredService<MongoConnectionBuilder<TContext>>();
					var tenantInfo = provider.GetRequiredService<MongoDbTenantInfo>();

					return (IMongoDbTenantContext) MongoDbContextUtil.CreateContext<TContext>(builder, tenantInfo);
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

		public static IServiceCollection AddMongoRepositoryProvider(this IServiceCollection services, Type repositoryType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
		{
			var providerType = typeof(MongoRepositoryProvider<>).MakeGenericType(repositoryType);
			return services.AddRepositoryProvider(providerType, lifetime);
		}

		public static IServiceCollection AddMongoRepositoryProvider<TRepository>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TRepository : class
		{
			return services.AddMongoRepositoryProvider(typeof(TRepository), lifetime);
		}
	}
}
