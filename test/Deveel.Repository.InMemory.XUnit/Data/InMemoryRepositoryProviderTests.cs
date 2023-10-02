using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class InMemoryRepositoryProviderTests : InMemoryRepositoryTests {
		private readonly string tenantId = Guid.NewGuid().ToString();

		public InMemoryRepositoryProviderTests(ITestOutputHelper outputHelper) : base(outputHelper) {
		}

		protected override IRepository<Person> Repository 
			=> Services.GetRequiredService<IRepositoryProvider<Person>>().GetRepository(tenantId);

		protected override void AddRepository(IServiceCollection services) {
			services.AddInMemoryRepository<Person>()
				.UseProvider<InMemoryRepositoryProvider<Person>>();
		}
	}
}
