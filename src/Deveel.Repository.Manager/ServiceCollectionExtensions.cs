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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Deveel {
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

		public static IServiceCollection AddOperationTokenSource<TSource>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TSource : class, IOperationCancellationSource {
			services.TryAdd(new ServiceDescriptor(typeof(IOperationCancellationSource), typeof(TSource), lifetime));
			services.Add(new ServiceDescriptor(typeof(TSource), typeof(TSource), lifetime));

			return services;
		}
	}
}
