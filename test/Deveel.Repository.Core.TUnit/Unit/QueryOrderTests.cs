namespace Deveel.Data;

[Category("Unit")]
public class QueryOrderTests
{
    #region QueryOrder.Combine

    [Test]
    public async Task Should_ReturnCombinedOrder_When_TwoSortsAreCombined()
    {
        // Arrange
        var sort1 = QueryOrder.OrderBy<Person>(x => x.FirstName);
        var sort2 = QueryOrder.OrderByDescending<Person>(x => x.LastName);

        // Act
        var combined = QueryOrder.Combine(sort1, sort2);

        // Assert
        await Assert.That(combined).IsTypeOf<CombinedOrder>();
        var result = (CombinedOrder)combined!;
        await Assert.That(result.Count()).IsEqualTo(2);
        await Assert.That(result.ElementAt(0)).IsEqualTo(sort1);
        await Assert.That(result.ElementAt(0).IsAscending()).IsTrue();
        await Assert.That(result.ElementAt(1)).IsEqualTo(sort2);
        await Assert.That(result.ElementAt(1).IsAscending()).IsFalse();
        await Assert.That(result.ElementAt(1).IsDescending()).IsTrue();
    }

    [Test]
    public async Task Should_FlattenSorts_When_CombiningWithAlreadyCombinedOrder()
    {
        // Arrange
        var sort1 = QueryOrder.OrderBy<Person>(x => x.FirstName);
        var sort2 = QueryOrder.OrderByDescending<Person>(x => x.LastName);
        var sort3 = QueryOrder.OrderBy<Person>(x => x.DateOfBirth);
        var combined1 = QueryOrder.Combine(sort1, sort2);

        // Act
        var combined2 = combined1.Combine(sort3);

        // Assert
        await Assert.That(combined2).IsNotNull();
    }

    [Test]
    public async Task Should_ThrowArgumentException_When_CreatingCombinedOrderWithEmptyList()
    {
        // Arrange & Act & Assert
        await Assert.That(() => new CombinedOrder(Array.Empty<IQueryOrder>()))
            .Throws<ArgumentException>();
    }

    #endregion

    #region Apply

    [Test]
    public async Task Should_ReturnSortedQueryable_When_ApplyingCombinedSort()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(4);
        var queryable = people.AsQueryable();
        var sort1 = QueryOrder.OrderBy<Person>(x => x.FirstName);
        var sort2 = QueryOrder.OrderByDescending<Person>(x => x.LastName);
        var combined = QueryOrder.Combine(sort1, sort2);

        // Act
        var result = combined.Apply(queryable);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.ToList().Count).IsEqualTo(4);
    }

    [Test]
    public async Task Should_SortByProperty_When_ApplyingFieldNameSortWithNoMapper()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(4);
        var queryable = people.AsQueryable();
        var sort = QueryOrder.OrderBy("FirstName");

        // Act
        var result = sort.Apply(queryable);

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.ToList().Count).IsEqualTo(4);
    }

    [Test]
    public async Task Should_SortByMappedField_When_ApplyingFieldNameSortWithDelegatedMapper()
    {
        // Arrange
        var people = PersonFixture.PersonFaker.Generate(4);
        var queryable = people.AsQueryable();
        var sort = QueryOrder.OrderBy("first_name");

        // Act
        var result = sort.Apply(queryable, field => field switch
        {
            "first_name" => (System.Linq.Expressions.Expression<Func<Person, object?>>)(p => p.FirstName),
            "last_name" => p => p.LastName,
            _ => throw new ArgumentException($"Unsupported field '{field}'")
        });

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result.ToList().Count).IsEqualTo(4);
    }

    #endregion
}

