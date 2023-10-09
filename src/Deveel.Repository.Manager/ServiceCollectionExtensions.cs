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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel {
	/// <summary>
	/// Provides methods to register entity management services
	/// in a collection of services.
	/// </summary>
	public static class ServiceCollectionExtensions {
		/// <summary>
		/// Registers a <see cref="IOperationErrorFactory"/> service in the
		/// collection of services.
		/// </summary>
		/// <typeparam name="TFactory">
		/// The type of the operation error factory to register.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the factory.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the factory.
		/// </param>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		public static IServiceCollection AddOperationErrorFactory<TFactory>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
			where TFactory : class, IOperationErrorFactory {
			services.TryAdd(new ServiceDescriptor(typeof(IOperationErrorFactory), typeof(TFactory), lifetime));
			services.Add(new ServiceDescriptor(typeof(TFactory), typeof(TFactory), lifetime));
			return services;
		}

		/// <summary>
		/// Registers the operation cancellation source in the 
		/// collection of services.
		/// </summary>
		/// <typeparam name="TSource">
		/// The type of the operation cancellation source to register.
		/// </typeparam>
		/// <param name="services">
		/// The collection of services to register the source.
		/// </param>
		/// <param name="lifetime">
		/// The desired lifetime of the cancellation source.
		/// </param>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		public static IServiceCollection AddOperationTokenSource<TSource>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TSource : class, IOperationCancellationSource {
			services.TryAdd(new ServiceDescriptor(typeof(IOperationCancellationSource), typeof(TSource), lifetime));
			services.Add(new ServiceDescriptor(typeof(TSource), typeof(TSource), lifetime));

			return services;
		}

		/// <summary>
		/// Registers a singleton instance of the <see cref="HttpRequestCancellationSource"/> 
		/// in the collection of services.
		/// </summary>
		/// <param name="services">
		/// The collection of services to register the source.
		/// </param>
		/// <remarks>
		/// This method also tries to register the <see cref="IHttpContextAccessor"/>
		/// into the collection of services, if not already registered.
		/// </remarks>
		/// <returns>
		/// Returns the given collection of services for chaining calls.
		/// </returns>
		public static IServiceCollection AddHttpRequestTokenSource(this IServiceCollection services) {
			services.AddHttpContextAccessor();
			services.AddOperationTokenSource<HttpRequestCancellationSource>(ServiceLifetime.Singleton);

			return services;
		}
	}
}
