using Finbuckle.MultiTenant;
#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;

using System.Net.NetworkInformation;

namespace Deveel.Data
{
	public static class MultiTenantBuilderExtensions
	{
#if NET7_0_OR_GREATER
		public static MultiTenantBuilder<TTenantInfo> WithContextStrategy<TTenantInfo>(
			this MultiTenantBuilder<TTenantInfo> builder)
			where TTenantInfo : class, ITenantInfo, new()
		{
			builder.WithStrategy<ExecutionContextStreategy>(ServiceLifetime.Scoped);
			builder.Services.AddSingleton<TenantExecutionContext<TTenantInfo>>();

			return builder;
		}
#else
		public static FinbuckleMultiTenantBuilder<TTenantInfo> WithContextStrategy<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder)
	where TTenantInfo : class, ITenantInfo, new()
		{
			builder.WithStrategy<ExecutionContextStreategy>(ServiceLifetime.Scoped);
			builder.Services.AddSingleton<TenantExecutionContext<TTenantInfo>>();
			return builder;
		}
#endif
	}
}
