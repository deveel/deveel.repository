using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Trait("Category", "Integration")]
[Trait("Layer", "Application")]
[Trait("Feature", "EntityManager")]
public class EntityManagerNoKeyTests : EntityManagerTestSuite<EntityManager<Person>, Person> {
	public EntityManagerNoKeyTests(ITestOutputHelper testOutput) : base(testOutput) {
	}

	protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

	protected override string GenerateKey() => Guid.NewGuid().ToString();

	protected override void ConfigureServices(IServiceCollection services) {
		services.AddRepository<InMemoryRepository<Person>>();
		base.ConfigureServices(services);
	}
}
