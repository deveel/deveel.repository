namespace Deveel.Data;

[Category("Unit")]
public class PageQueryTests
{
    #region PageQuery creation

    [Test]
    public async Task Should_HaveDefaultEmptyState_When_CreatedWithPageAndSize()
    {
        // Arrange & Act
        var query = new PageQuery<Person>(1, 10);

        // Assert
        await Assert.That(query.Page).IsEqualTo(1);
        await Assert.That(query.Size).IsEqualTo(10);
        await Assert.That(query.Offset).IsEqualTo(0);
        await Assert.That(query.Query).IsNotNull();
        await Assert.That(query.Query).IsEqualTo(Query.Empty);
        await Assert.That(query.Query.Filter).IsNotNull();
        await Assert.That(query.Query.Filter).IsEqualTo(QueryFilter.Empty);
    }

    [Test]
    public async Task Should_HaveEmptyFilterAndNoOrder_When_AssignedEmptyQuery()
    {
        // Arrange & Act
        var query = new PageQuery<Person>(1, 10) { Query = Query.Empty };

        // Assert
        await Assert.That(query.Query.Filter).IsEqualTo(QueryFilter.Empty);
        await Assert.That(query.Query.Filter.IsEmpty()).IsTrue();
        await Assert.That(query.Query.Order).IsNull();
    }

    [Test]
    public async Task Should_HaveExpressionFilter_When_WhereClauseIsApplied()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new PageQuery<Person>(1, 10).Where(x => x.FirstName == person.FirstName);

        // Assert
        await Assert.That(query.Query.HasFilter()).IsTrue();
        await Assert.That(query.Query.Filter.IsEmpty()).IsFalse();
        await Assert.That(query.Query.Filter).IsTypeOf<ExpressionQueryFilter<Person>>();
        await Assert.That(query.Query.Order).IsNull();
    }

    [Test]
    public async Task Should_HaveCombinedFilter_When_MultipleWhereClausesAreApplied()
    {
        // Arrange
        var person = PersonFixture.PersonFaker.Generate();

        // Act
        var query = new PageQuery<Person>(1, 10)
            .Where(x => x.FirstName == person.FirstName)
            .Where(x => x.LastName == person.LastName);

        // Assert
        await Assert.That(query.Query.Filter).IsTypeOf<CombinedQueryFilter>();
        var filter = (CombinedQueryFilter)query.Query.Filter!;
        await Assert.That(filter.Count()).IsEqualTo(2);
    }

    [Test]
    public async Task Should_HaveExpressionSortAscending_When_OrderByExpressionIsApplied()
    {
        // Arrange & Act
        var query = new PageQuery<Person>(1, 10).OrderBy(x => x.FirstName);

        // Assert
        await Assert.That(query.Query.HasFilter()).IsFalse();
        await Assert.That(query.Query.Order).IsTypeOf<ExpressionSort<Person>>();
        var sort = (ExpressionSort<Person>)query.Query.Order!;
        await Assert.That(sort.Field.ToString()).IsEqualTo("x => x.FirstName");
    }

    [Test]
    public async Task Should_HaveCombinedSort_When_MultipleOrderByClausesAreApplied()
    {
        // Arrange & Act
        var query = new PageQuery<Person>(1, 10)
            .OrderBy(x => x.FirstName)
            .OrderByDescending(x => x.LastName);

        // Assert
        await Assert.That(query.Query.Order).IsTypeOf<CombinedOrder>();
        var combined = (CombinedOrder)query.Query.Order!;
        await Assert.That(combined.Count()).IsEqualTo(2);
        await Assert.That(combined.ElementAt(0)).IsTypeOf<ExpressionSort<Person>>();
        var first = (ExpressionSort<Person>)combined.ElementAt(0);
        await Assert.That(first.IsAscending()).IsTrue();
        await Assert.That(combined.ElementAt(1)).IsTypeOf<ExpressionSort<Person>>();
        var second = (ExpressionSort<Person>)combined.ElementAt(1);
        await Assert.That(second.IsAscending()).IsFalse();
    }

    [Test]
    public async Task Should_HaveFieldSort_When_OrderByFieldNameIsApplied()
    {
        // Arrange & Act
        var query = new PageQuery<Person>(1, 10).OrderBy("FirstName");

        // Assert
        await Assert.That(query.Query.Order).IsTypeOf<FieldOrder>();
        var sort = (FieldOrder)query.Query.Order!;
        await Assert.That(sort.FieldName).IsEqualTo("FirstName");
        await Assert.That(sort.IsAscending()).IsTrue();
    }

    [Test]
    public async Task Should_SupportMixedExpressionAndFieldSort_When_BothAreApplied()
    {
        // Arrange & Act — verifies no exception is thrown
        var query = new PageQuery<Person>(1, 10)
            .OrderBy(x => x.FirstName)
            .OrderByDescending("LastName");

        // Assert
        await Assert.That(query.Query.Order).IsNotNull();
    }

    #endregion
}

