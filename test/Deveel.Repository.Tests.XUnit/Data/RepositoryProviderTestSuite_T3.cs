using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data
{
	public abstract class RepositoryProviderTestSuite<TPerson, TKey, TRelationship> : RepositoryTestSuite<TPerson, TKey, TRelationship>
		where TPerson : class, IPerson<TKey>
		where TRelationship : class, IRelationship
	{
		protected RepositoryProviderTestSuite(ITestOutputHelper? testOutput) : base(testOutput)
		{
		}

		public string TenantId { get; set; } = "test-tenant";

		protected override void ConfigureServices(IServiceCollection services)
		{
			ConfigureRepositoryProvider(services);
			base.ConfigureServices(services);
		}

		protected abstract void ConfigureRepositoryProvider(IServiceCollection services);

		protected override async Task<IRepository<TPerson, TKey>> GetRepositoryAsync()
		{
			var provider = Services.GetRequiredService<IRepositoryProvider<IRepository<TPerson, TKey>>>();
			return (await provider.GetRepositoryAsync(TenantId))!;
		}
	}
}
