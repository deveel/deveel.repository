namespace Deveel.Data;

[Category("Unit")]
public class QueryTests
{
    #region Query construction

    [Test]
    public async Task Should_HaveFilterAndNoOrder_When_QueryHasFilterOnly()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new Query(QueryFilter.Where<Person>(p => p.FirstName == person.FirstName));

        // Assert
        await Assert.That(query.Filter).IsNotNull();
        await Assert.That(query.Order).IsNull();
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
    }

    [Test]
    public async Task Should_HaveExpressionFilter_When_BuiltWithWhereFactory()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = Query.Where<Person>(p => p.FirstName == person.FirstName);

        // Assert
        await Assert.That(query.Filter).IsNotNull();
        await Assert.That(query.Order).IsNull();
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
    }

    [Test]
    public async Task Should_HaveCombinedFilter_When_TwoWhereClausesAreChained()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new QueryBuilder<Person>()
            .Where(p => p.FirstName == person.FirstName)
            .Where(p => p.LastName == person.LastName)
            .Query;

        // Assert
        await Assert.That(query.Filter).IsNotNull();
        await Assert.That(query.Order).IsNull();
        await Assert.That(query.Filter).IsTypeOf<CombinedQueryFilter>();
        var combined = (CombinedQueryFilter)query.Filter!;
        await Assert.That(combined.Count()).IsEqualTo(2);
        await Assert.That(combined.ElementAt(0)).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(combined.ElementAt(1)).IsTypeOf<ExpressionQueryFilter<Person>>();
    }

    [Test]
    public async Task Should_HaveFilterAndExpressionSort_When_WhereAndOrderByExpressionAreChained()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new QueryBuilder<Person>()
            .Where(p => p.FirstName == person.FirstName)
            .OrderBy(p => p.LastName)
            .Query;

        // Assert
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(query.Order).IsTypeOf<ExpressionSort<Person>>();
    }

    [Test]
    public async Task Should_HaveFilterAndFieldSort_When_WhereAndOrderByFieldNameAreChained()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new QueryBuilder<Person>()
            .Where(p => p.FirstName == person.FirstName)
            .OrderBy("LastName")
            .Query;

        // Assert
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(query.Order).IsTypeOf<FieldOrder>();
    }

    [Test]
    public async Task Should_HaveDescendingFieldSort_When_WhereAndOrderByDescendingFieldNameAreChained()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new QueryBuilder<Person>()
            .Where(p => p.FirstName == person.FirstName)
            .OrderByDescending("LastName")
            .Query;

        // Assert
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(query.Order).IsTypeOf<FieldOrder>();
        var sort = (FieldOrder)query.Order!;
        await Assert.That(sort.IsDescending()).IsTrue();
    }

    [Test]
    public async Task Should_HaveDescendingExpressionSort_When_WhereAndOrderByDescendingExpressionAreChained()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new QueryBuilder<Person>()
            .Where(p => p.FirstName == person.FirstName)
            .OrderByDescending(p => p.LastName)
            .Query;

        // Assert
        await Assert.That(query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(query.Order).IsTypeOf<ExpressionSort<Person>>();
        var sort = (ExpressionSort<Person>)query.Order!;
        await Assert.That(sort.IsDescending()).IsTrue();
    }

    #endregion

    #region Apply

    [Test]
    public async Task Should_FilterQueryable_When_ApplyingQueryWithWhereOnly()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(10);
        var matches = people.Take(3).ToList();
        var sharedName = Guid.NewGuid().ToString("N")[..8]; // unique surname to avoid collision
        foreach (var p in matches)
            p.LastName = sharedName;

        var query = Query.Where<Person>(p => p.LastName == sharedName);

        // Act
        var result = query.Apply(people.AsQueryable());

        // Assert
        await Assert.That(result.Count()).IsEqualTo(matches.Count);
    }

    [Test]
    public async Task Should_FilterAndSortQueryable_When_ApplyingQueryWithWhereAndOrderBy()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(10);
        var matches = people.Take(3).ToList();
        var sharedName = Guid.NewGuid().ToString("N")[..8];
        foreach (var p in matches)
            p.LastName = sharedName;

        var query = new QueryBuilder<Person>()
            .Where(p => p.LastName == sharedName)
            .OrderBy(p => p.FirstName);

        // Act
        var result = query.Apply(people.AsQueryable());

        // Assert
        await Assert.That(result.Count()).IsEqualTo(matches.Count);
        await Assert.That(result.First().FirstName).IsEqualTo(matches.OrderBy(p => p.FirstName).First().FirstName);
    }

    #endregion
}

