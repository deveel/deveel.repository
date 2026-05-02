namespace Deveel.Data;

/// <summary>
/// TUnit equivalent of the xUnit <c>ListAsRepositoryTests</c> class.
/// <para>
/// Uses <see cref="ClassDataSourceAttribute{T}"/> with <see cref="SharedType.PerClass"/>
/// in place of xUnit's <c>IClassFixture&lt;PersonFixture&gt;</c> — the fixture is
/// created once per test class and passed to the constructor for each test.
/// </para>
/// <para>
/// TUnit injects <see cref="CancellationToken"/> directly as a test-method parameter
/// rather than via <c>TestContext.Current.CancellationToken</c>.
/// </para>
/// </summary>
[ClassDataSource<PersonFixture>(Shared = SharedType.PerClass)]
[Category("Unit")]
public class ListAsRepositoryTests
{
    private readonly List<Person> _people;
    private readonly IRepository<Person> _repository;

    public ListAsRepositoryTests(PersonFixture fixture)
    {
        _people = fixture.BuildPeople(100).ToList();
        _repository = _people.AsRepository();
    }

    private Person RandomPerson() => _people[Random.Shared.Next(0, _people.Count - 1)];

    #region Add

    [Test]
    public async Task Should_ThrowNotSupportedException_When_AddingToReadOnlyRepository(CancellationToken cancellationToken)
    {
        // Arrange
        
        var readOnly = _people.AsReadOnly().AsRepository();
        var newPerson = PersonFixture.PersonFaker.Generate();

        // Act & Assert
        await Assert.That(async () => await readOnly.AddAsync(newPerson, cancellationToken))
            .Throws<NotSupportedException>();
    }

    [Test]
    public async Task Should_IncrementCount_When_AddingToMutableRepository(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var newPerson = PersonFixture.PersonFaker.Generate();

        // Act
        await _repository.AddAsync(newPerson, cancellationToken);

        // Assert
        await Assert.That(await _repository.CountAllAsync(cancellationToken)).IsEqualTo(initialCount + 1);
        await Assert.That(newPerson.Id).IsNotNull();
    }

    [Test]
    public async Task Should_AssignNewId_When_AddingPersonWithExistingId(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var person = PersonFixture.PersonFaker.Generate();
        var originalId = person.Id;

        // Act
        await _repository.AddAsync(person, cancellationToken);

        // Assert
        await Assert.That(_repository.CountAll()).IsEqualTo(initialCount + 1);
        await Assert.That(person.Id).IsNotEqualTo(originalId);
    }

    [Test]
    public async Task Should_IncrementCountByRange_When_AddingMultiplePersons(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var newPeople = PersonFixture.PersonFaker.Generate(10);

        // Act
        await _repository.AddRangeAsync(newPeople, cancellationToken);

        // Assert
        await Assert.That(_repository.CountAll()).IsEqualTo(initialCount + 10);
        foreach (var p in newPeople)
            await Assert.That(p.Id).IsNotNull();
    }

    [Test]
    public async Task Should_ThrowNotSupportedException_When_AddingRangeToReadOnlyRepository(CancellationToken cancellationToken)
    {
        // Arrange
        
        var readOnly = _people.AsReadOnly().AsRepository();
        var newPeople = PersonFixture.PersonFaker.Generate(10);

        // Act & Assert
        await Assert.That(async () => await readOnly.AddRangeAsync(newPeople, cancellationToken))
            .Throws<NotSupportedException>();
    }

    #endregion

    #region Remove

    [Test]
    public async Task Should_DecrementCount_When_RemovingExistingPerson(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var target = RandomPerson();

        // Act
        var result = await _repository.RemoveAsync(target, cancellationToken);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(_repository.AsFilterable().CountAll()).IsEqualTo(initialCount - 1);
    }

    [Test]
    public async Task Should_DecrementCount_When_RemovingByExistingKey(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var target = RandomPerson();

        // Act
        var result = await _repository.RemoveByKeyAsync(target.Id!, cancellationToken);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(_repository.AsFilterable().CountAll()).IsEqualTo(initialCount - 1);
    }

