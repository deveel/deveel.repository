namespace Deveel.Data;

[Category("Unit")]
public class DynamicLinqFilterTests
{
    [Test]
    public async Task Should_BuildTypedLambda_When_ValidExpressionProvided()
    {
        // Arrange
        var filter = new DynamicLinqFilter("p", "p.FirstName == \"John\"");

        // Act
        var expression = filter.AsLambda<Person>();

        // Assert
        await Assert.That(filter.ParameterName).IsEqualTo("p");
        await Assert.That(filter.Expression).IsEqualTo("p.FirstName == \"John\"");
        await Assert.That(expression).IsNotNull();
        await Assert.That(expression.Parameters[0].Type).IsEqualTo(typeof(Person));
        await Assert.That(expression.ReturnType).IsEqualTo(typeof(bool));
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_ExpressionDoesNotReturnBool()
    {
        // Arrange
        var filter = new DynamicLinqFilter("p", "p.FirstName");

        // Act & Assert
        await Assert.That(() => filter.AsLambda<Person>()).Throws<InvalidOperationException>();
    }
}

