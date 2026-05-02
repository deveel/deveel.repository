using Bogus;

using Deveel.Data.Caching;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Category("Integration")]
public class PersonManagerCachingTests : EntityManagerTestSuite<PersonManager, Person, string>
{
    protected override Faker<Person> PersonFaker { get; } = new PersonFaker();

    protected override string GenerateKey() => Guid.NewGuid().ToString();

    protected override void SetKey(Person person, string key)
    {
        person.Id = key;
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddRepository<InMemoryRepository<Person>>();
        services.AddEasyCaching(options => options.UseInMemory("default"));
        services.AddEntityEasyCacheFor<Person>(options => {
            options.Expiration = TimeSpan.FromMinutes(15);
        });
        services.AddEntityCacheKeyGenerator<PersonCacheKeyGenerator>();
        base.ConfigureServices(services);
    }

    [Test]
    [Category("Integration")]
    public async Task Should_FindPersonByEmail_When_EntityExistsInCache()
    {
        // Arrange
        var person = People.Random(x => x.Email != null);

        await Assert.That(person).IsNotNull();
        await Assert.That(person!.Email).IsNotNull();

        // Act
        var found = await Manager.FindByEmailAsync(person.Email!);

        // Assert
        await Assert.That(found).IsNotNull();
        await Assert.That(found!.Id).IsEqualTo(person.Id);
        await Assert.That(found.Email).IsEqualTo(person.Email);
    }
}

