using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddPaginationOptions(this IServiceCollection services, string sectionName) {
			services.AddOptions<PaginationModelOptions>()
				.Configure<IConfiguration>((options, config) => config.GetSection(sectionName)?.Bind(options));

			return services;
		}

		public static IServiceCollection AddPaginationOptions(this IServiceCollection services, Action<PaginationModelOptions> configure) {
			services.AddOptions<PaginationModelOptions>()
				.Configure(configure);

			return services;
		}
	}
}
