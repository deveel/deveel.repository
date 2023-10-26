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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel.Data {
	/// <summary>
	/// Extends a <see cref="IServiceCollection"/> to provide
	/// the registration of a <see cref="EntityRepository{TEntity}"/>
	/// </summary>
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Registers an Entity Framework repository for the given 
		/// type of entity in the service collection.
		/// </summary>
		/// <param name="services">
		/// The service collection to register the repository into.
		/// </param>
		/// <param name="entityType">
		/// The type of entity to register the repository for.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the repository in the service collection.
		/// </param>
		/// <remarks>
		/// This method will register a <see cref="EntityRepository{TEntity}"/>
		/// built with the given <paramref name="entityType"/> as generic argument.
		/// </remarks>
		/// <returns>
		/// Returns the service collection with the repository registered.
		/// </returns>
		public static IServiceCollection AddEntityRepository(this IServiceCollection services, Type entityType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
			var repositoryType = typeof(EntityRepository<>).MakeGenericType(entityType);
			return services.AddRepository(repositoryType, lifetime);
		}

		/// <summary>
		/// Registers an Entity Framework repository for the given
		/// type of entity in the service collection.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to register the repository for.
		/// </typeparam>
		/// <param name="services">
		/// The service collection to register the repository into.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the repository in the service collection.
		/// </param>
		/// <returns>
		/// Returns the service collection with the repository registered.
		/// </returns>
		public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TEntity : class
			=> services.AddEntityRepository(typeof(TEntity), lifetime);

		/// <summary>
		/// Registers a default implementation of the repository provider
		/// that is based on Entity Framework.
		/// </summary>
		/// <typeparam name="TContext">
		/// The type of the <see cref="DbContext"/> to use to access the
		/// data source for the given entity.
		/// </typeparam>
		/// <typeparam name="TEntity">
		/// The type of entity to register the repository provider for.
		/// </typeparam>
		/// <param name="services">
		/// The service collection to register the repository provider into.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the repository provider in the service collection.
		/// </param>
		/// <returns>
		/// Returns the service collection with the repository provider registered,
		/// for chaining.
		/// </returns>
		public static IServiceCollection AddEntityRepositoryProvider<TContext, TEntity>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped) 
			where TEntity : class
			where TContext : DbContext {
			services.AddRepositoryProvider<EntityRepositoryProvider<TContext, TEntity>>(lifetime);
			return services;
		}

		public static IServiceCollection AddEntityRepositoryProvider<TEntity, TContext>(this IServiceCollection services, Action<ITenantInfo, DbContextOptionsBuilder<TContext>> optionsFactory, ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TEntity : class
			where TContext : DbContext {
			services.AddRepositoryProvider<EntityRepositoryProvider<TContext, TEntity>>(lifetime);
			services.AddDbContextOptionsFactory(optionsFactory, lifetime);
			return services;
		}

		public static IServiceCollection AddDbContextOptionsFactory<TContext>(this IServiceCollection services, Action<ITenantInfo, DbContextOptionsBuilder<TContext>> optionsFactory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : DbContext {
			services.AddSingleton<IDbContextOptionsFactory<TContext>>(new DelegatedDbContextOptionsFactory<TContext>(optionsFactory));
			return services;
		}

		public static IServiceCollection AddDbContextOptionsFactory<TContext, TFactory>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TContext : DbContext
			where TFactory : class, IDbContextOptionsFactory<TContext> {
			services.TryAdd(new ServiceDescriptor(typeof(IDbContextOptionsFactory<TContext>), typeof(TFactory), lifetime));
			services.Add(new ServiceDescriptor(typeof(TFactory), typeof(TFactory), lifetime));
			return services;
		}

		class DelegatedDbContextOptionsFactory<TContext> : IDbContextOptionsFactory<TContext> where TContext : DbContext {
			private readonly Action<ITenantInfo, DbContextOptionsBuilder<TContext>> factory;

			public DelegatedDbContextOptionsFactory(Action<ITenantInfo, DbContextOptionsBuilder<TContext>> factory) {
				this.factory = factory;
			}

			public DbContextOptions<TContext> Create(ITenantInfo tenantInfo) {
				var builder = new DbContextOptionsBuilder<TContext>();
				factory(tenantInfo, builder);
				return builder.Options;
			}
		}
    }
}