    [Test]
    public async Task Should_ReturnFalse_When_RemovingByKeyThatDoesNotExist(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var missingId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.RemoveByKeyAsync(missingId, cancellationToken);

        // Assert
        await Assert.That(result).IsFalse();
        await Assert.That(_repository.AsFilterable().CountAll()).IsEqualTo(initialCount);
    }

    [Test]
    public async Task Should_RemoveAllSpecified_When_RemovingRange(CancellationToken cancellationToken)
    {
        // Arrange
        
        var initialCount = _people.Count;
        var toRemove = _people.Take(Math.Min(10, _people.Count)).ToList();

        // Act
        await _repository.RemoveRangeAsync(toRemove, cancellationToken);

        // Assert
        foreach (var p in toRemove)
            await Assert.That(_repository.Find(p.Id!)).IsNull();
        await Assert.That(_people.Count).IsEqualTo(initialCount - toRemove.Count);
    }

    [Test]
    public async Task Should_ThrowNotSupportedException_When_RemovingFromReadOnlyRepository(CancellationToken cancellationToken)
    {
        // Arrange
        
        var readOnly = _people.AsReadOnly().AsRepository();
        var target = RandomPerson();

        // Act & Assert
        await Assert.That(async () => await readOnly.RemoveAsync(target, cancellationToken))
            .Throws<NotSupportedException>();
    }

    #endregion

    #region Filter

    [Test]
    public async Task Should_ReturnTrue_When_FilterMatchesExistingPerson(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();

        // Act
        var result = await _repository.ExistsAsync(x => x.LastName == target.LastName, cancellationToken);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task Should_ReturnTotalCount_When_CountingAllWithoutFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        await Assert.That(_repository).IsAssignableTo<IFilterableRepository<Person>>();

        // Act
        var count = await _repository.AsFilterable().CountAllAsync(cancellationToken);

        // Assert
        await Assert.That(count).IsEqualTo(_people.Count);
    }

    [Test]
    public async Task Should_ReturnMatchingCount_When_CountingWithFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();
        var expected = _people.Count(x => x.FirstName == target.FirstName);

        // Act
        var count = await _repository.AsFilterable().CountAsync(x => x.FirstName == target.FirstName, cancellationToken);

        // Assert
        await Assert.That(count).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnZero_When_CountingWithNonMatchingFilter(CancellationToken cancellationToken)
    {
        // Arrange
        

        // Act
        var count = await _repository.AsFilterable().CountAsync(x => x.FirstName == "__no_match__", cancellationToken);

        // Assert
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task Should_ReturnEntity_When_FindingFirstWithMatchingFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();

        // Act
        var found = await _repository.FindFirstAsync(QueryFilter.Where<Person>(x => x.Id == target.Id), cancellationToken);

        // Assert
        await Assert.That(found).IsNotNull();
        await Assert.That(found!.Id).IsEqualTo(target.Id);
        await Assert.That(found.FirstName).IsEqualTo(target.FirstName);
        await Assert.That(found.LastName).IsEqualTo(target.LastName);
    }

    [Test]
    public async Task Should_ReturnAllEntities_When_FindingAllWithoutFilter(CancellationToken cancellationToken)
    {
        // Arrange
        

        // Act
        var result = await _repository.AsFilterable().FindAllAsync(cancellationToken: cancellationToken);

        // Assert
        await Assert.That(result.Count).IsEqualTo(_people.Count);
    }

    [Test]
    public async Task Should_ReturnMatchingEntities_When_FindingAllWithFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();
        var expected = _people.Count(x => x.FirstName == target.FirstName);

        // Act
        var result = await _repository.AsFilterable().FindAllAsync(x => x.FirstName == target.FirstName, cancellationToken: cancellationToken);

        // Assert
        await Assert.That(result.Count).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnEmpty_When_FindingAllWithNonMatchingFilter(CancellationToken cancellationToken)
    {
        // Arrange
        

        // Act
        var result = await _repository.AsFilterable().FindAllAsync(x => x.FirstName == "__no_match__", cancellationToken: cancellationToken);

        // Assert
        await Assert.That(result.Count).IsEqualTo(0);
    }

    #endregion

    #region GetPage

