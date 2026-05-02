namespace Deveel.Data;

[Category("Unit")]
public class CombinedFilterTests
{
    private static readonly Bogus.Faker<Person> PersonFaker = PersonFixture.PersonFaker;

    #region QueryFilter.Combine

    [Test]
    public async Task Should_FlattenFilters_When_CombiningAlreadyCombinedFilter()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
        var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");
        var combined1 = QueryFilter.Combine(filter1, filter2);

        // Act
        var combined2 = combined1.Combine(QueryFilter.Empty);

        // Assert
        await Assert.That(combined2).IsTypeOf<CombinedQueryFilter>();
        var result = (CombinedQueryFilter)combined2!;
        await Assert.That(result.Count()).IsEqualTo(3);
        await Assert.That(result.ElementAt(0)).IsEqualTo(filter1);
        await Assert.That(result.ElementAt(1)).IsEqualTo(filter2);
        await Assert.That(result.ElementAt(2)).IsEqualTo(QueryFilter.Empty);
    }

    [Test]
    public async Task Should_ReturnCombinedFilter_When_TwoExpressionFiltersAreCombined()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
        var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");

        // Act
        var combined = QueryFilter.Combine(filter1, filter2);

        // Assert
        await Assert.That(combined).IsTypeOf<CombinedQueryFilter>();
        var result = (CombinedQueryFilter)combined!;
        await Assert.That(result.Count()).IsEqualTo(2);
        await Assert.That(result.ElementAt(0)).IsEqualTo(filter1);
        await Assert.That(result.ElementAt(1)).IsEqualTo(filter2);
    }

    [Test]
    public async Task Should_ExcludeEmptyFilter_When_CombiningWithEmpty()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");

        // Act
        var combined = QueryFilter.Combine(filter1, QueryFilter.Empty);

        // Assert
        await Assert.That(combined).IsTypeOf<CombinedQueryFilter>();
        var result = (CombinedQueryFilter)combined!;
        await Assert.That(result.Count()).IsEqualTo(1);
        await Assert.That(result.ElementAt(0)).IsEqualTo(filter1);
    }

    [Test]
    public async Task Should_ThrowArgumentException_When_AllFiltersAreEmpty()
    {
        // Arrange & Act & Assert
        await Assert.That(() => QueryFilter.Combine(QueryFilter.Empty, QueryFilter.Empty))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Should_ProduceAndAlsoLambda_When_ConvertingCombinedFilter()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
        var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");
        var combined = QueryFilter.Combine(filter1, filter2);

        // Act
        var lambda = combined.AsLambda<Person>();

        // Assert
        await Assert.That(lambda).IsNotNull();
        await Assert.That(lambda.ToString()).IsEqualTo("x => ((x.FirstName == \"John\") AndAlso (x.LastName == \"Doe\"))");
    }

    [Test]
    public async Task Should_ProduceSingleConditionLambda_When_CombinedFilterHasOnlyOneNonEmptyFilter()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
        var combined = QueryFilter.Combine(filter1, QueryFilter.Empty);

        // Act
        var lambda = combined.AsLambda<Person>();

        // Assert
        await Assert.That(lambda).IsNotNull();
        await Assert.That(lambda.ToString()).IsEqualTo("x => (x.FirstName == \"John\")");
    }

    [Test]
    public async Task Should_ProduceTripleAndAlsoLambda_When_ThreeFiltersAreCombined()
    {
        // Arrange
        var filter1 = new ExpressionQueryFilter<Person>(x => x.FirstName == "John");
        var filter2 = new ExpressionQueryFilter<Person>(x => x.LastName == "Doe");
        var filter3 = new ExpressionQueryFilter<Person>(x => x.Email == "john.doe@example.com");

        // Act
        var combined = QueryFilter.Combine(filter1, filter2, filter3);
        var lambda = combined.AsLambda<Person>();

        // Assert
        await Assert.That(lambda).IsNotNull();
        await Assert.That(lambda.ToString()).IsEqualTo(
            "x => (((x.FirstName == \"John\") AndAlso (x.LastName == \"Doe\")) AndAlso (x.Email == \"john.doe@example.com\"))");
    }

    #endregion
}

