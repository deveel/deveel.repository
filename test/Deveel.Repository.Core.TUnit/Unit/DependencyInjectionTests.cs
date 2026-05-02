using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data;

[Category("Unit")]
public class DependencyInjectionTests
{
    #region AddRepository

    [Test]
    public async Task Should_ResolveAllContracts_When_RepositoryRegisteredFromCustomContract()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddRepository<MyPersonRepository>();

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IPersonRepository>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<MyRepository<Person>>()).IsNotNull();
        await Assert.That(provider.GetService<MyPersonRepository>()).IsNotNull();
    }

    #endregion
}

interface IPersonRepository : IRepository<Person>
{
    Task<Person?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
}

class MyRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected IRepository<TEntity> Repository { get; }

    public MyRepository()
    {
        Repository = new List<TEntity>(200).AsRepository();
    }

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Repository.AddAsync(entity, cancellationToken);

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Repository.AddRangeAsync(entities, cancellationToken);

    public Task<TEntity?> FindAsync(object key, CancellationToken cancellationToken = default) =>
        Repository.FindAsync(key, cancellationToken);

    public object? GetEntityKey(TEntity entity) =>
        Repository.GetEntityKey(entity);

    public Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Repository.RemoveAsync(entity, cancellationToken);

    public Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Repository.RemoveRangeAsync(entities, cancellationToken);

    public Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Repository.UpdateAsync(entity, cancellationToken);
}

class MyPersonRepository : MyRepository<Person>, IPersonRepository
{
    public Task<Person?> FindByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Repository.FindFirstAsync(x => x.FirstName == name, cancellationToken);
}

