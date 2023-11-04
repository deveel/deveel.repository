using Bogus;

using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	public class EntityManagerTestSuite : EntityManagerTestSuite<EntityManager<Person, string>, Person, string> {
		public EntityManagerTestSuite(ITestOutputHelper testOutput) : base(testOutput) {
		}

		protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

		protected override string GenerateKey() => Guid.NewGuid().ToString();

		protected override void SetKey(Person person, string key) {
			person.Id = key;
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddRepository<InMemoryRepository<Person, string>>();
			// services.AddEntityValidator<PersonValidator>();
			base.ConfigureServices(services);
		}
	}
}