    [Test]
    public async Task Should_ReturnFirstPage_When_GettingPageWithoutFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var totalPages = (int)Math.Ceiling(_people.Count / 10.0);
        var request = new PageQuery<Person>(1, 10);

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.Request.Size).IsEqualTo(10);
        await Assert.That(page.Request.Page).IsEqualTo(1);
        await Assert.That(page.TotalItems).IsEqualTo(_people.Count);
        await Assert.That(page.TotalPages).IsEqualTo(totalPages);
        await Assert.That(page.Items).IsNotNull();
        await Assert.That(page.Items.Count).IsEqualTo(10);
    }

    [Test]
    public async Task Should_ReturnFilteredPage_When_GettingPageWithFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();
        var subset = _people.Where(x => x.FirstName == target.FirstName).ToList();
        var expectedItemCount = Math.Min(10, subset.Count);
        var expectedTotalPages = (int)Math.Ceiling(subset.Count / 10.0);
        var request = new PageQuery<Person>(1, 10).Where(x => x.FirstName == target.FirstName);

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.TotalItems).IsEqualTo(subset.Count);
        await Assert.That(page.TotalPages).IsEqualTo(expectedTotalPages);
        await Assert.That(page.Items.Count).IsEqualTo(expectedItemCount);
    }

    [Test]
    public async Task Should_ReturnEmptyPage_When_GettingPageWithNonMatchingFilter(CancellationToken cancellationToken)
    {
        // Arrange
        
        var request = new PageQuery<Person>(1, 10).Where(x => x.FirstName == "__no_match__");

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.TotalItems).IsEqualTo(0);
        await Assert.That(page.TotalPages).IsEqualTo(0);
        await Assert.That(page.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Should_ReturnSortedPage_When_GettingPageWithAscendingSort(CancellationToken cancellationToken)
    {
        // Arrange
        
        var request = new PageQuery<Person>(1, 10).OrderBy(x => x.FirstName);
        var expectedFirst = _people.OrderBy(x => x.FirstName).First().FirstName;

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.Items.Count).IsEqualTo(10);
        await Assert.That(page.Items.First().FirstName).IsEqualTo(expectedFirst);
    }

    [Test]
    public async Task Should_ReturnSortedPage_When_GettingPageWithDescendingSort(CancellationToken cancellationToken)
    {
        // Arrange
        
        var request = new PageQuery<Person>(1, 10).OrderByDescending(x => x.FirstName);
        var expectedFirst = _people.OrderByDescending(x => x.FirstName).First().FirstName;

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.Items.Count).IsEqualTo(10);
        await Assert.That(page.Items.First().FirstName).IsEqualTo(expectedFirst);
    }

    [Test]
    public async Task Should_ReturnSortedPage_When_GettingPageWithFieldNameSort(CancellationToken cancellationToken)
    {
        // Arrange
        
        var request = new PageQuery<Person>(1, 10).OrderBy("FirstName");
        var expectedFirst = _people.OrderBy(x => x.FirstName).First().FirstName;

        // Act
        var page = await _repository.GetPageAsync(request, cancellationToken);

        // Assert
        await Assert.That(page.Items.Count).IsEqualTo(10);
        await Assert.That(page.Items.First().FirstName).IsEqualTo(expectedFirst);
    }

    #endregion

    #region Update

    [Test]
    public async Task Should_ReturnTrue_When_UpdatingExistingPerson(CancellationToken cancellationToken)
    {
        // Arrange
        
        var target = RandomPerson();
        var originalId = target.Id;
        var newFirstName = PersonFixture.PersonFaker.Generate().FirstName;
        target.FirstName = newFirstName;

        // Act
        var result = await _repository.UpdateAsync(target, cancellationToken);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(target.Id).IsEqualTo(originalId);
        await Assert.That(target.FirstName).IsEqualTo(newFirstName);
    }

    [Test]
    public async Task Should_ReturnFalse_When_UpdatingPersonThatDoesNotExist(CancellationToken cancellationToken)
    {
        // Arrange
        
        var nonExistent = PersonFixture.PersonFaker.Generate();

        // Act
        var result = await _repository.UpdateAsync(nonExistent, cancellationToken);

        // Assert
        await Assert.That(result).IsFalse();
    }

    #endregion
}




