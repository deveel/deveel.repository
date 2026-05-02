using Bogus;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Category("Unit")]
public class DependencyInjectionTests
{
    private static readonly Faker<Person> PersonFaker = new Faker<Person>("en")
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.DateOfBirth, f => f.Date.Past(20))
        .RuleFor(x => x.Email, f => f.Internet.Email().OrNull(f))
        .RuleFor(x => x.PhoneNumber, f => f.Phone.PhoneNumber().OrNull(f));

    [Test]
    public async Task Should_CreateRepository_When_SourceIsProvided()
    {
        var items = PersonFaker.Generate(34);

        var repository = new InMemoryRepository<Person>(items);

        await Assert.That(repository).IsNotNull();
        await Assert.That(repository.Entities.Count).IsEqualTo(34);
    }

    [Test]
    public async Task Should_ThrowRepositoryException_When_ItemHasNoId()
    {
        var faker = new Faker<Person>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid().ToString().OrNull(f))
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName());
        var items = faker.Generate(34);

        await Assert.That(() => new InMemoryRepository<Person>(items)).Throws<RepositoryException>();
    }

    [Test]
    public async Task Should_ResolveAllRepositoryInterfaces_When_DefaultInMemoryRepositoryRegistered()
    {
        var services = new ServiceCollection();

        services.AddRepository<InMemoryRepository<Person>>();
        var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetService<InMemoryRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<InMemoryRepository<Person, string>>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<Person, string>>()).IsNotNull();
        await Assert.That(provider.GetService<IPageableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IPageableRepository<Person, string>>()).IsNotNull();
        await Assert.That(provider.GetService<IFilterableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IFilterableRepository<Person, string>>()).IsNotNull();
        await Assert.That(provider.GetService<IQueryableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IQueryableRepository<Person, string>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomRepositoryType_When_CustomRepositoryRegistered()
    {
        var services = new ServiceCollection();

        services.AddRepository<PersonRepository>();
        var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetService<PersonRepository>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<Person>>()).IsTypeOf<PersonRepository>();
        await Assert.That(provider.GetService<IPageableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IPageableRepository<Person>>()).IsTypeOf<PersonRepository>();
        await Assert.That(provider.GetService<IFilterableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IFilterableRepository<Person>>()).IsTypeOf<PersonRepository>();
        await Assert.That(provider.GetService<IQueryableRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<IQueryableRepository<Person>>()).IsTypeOf<PersonRepository>();
    }
}

