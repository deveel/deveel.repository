namespace Deveel.Data;

[Category("Unit")]
public class FilterExpressionTests
{
    [Test]
    public async Task Should_BuildBoolLambda_When_ValidExpressionProvided()
    {
        // Arrange & Act
        var expression = FilterExpression.AsLambda<Person>("p", "p.FirstName == \"John\"");

        // Assert
        await Assert.That(expression).IsNotNull();
        await Assert.That(expression.Parameters[0].Type).IsEqualTo(typeof(Person));
        await Assert.That(expression.ReturnType).IsEqualTo(typeof(bool));
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_ExpressionDoesNotReturnBool()
    {
        // Act & Assert
        await Assert.That(() => FilterExpression.AsLambda<Person>("p", "p.FirstName"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Should_ThrowInvalidOperationException_When_TypeMismatch()
    {
        // Act & Assert
        await Assert.That(() => FilterExpression.AsLambda<Person>("p", "p.FirstName == 1"))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task Should_CompileToTypedDelegate_When_SingleParameterExpression()
    {
        // Arrange & Act
        var result = FilterExpression.Compile(typeof(Person), "p", "p.FirstName == \"John\"");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<Func<Person, bool>>();
        var func = (Func<Person, bool>)result;
        await Assert.That(func.Method).IsNotNull();
        var pars = func.Method.GetParameters();
        await Assert.That(pars).IsNotEmpty();
        await Assert.That(pars.Length).IsEqualTo(2);
        await Assert.That(pars[1].ParameterType).IsEqualTo(typeof(Person));
        await Assert.That(func.Method.ReturnType).IsEqualTo(typeof(bool));
    }

    [Test]
    public async Task Should_CacheCompileResult_When_CacheProvided()
    {
        // Arrange
        var cache = new SimpleFilterCache();

        // Act
        var result = FilterExpression.Compile(cache, typeof(Person), "p", "p.FirstName == \"John\"");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<Func<Person, bool>>();
        var func = (Func<Person, bool>)result;
        await Assert.That(cache.TryGet("p.FirstName == \"John\"", out var cachedFunc)).IsTrue();
        await Assert.That(cachedFunc).IsNotNull();
        await Assert.That(func).IsSameReferenceAs(cachedFunc);
    }

    [Test]
    public async Task Should_CompileGenericDelegate_When_TypedCompileUsed()
    {
        // Arrange & Act
        var result = FilterExpression.Compile<Person>("p", "p.FirstName == \"John\"");

        // Assert
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsTypeOf<Func<Person, bool>>();
        var func = result;
        await Assert.That(func.Method).IsNotNull();
        await Assert.That(func.Method.ReturnType).IsEqualTo(typeof(bool));
    }

    private sealed class SimpleFilterCache : IFilterCache
    {
        private readonly Dictionary<string, Delegate> _cache = new();

        public void Set(string expression, Delegate func) => _cache[expression] = func;

        public bool TryGet(string expression, out Delegate? lambda) =>
            _cache.TryGetValue(expression, out lambda);
    }
}

