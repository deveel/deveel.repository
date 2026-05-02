using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace Deveel.Data;

[Category("Unit")]
public class DependencyInjectionTests
{
    #region AddManager

    [Test]
    public async Task Should_ResolveDefaultManager_When_ManagerForEntityRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRepository<InMemoryRepository<Person>>();

        // Act
        services.AddManagerFor<Person>();
        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<EntityManager<Person>>();

        // Assert
        await Assert.That(manager).IsNotNull();
        await Assert.That(manager.SupportsPaging).IsTrue();
        await Assert.That(manager.SupportsQueries).IsTrue();
    }

    [Test]
    public async Task Should_ResolveCustomManager_When_EntityManagerRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRepository<InMemoryRepository<Person>>();

        // Act
        services.AddEntityManager<MyPersonManager>();
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<EntityManager<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<MyPersonManager>()).IsNotNull();
        var manager = provider.GetRequiredService<EntityManager<Person>>();
        await Assert.That(manager.SupportsPaging).IsTrue();
        await Assert.That(manager.SupportsQueries).IsTrue();
    }

    [Test]
    public async Task Should_ThrowArgumentException_When_RegisteringNonManagerType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        await Assert.That(() => services.AddEntityManager<NotEntityManager>()).Throws<ArgumentException>();
    }

    #endregion

    #region AddValidator

    [Test]
    public async Task Should_ResolveValidator_When_EntityValidatorRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEntityValidator<PersonValidator>();
        var provider = services.BuildServiceProvider();

        // Assert
        var validator = provider.GetRequiredService<IEntityValidator<Person>>();
        await Assert.That(validator).IsNotNull();
    }

    #endregion

    #region AddCancellationSource

    [Test]
    public async Task Should_ResolveCancellationSource_When_OperationTokenSourceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOperationTokenSource<Deveel.TestCancellationTokenSource>();
        var provider = services.BuildServiceProvider();

        // Assert
        var source = provider.GetService<IOperationCancellationSource>();
        await Assert.That(source).IsNotNull();
        await Assert.That(source).IsTypeOf<Deveel.TestCancellationTokenSource>();
    }

    [Test]
    public async Task Should_ResolveHttpRequestCancellationSource_When_HttpRequestTokenSourceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddHttpRequestTokenSource();
        var provider = services.BuildServiceProvider();

        // Assert
        var source = provider.GetService<IOperationCancellationSource>();
        await Assert.That(source).IsNotNull();
        await Assert.That(source).IsTypeOf<HttpRequestCancellationSource>();
        await Assert.That(source!.Token).IsEqualTo(CancellationToken.None);
    }

    #endregion

    #region AddErrorFactory

    [Test]
    public async Task Should_ResolveDefaultErrorFactory_When_DefaultFactoryRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOperationErrorFactory<Person, OperationErrorFactory>();
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IOperationErrorFactory<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<OperationErrorFactory>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomErrorFactory_When_CustomFactoryRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOperationErrorFactory<Person, PersonErrorFactory>();
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IOperationErrorFactory<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<PersonErrorFactory>()).IsNotNull();
        await Assert.That(provider.GetService<OperationErrorFactory>()).IsNull();
    }

    #endregion

    #region Support Types

    private sealed class NotEntityManager { }

    private sealed class PersonValidator : IEntityValidator<Person>
    {
#pragma warning disable CS1998
        public async IAsyncEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> ValidateAsync(
            EntityManager<Person> manager,
            Person entity,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new System.ComponentModel.DataAnnotations.ValidationResult("Test error");
        }
#pragma warning restore CS1998
    }

    #endregion
}

