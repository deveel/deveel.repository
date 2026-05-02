using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Category("Integration")]
public class EntityManagerTests : EntityManagerTestSuite<EntityManager<Person, string>, Person, string>
{
    protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

    protected override string GenerateKey() => Guid.NewGuid().ToString();

    protected override void SetKey(Person person, string key)
    {
        person.Id = key;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddRepository<InMemoryRepository<Person, string>>();
        base.ConfigureServices(services);
    }
}

