using Bogus;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class InMemoryRepositoryTests : RepositoryTestSuite<Person> {
		public InMemoryRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper) {
		}

		protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

		protected override void ConfigureServices(IServiceCollection services) {
			AddRepository(services);

			base.ConfigureServices(services);
		}

		protected virtual void AddRepository(IServiceCollection services) {
			services.AddRepository<InMemoryRepository<Person>>();
			services.AddRepositoryController();
		}
	}
}
