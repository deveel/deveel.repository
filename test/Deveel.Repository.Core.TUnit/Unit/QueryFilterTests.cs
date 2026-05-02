namespace Deveel.Data;

[Category("Unit")]
public class QueryFilterTests
{
    private class Company
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    private class Employee : Person
    {
        public string CompanyName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
    }

    #region AsLambda

    [Test]
    public async Task Should_ProduceLambdaExpression_When_ConvertingExpressionFilter()
    {
        // Arrange
        var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

        // Act
        var lambda = expr.AsLambda<Person>();

        // Assert
        await Assert.That(lambda).IsNotNull();
        await Assert.That(lambda.ToString()).IsEqualTo("x => (x.FirstName == \"John\")");
    }

    [Test]
    public async Task Should_ProduceTrueLambda_When_ConvertingEmptyFilter()
    {
        // Arrange & Act
        var lambda = QueryFilter.Empty.AsLambda<Person>();

        // Assert
        await Assert.That(lambda).IsNotNull();
        await Assert.That(lambda.ToString()).IsEqualTo("x => True");
    }

    [Test]
    public async Task Should_ThrowArgumentException_When_ConvertingFilterToIncompatibleTargetType()
    {
        // Arrange
        var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

        // Act & Assert
        await Assert.That(() => expr.AsLambda<Company>())
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Should_ProduceCompatibleLambda_When_TargetIsSubtypeOfFilterType()
    {
        // Arrange
        var expr = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

        // Act
        var result = expr.AsLambda<Employee>();

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.ToString()).IsEqualTo("x => (x.FirstName == \"John\")");
    }

    #endregion

    #region IsEmpty

    [Test]
    public async Task Should_ReturnTrue_When_FilterIsEmpty()
    {
        // Arrange & Act & Assert
        await Assert.That(QueryFilter.Empty.IsEmpty()).IsTrue();
    }

    [Test]
    public async Task Should_ReturnFalse_When_FilterHasExpressionCondition()
    {
        // Arrange
        var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

        // Act & Assert
        await Assert.That(filter.IsEmpty()).IsFalse();
    }

    #endregion

    #region LINQ extension methods

    [Test]
    public async Task Should_ReturnMatchingEntity_When_CallingFirstOrDefaultWithFilter()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(10);
        var target = people[Random.Shared.Next(0, people.Count - 1)];
        var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == target.FirstName);

        // Act
        var result = people.AsQueryable().FirstOrDefault(filter);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.FirstName).IsEqualTo(target.FirstName);
    }

    [Test]
    public async Task Should_ReturnMatchingList_When_CallingToListWithFilter()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(100);
        var target = people[Random.Shared.Next(0, people.Count - 1)];
        var expected = people.Where(x => x.FirstName == target.FirstName).ToList();
        var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == target.FirstName);

        // Act
        var result = people.AsQueryable().ToList(filter);

        // Assert
        await Assert.That(result.Count).IsEqualTo(expected.Count);
        await Assert.That(result.First().FirstName).IsEqualTo(expected.First().FirstName);
    }

    [Test]
    public async Task Should_ReturnTrue_When_AnyMatchesFilter()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(100);
        var target = people[Random.Shared.Next(0, people.Count - 1)];
        var filter = new ExpressionQueryFilter<Person>(x => x.FirstName == target.FirstName);

        // Act
        var result = people.AsQueryable().Any(filter);

        // Assert
        await Assert.That(result).IsTrue();
    }

    #endregion
}

