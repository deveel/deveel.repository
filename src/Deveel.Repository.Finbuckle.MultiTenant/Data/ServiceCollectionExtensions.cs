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

namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IServiceCollection"/> to add methods
	/// to register the <see cref="IRepositoryTenantResolver"/> service.
	/// </summary>
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Registers the default <see cref="IRepositoryTenantResolver"/> service.
		/// </summary>
		/// <typeparam name="TTenantInfo">
		/// The type of the tenant information object.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the resolver into.
		/// </param>
		/// <returns>
		/// Returns the same instance of the <paramref name="services"/>
		/// for chaining calls.
		/// </returns>
		public static IServiceCollection AddRepositoryTenantResolver<TTenantInfo>(this IServiceCollection services)
			where TTenantInfo : class, ITenantInfo, new()
			=> services.AddRepositoryTenantResolver(typeof(RepositoryTenantResolver<TTenantInfo>), ServiceLifetime.Scoped);

		/// <summary>
		/// Registers the given tenant resolver service.
		/// </summary>
		/// <param name="services">
		/// The collection of services to register the resolver into.
		/// </param>
		/// <param name="serviceType">
		/// The type of the resolver to register.
		/// </param>
		/// <param name="lifetime">
		/// The lifetime of the service.
		/// </param>
		/// <returns>
		/// Returns the same instance of the <paramref name="services"/>
		/// for chaining calls.
		/// </returns>
		/// <exception cref="ArgumentException"></exception>
		public static IServiceCollection AddRepositoryTenantResolver(this IServiceCollection services, Type serviceType, ServiceLifetime lifetime = ServiceLifetime.Scoped) {
			ArgumentNullException.ThrowIfNull(serviceType);

			if (!typeof(IRepositoryTenantResolver).IsAssignableFrom(serviceType))
				throw new ArgumentException($"The type {serviceType} is not assignable to {typeof(IRepositoryTenantResolver)}");

			if (!serviceType.IsClass || serviceType.IsAbstract)
				throw new ArgumentException($"The type {serviceType} is not a concrete class");

			services.TryAdd(ServiceDescriptor.Describe(typeof(IRepositoryTenantResolver), serviceType, lifetime));
			services.Add(ServiceDescriptor.Describe(serviceType, serviceType, lifetime));
			return services;
		}
	}
}
