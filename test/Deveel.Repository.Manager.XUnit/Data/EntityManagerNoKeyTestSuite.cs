using Bogus;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class EntityManagerNoKeyTestSuite : EntityManagerTestSuite<EntityManager<Person>, Person> {
		public EntityManagerNoKeyTestSuite(ITestOutputHelper testOutput) : base(testOutput) {
		}

		protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

		protected override string GenerateKey() => Guid.NewGuid().ToString();

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddRepository<InMemoryRepository<Person>>();
			// services.AddEntityValidator<PersonValidator>();
			base.ConfigureServices(services);
		}
	}
}
