namespace Deveel.Data;

[Category("Unit")]
public class OperationErrorFactoryTests
{
    private readonly OperationErrorFactory _factory = new();

    #region CreateError

    [Test]
    public async Task Should_CreateErrorWithCodeAndDomain_When_MinimalArgsProvided()
    {
        // Act
        var error = _factory.CreateError(EntityErrorCodes.NotFound, "test");

        // Assert
        await Assert.That(error).IsNotNull();
        await Assert.That(error.Code).IsEqualTo(EntityErrorCodes.NotFound);
        await Assert.That(error.Domain).IsEqualTo("test");
        await Assert.That(error.Message).IsNull();
    }

    [Test]
    public async Task Should_CreateErrorWithMessage_When_MessageProvided()
    {
        // Act
        var error = _factory.CreateError(EntityErrorCodes.NotFound, "test", "The entity was not found");

        // Assert
        await Assert.That(error).IsNotNull();
        await Assert.That(error.Domain).IsEqualTo("test");
        await Assert.That(error.Code).IsEqualTo(EntityErrorCodes.NotFound);
        await Assert.That(error.Message).IsEqualTo("The entity was not found");
    }

    [Test]
    public async Task Should_CreateUnknownError_When_GenericExceptionProvided()
    {
        // Arrange
        var exception = new Exception("Something went wrong");

        // Act
        var error = _factory.CreateError(exception);

        // Assert
        await Assert.That(error).IsNotNull();
        await Assert.That(error.Code).IsEqualTo(EntityErrorCodes.UnknownError);
        await Assert.That(error.Domain).IsEqualTo(EntityErrorCodes.UnknownDomain);
        await Assert.That(error.Message).IsEqualTo("Something went wrong");
    }

    [Test]
    public async Task Should_CreateErrorFromOperationException_When_CodeAndDomainSet()
    {
        // Arrange
        var exception = new OperationException(EntityErrorCodes.NotFound, "test");

        // Act
        var error = _factory.CreateError(exception);

        // Assert
        await Assert.That(error).IsNotNull();
        await Assert.That(error.Code).IsEqualTo(EntityErrorCodes.NotFound);
        await Assert.That(error.Message).IsNotNull();
    }

    [Test]
    public async Task Should_CreateErrorWithMessage_When_OperationExceptionHasMessage()
    {
        // Arrange
        var exception = new OperationException(EntityErrorCodes.NotFound, "test", "The entity was not found");

        // Act
        var error = _factory.CreateError(exception);

        // Assert
        await Assert.That(error).IsNotNull();
        await Assert.That(error.Code).IsEqualTo(EntityErrorCodes.NotFound);
        await Assert.That(error.Message).IsEqualTo("The entity was not found");
    }

    #endregion
}

