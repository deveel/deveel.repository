using Deveel.Data.Caching;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Deveel.Data;

[Category("Unit")]
public class DependencyInjectionTests
{
    #region EntityCacheOptions

    [Test]
    public async Task Should_ResolveConfiguredOptions_When_OptionsConfiguredByAction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddEntityCacheOptions<Person>(options => {
            options.Expiration = TimeSpan.FromMinutes(15);
        });

        // Act
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<EntityCacheOptions<Person>>>();

        // Assert
        await Assert.That(options).IsNotNull();
        await Assert.That(options!.Value).IsNotNull();
        await Assert.That(options.Value.Expiration).IsEqualTo(TimeSpan.FromMinutes(15));
    }

    [Test]
    public async Task Should_ResolveConfiguredOptions_When_OptionsBindFromConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                { "EntityCacheOptions:Person:Expiration", "00:15:00" }
            });
        services.AddSingleton<IConfiguration>(config.Build());
        services.AddEntityCacheOptions<Person>("EntityCacheOptions:Person");

        // Act
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<EntityCacheOptions<Person>>>();

        // Assert
        await Assert.That(options).IsNotNull();
        await Assert.That(options!.Value).IsNotNull();
        await Assert.That(options.Value.Expiration).IsEqualTo(TimeSpan.FromMinutes(15));
    }

    #endregion

    #region EntityCache

    [Test]
    public async Task Should_ResolveEntityCache_When_InMemoryEasyCacheRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddEasyCaching(options => options.UseInMemory("default"));
        services.AddEntityEasyCacheFor<Person>(options => {
            options.Expiration = TimeSpan.FromMinutes(15);
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IEntityCache<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<EntityEasyCache<Person>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ThrowArgumentException_When_RegisteringNonCacheType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        await Assert.That(() => services.AddEntityEasyCache<NotCache>()).Throws<ArgumentException>();
    }

    #endregion

    #region CacheKeyGenerator

    [Test]
    public async Task Should_ResolveKeyGenerator_When_EntityCacheKeyGeneratorRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddEntityCacheKeyGenerator<PersonCacheKeyGenerator>();

        // Act
        var provider = services.BuildServiceProvider();
        var generator = provider.GetService<IEntityCacheKeyGenerator<Person>>();

        // Assert
        await Assert.That(generator).IsNotNull();
        await Assert.That(generator).IsTypeOf<PersonCacheKeyGenerator>();
    }

    [Test]
    public async Task Should_ResolveConverter_When_EntityEasyCacheConverterRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddEntityEasyCacheConverter<PersonCacheConverter>();

        // Act
        var provider = services.BuildServiceProvider();
        var converter = provider.GetService<IEntityEasyCacheConverter<Person, CachedPerson>>();

        // Assert
        await Assert.That(converter).IsNotNull();
        await Assert.That(converter).IsTypeOf<PersonCacheConverter>();
    }

    #endregion

    #region Support Types

    private sealed class PersonCacheConverter : IEntityEasyCacheConverter<Person, CachedPerson>
    {
        public Person ConvertFromCached(CachedPerson cached) => new Person {
            Id = cached.Id,
            FirstName = cached.FirstName,
            LastName = cached.LastName,
            DateOfBirth = cached.DateOfBirth,
            Email = cached.Email,
            PhoneNumber = cached.PhoneNumber
        };

        public CachedPerson ConvertToCached(Person entity) => new CachedPerson {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            DateOfBirth = entity.DateOfBirth,
            Email = entity.Email,
            PhoneNumber = entity.PhoneNumber
        };
    }

    private sealed class CachedPerson : Person { }

    private sealed class NotCache { }

    #endregion
}

