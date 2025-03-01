namespace Deveel.Data {
	public static class OperationErrorFactoryTests {
		[Fact]
		public static void CreateError() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(EntityErrorCodes.NotFound, "test");

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.Code);
			Assert.Equal("test", error.Domain);
			Assert.Null(error.Message);
		}

		[Fact]
		public static void CreateError_WithMessage() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(EntityErrorCodes.NotFound, "test", "The entity was not found");

			Assert.NotNull(error);
			Assert.Equal("test", error.Domain);	
			Assert.Equal(EntityErrorCodes.NotFound, error.Code);
			Assert.Equal("The entity was not found", error.Message);
		}

		[Fact]
		public static void CreateError_Exception() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new Exception("Something went wrong"));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.UnknownError, error.Code);
			Assert.Equal(EntityErrorCodes.UnknownDomain, error.Domain);
			Assert.Equal("Something went wrong", error.Message);
		}

		[Fact]
		public static void CreateError_OperationException() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new OperationException(EntityErrorCodes.NotFound, "test"));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.Code);
			Assert.NotNull(error.Message);
		}

		[Fact]
		public static void CreateError_OperationException_WithMessage() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new OperationException(EntityErrorCodes.NotFound, "test", "The entity was not found"));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.Code);
			Assert.Equal("The entity was not found", error.Message);
		}
	}
}
