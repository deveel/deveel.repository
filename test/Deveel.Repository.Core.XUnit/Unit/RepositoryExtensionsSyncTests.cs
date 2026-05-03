namespace Deveel.Data;

/// <summary>
/// Unit tests for the synchronous extension methods added to <see cref="RepositoryExtensions"/>:
/// <list type="bullet">
///   <item><see cref="RepositoryExtensions.FindFirst{TEntity}(IRepository{TEntity}, IQueryFilter)"/></item>
///   <item><see cref="RepositoryExtensions.FindFirst{TEntity, TKey}(IRepository{TEntity, TKey}, IQueryFilter)"/></item>
///   <item><see cref="RepositoryExtensions.FindAll{TEntity}(IRepository{TEntity}, IQueryFilter)"/></item>
///   <item><see cref="RepositoryExtensions.Count{TEntity}(IRepository{TEntity}, IQueryFilter)"/></item>
/// </list>
/// </summary>
[Trait("Category", "Unit")]
[Trait("Layer", "Core")]
[Trait("Feature", "RepositoryExtensions")]
public class RepositoryExtensionsSyncTests : IClassFixture<PersonFixture>
{
    private readonly List<Person> _people;
    private readonly IRepository<Person> _repository;

    public RepositoryExtensionsSyncTests(PersonFixture fixture)
    {
        _people = fixture.BuildPeople(50).ToList();
        _repository = _people.AsRepository();
    }

    private Person RandomPerson() => _people[Random.Shared.Next(0, _people.Count - 1)];

    #region FindFirst<TEntity>(IRepository<TEntity>, IQueryFilter)

    [Fact]
    public void Should_ReturnMatchingEntity_When_FindFirstCalledWithMatchingIQueryFilter()
    {
        // Arrange
        var target = RandomPerson();
        var filter = QueryFilter.Where<Person>(x => x.Id == target.Id);

        // Act
        var result = _repository.FindFirst(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(target.Id, result.Id);
        Assert.Equal(target.FirstName, result.FirstName);
        Assert.Equal(target.LastName, result.LastName);
    }

    [Fact]
    public void Should_ReturnNull_When_FindFirstCalledWithNonMatchingIQueryFilter()
    {
        // Arrange
        var filter = QueryFilter.Where<Person>(x => x.FirstName == "__no_match__");

        // Act
        var result = _repository.FindFirst(filter);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Should_ReturnFirstByFirstName_When_FindFirstCalledWithFirstNameFilter()
    {
        // Arrange
        var target = RandomPerson();
        var filter = QueryFilter.Where<Person>(x => x.FirstName == target.FirstName);
        var expected = _people.First(x => x.FirstName == target.FirstName);

        // Act
        var result = _repository.FindFirst(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.Id, result.Id);
    }

    #endregion

    #region FindFirst<TEntity, TKey>(IRepository<TEntity, TKey>, IQueryFilter)

    [Fact]
    public void Should_ReturnMatchingEntity_When_FindFirstWithKeyCalledWithMatchingIQueryFilter()
    {
        // Arrange
        var target = RandomPerson();
        IRepository<Person, object> typedRepository = _repository;
        var filter = QueryFilter.Where<Person>(x => x.Id == target.Id);

        // Act
        var result = typedRepository.FindFirst(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(target.Id, result.Id);
        Assert.Equal(target.FirstName, result.FirstName);
    }

    [Fact]
    public void Should_ReturnNull_When_FindFirstWithKeyCalledWithNonMatchingIQueryFilter()
    {
        // Arrange
        IRepository<Person, object> typedRepository = _repository;
        var filter = QueryFilter.Where<Person>(x => x.LastName == "__no_match__");

        // Act
        var result = typedRepository.FindFirst(filter);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region FindAll<TEntity>(IRepository<TEntity>, IQueryFilter)

    [Fact]
    public void Should_ReturnMatchingEntities_When_FindAllCalledWithMatchingIQueryFilter()
    {
        // Arrange
        var target = RandomPerson();
        var filter = QueryFilter.Where<Person>(x => x.FirstName == target.FirstName);
        var expectedCount = _people.Count(x => x.FirstName == target.FirstName);

        // Act
        var result = _repository.FindAll(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, p => Assert.Equal(target.FirstName, p.FirstName));
    }

    [Fact]
    public void Should_ReturnEmpty_When_FindAllCalledWithNonMatchingIQueryFilter()
    {
        // Arrange
        var filter = QueryFilter.Where<Person>(x => x.FirstName == "__no_match__");

        // Act
        var result = _repository.FindAll(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Should_ReturnAllEntities_When_FindAllCalledWithEmptyFilter()
    {
        // Arrange
        var filter = QueryFilter.Empty;

        // Act
        var result = _repository.FindAll(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_people.Count, result.Count);
    }

    #endregion

    #region Count<TEntity>(IRepository<TEntity>, IQueryFilter)

    [Fact]
    public void Should_ReturnMatchingCount_When_CountCalledWithMatchingIQueryFilter()
    {
        // Arrange
        var target = RandomPerson();
        var filter = QueryFilter.Where<Person>(x => x.FirstName == target.FirstName);
        var expectedCount = _people.LongCount(x => x.FirstName == target.FirstName);

        // Act
        var count = _repository.Count(filter);

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void Should_ReturnZero_When_CountCalledWithNonMatchingIQueryFilter()
    {
        // Arrange
        var filter = QueryFilter.Where<Person>(x => x.FirstName == "__no_match__");

        // Act
        var count = _repository.Count(filter);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public void Should_ReturnTotalCount_When_CountCalledWithEmptyFilter()
    {
        // Arrange
        var filter = QueryFilter.Empty;

        // Act
        var count = _repository.Count(filter);

        // Assert
        Assert.Equal(_people.Count, count);
    }

    [Fact]
    public void Should_ReturnCountByIdFilter_When_UniqueEntityExists()
    {
        // Arrange
        var target = RandomPerson();
        var filter = QueryFilter.Where<Person>(x => x.Id == target.Id);

        // Act
        var count = _repository.Count(filter);

        // Assert
        Assert.Equal(1L, count);
    }

    #endregion

    #region Count<TEntity, TKey>(IRepository<TEntity, TKey>, IQueryFilter)

    [Fact]
    public void Should_ReturnMatchingCount_When_CountWithKeyCalledWithMatchingIQueryFilter()
    {
        // Arrange
        var target = RandomPerson();
        IRepository<Person, object> typedRepository = _repository;
        var filter = QueryFilter.Where<Person>(x => x.FirstName == target.FirstName);
        var expectedCount = _people.LongCount(x => x.FirstName == target.FirstName);

        // Act
        var count = typedRepository.Count(filter);

        // Assert
        Assert.Equal(expectedCount, count);
    }

    [Fact]
    public void Should_ReturnZero_When_CountWithKeyCalledWithNonMatchingIQueryFilter()
    {
        // Arrange
        IRepository<Person, object> typedRepository = _repository;
        var filter = QueryFilter.Where<Person>(x => x.LastName == "__no_match__");

        // Act
        var count = typedRepository.Count(filter);

        // Assert
        Assert.Equal(0L, count);
    }

    #endregion
}

