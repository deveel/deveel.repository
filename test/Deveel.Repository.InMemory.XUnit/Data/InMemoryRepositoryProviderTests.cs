using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class InMemoryRepositoryProviderTests : InMemoryRepositoryTests {
		private readonly string tenantId = Guid.NewGuid().ToString();

		public InMemoryRepositoryProviderTests(ITestOutputHelper outputHelper) : base(outputHelper) {
		}

		protected override IRepository<Person, string> Repository 
			=> Services.GetRequiredService<IRepositoryProvider<Person, string>>().GetRepository(tenantId);

		protected override void AddRepository(IServiceCollection services) {
			services.AddRepositoryProvider<InMemoryRepositoryProvider<Person, string>>();
		}
	}
}
