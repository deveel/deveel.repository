using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Category("Integration")]
[InheritsTests]
public class EntityManagerNoKeyTests : EntityManagerTestSuite<EntityManager<Person>, Person>
{
    protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

    protected override string GenerateKey() => Guid.NewGuid().ToString();

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddRepository<InMemoryRepository<Person>>();
        base.ConfigureServices(services);
    }
}

