using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public class InMemoryRepositoryProviderTests : InMemoryRepositoryTests {
		private readonly string tenantId = Guid.NewGuid().ToString();

		protected override IRepository<Person> Repository 
			=> Services.GetRequiredService<InMemoryRepositoryProvider<Person>>().GetRepository(tenantId);

		protected override void AddRepository(IServiceCollection services) {
			services.AddInMemoryRepository<Person>()
				.UseProvider<InMemoryRepositoryProvider<Person>>();
		}
	}
}
