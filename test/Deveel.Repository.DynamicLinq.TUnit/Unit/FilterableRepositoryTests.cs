namespace Deveel.Data;

[Category("Unit")]
public class FilterableRepositoryTests
{
    private static readonly Faker<Person> PersonFaker = new Faker<Person>("en")
        .RuleFor(x => x.Id, f => f.Random.Guid().ToString())
        .RuleFor(x => x.FirstName, f => f.Name.FirstName())
        .RuleFor(x => x.LastName, f => f.Name.LastName())
        .RuleFor(x => x.DateOfBirth, f => f.Date.Past(20))
        .RuleFor(x => x.Email, f => f.Internet.Email().OrNull(f))
        .RuleFor(x => x.Phone, f => f.Phone.PhoneNumber().OrNull(f));

    private readonly IList<Person> _persons;
    private readonly IRepository<Person> _repository;

    public FilterableRepositoryTests()
    {
        _persons = PersonFaker.Generate(100).ToList();
        _repository = _persons.AsRepository();
    }

    private Person RandomPerson() => _persons[Random.Shared.Next(0, _persons.Count - 1)];

    #region CountAsync

    [Test]
    public async Task Should_ReturnFilteredCount_When_ParameterNameProvided(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Count(x => x.FirstName == person.FirstName);

        var count = await _repository.CountAsync("p", $"p.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(count).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnFilteredCount_When_NoParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Count(x => x.FirstName == person.FirstName);

        var count = await _repository.CountAsync($"x.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(count).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_CountExpressionInvalid(CancellationToken cancellationToken)
    {
        await Assert.That(async () => await _repository.CountAsync("x.FirstName", cancellationToken))
            .Throws<InvalidOperationException>();
    }

    #endregion

    #region ExistsAsync

    [Test]
    public async Task Should_ReturnTrue_When_EntityMatchesExpressionWithParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Any(x => x.FirstName == person.FirstName);

        var result = await _repository.ExistsAsync("p", $"p.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnTrue_When_EntityMatchesExpressionWithoutParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Any(x => x.FirstName == person.FirstName);

        var result = await _repository.ExistsAsync($"x.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_ExistsExpressionInvalid(CancellationToken cancellationToken)
    {
        await Assert.That(async () => await _repository.ExistsAsync("x.FirstName", cancellationToken))
            .Throws<InvalidOperationException>();
    }

    #endregion

    #region FindFirstAsync

    [Test]
    public async Task Should_ReturnFirstMatch_When_ExpressionWithParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.FirstOrDefault(x => x.FirstName == person.FirstName);

        var result = await _repository.FindFirstAsync("p", $"p.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnFirstMatch_When_ExpressionWithoutParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.FirstOrDefault(x => x.FirstName == person.FirstName);

        var result = await _repository.FindFirstAsync<Person>($"x.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_FindFirstExpressionInvalid(CancellationToken cancellationToken)
    {
        await Assert.That(async () => await _repository.FindFirstAsync<Person>("x.FirstName", cancellationToken))
            .Throws<InvalidOperationException>();
    }

    #endregion

    #region FindAllAsync

    [Test]
    public async Task Should_ReturnAllMatches_When_ExpressionWithParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Where(x => x.FirstName == person.FirstName).ToList();

        var result = await _repository.FindAllAsync("p", $"p.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ReturnAllMatches_When_ExpressionWithoutParameterName(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var expected = _persons.Where(x => x.FirstName == person.FirstName).ToList();

        var result = await _repository.FindAllAsync($"x.FirstName == \"{person.FirstName}\"", cancellationToken);

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_FindAllExpressionInvalid(CancellationToken cancellationToken)
    {
        await Assert.That(async () => await _repository.FindAllAsync("x.FirstName", cancellationToken))
            .Throws<InvalidOperationException>();
    }

    #endregion

    #region GetPageAsync

    [Test]
    public async Task Should_ReturnFilteredPage_When_ParameterNameInPageQuery(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var list = _persons.Where(x => x.FirstName == person.FirstName).ToList();
        var totalPages = (int)Math.Ceiling((double)list.Count / 10);
        var pageRequest = new PageQuery<Person>(1, 10)
            .Where("p", $"p.FirstName == \"{person.FirstName}\"");

        var result = await _repository.GetPageAsync(pageRequest, cancellationToken);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.TotalPages).IsEqualTo(totalPages);
        await Assert.That(result.TotalItems).IsEqualTo(list.Count);
        await Assert.That(result.Items).IsNotNull();
        await Assert.That(result.Items).IsNotEmpty();
    }

    [Test]
    public async Task Should_ReturnFilteredPage_When_DefaultParameterNameInPageQuery(CancellationToken cancellationToken)
    {
        var person = RandomPerson();
        var list = _persons.Where(x => x.FirstName == person.FirstName).ToList();
        var totalPages = (int)Math.Ceiling((double)list.Count / 10);
        var pageRequest = new PageQuery<Person>(1, 10)
            .Where($"x.FirstName == \"{person.FirstName}\"");

        var result = await _repository.GetPageAsync(pageRequest, cancellationToken);

        await Assert.That(result).IsNotNull();
        await Assert.That(result.TotalPages).IsEqualTo(totalPages);
        await Assert.That(result.TotalItems).IsEqualTo(list.Count);
        await Assert.That(result.Items).IsNotNull();
        await Assert.That(result.Items).IsNotEmpty();
    }

    #endregion
}

