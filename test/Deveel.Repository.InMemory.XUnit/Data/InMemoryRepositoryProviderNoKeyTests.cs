using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class InMemoryRepositoryProviderNoKeyTests : InMemoryRepositoryNoKeyTests {
		private readonly string tenantId = Guid.NewGuid().ToString();

		public InMemoryRepositoryProviderNoKeyTests(ITestOutputHelper outputHelper) : base(outputHelper) {
		}

		protected override IRepository<Person> Repository
			=> Services.GetRequiredService<IRepositoryProvider<Person>>().GetRepository(tenantId);

		protected override void AddRepository(IServiceCollection services) {
			services.AddRepositoryProvider<InMemoryRepositoryProvider<Person>>();
		}
	}
}
