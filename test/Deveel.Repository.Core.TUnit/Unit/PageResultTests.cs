namespace Deveel.Data;

[Category("Unit")]
public class PageResultTests
{
    #region PageResult construction

    [Test]
    public async Task Should_HaveCorrectPaginationState_When_OnFirstPageOfManyPages()
    {
        // Arrange
        var query = new PageQuery<Person>(1, 10);
        var items = PersonFixture.PersonFaker.Generate(10);

        // Act
        var result = new PageResult<Person>(query, 100, items);

        // Assert
        await Assert.That(result.TotalItems).IsEqualTo(100);
        await Assert.That(result.TotalPages).IsEqualTo(10);
        await Assert.That(result.Request.Page).IsEqualTo(1);
        await Assert.That(result.Request.Size).IsEqualTo(10);
        await Assert.That(result.Items.Count).IsEqualTo(10);
        await Assert.That(result.IsFirstPage).IsTrue();
        await Assert.That(result.IsLastPage).IsFalse();
        await Assert.That(result.HasNextPage).IsTrue();
        await Assert.That(result.HasPreviousPage).IsFalse();
        await Assert.That(result.NextPage).IsEqualTo(2);
        await Assert.That(result.PreviousPage).IsNull();
    }

    [Test]
    public async Task Should_HaveCorrectPaginationState_When_OnLastPageOfManyPages()
    {
        // Arrange
        var query = new PageQuery<Person>(10, 10);
        var items = PersonFixture.PersonFaker.Generate(8);

        // Act
        var result = new PageResult<Person>(query, 100, items);

        // Assert
        await Assert.That(result.TotalItems).IsEqualTo(100);
        await Assert.That(result.TotalPages).IsEqualTo(10);
        await Assert.That(result.Request.Page).IsEqualTo(10);
        await Assert.That(result.Items.Count).IsEqualTo(8);
        await Assert.That(result.IsFirstPage).IsFalse();
        await Assert.That(result.IsLastPage).IsTrue();
        await Assert.That(result.HasNextPage).IsFalse();
        await Assert.That(result.HasPreviousPage).IsTrue();
        await Assert.That(result.NextPage).IsNull();
        await Assert.That(result.PreviousPage).IsEqualTo(9);
    }

    [Test]
    public async Task Should_ThrowArgumentOutOfRangeException_When_TotalItemsIsNegative()
    {
        // Arrange
        var query = new PageQuery<Person>(1, 10);

        // Act & Assert
        await Assert.That(() => new PageResult<Person>(query, -1))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Should_ThrowArgumentNullException_When_RequestIsNull()
    {
        // Arrange & Act & Assert
        await Assert.That(() => new PageResult<Person>(null!, 0))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Should_ReturnZeroItemsAndPages_When_ResultIsEmpty()
    {
        // Arrange
        var query = new PageQuery<Person>(1, 10);

        // Act
        var result = PageResult<Person>.Empty(query);

        // Assert
        await Assert.That(result.TotalItems).IsEqualTo(0);
        await Assert.That(result.TotalPages).IsEqualTo(0);
        await Assert.That(result.Request.Page).IsEqualTo(1);
        await Assert.That(result.Request.Size).IsEqualTo(10);
    }

    #endregion
}

