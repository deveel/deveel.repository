namespace Deveel.Data {
	public static class OperationErrorFactoryTests {
		[Fact]
		public static void CreateError() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(EntityErrorCodes.NotFound);

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.ErrorCode);
			Assert.Null(error.Message);
		}

		[Fact]
		public static void CreateError_WithMessage() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(EntityErrorCodes.NotFound, "The entity was not found");

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.ErrorCode);
			Assert.Equal("The entity was not found", error.Message);
		}

		[Fact]
		public static void CreateError_Exception() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new Exception("Something went wrong"));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.UnknownError, error.ErrorCode);
			Assert.Equal("Something went wrong", error.Message);
		}

		[Fact]
		public static void CreateError_OperationException() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new OperationException(EntityErrorCodes.NotFound));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.ErrorCode);
			Assert.NotNull(error.Message);
		}

		[Fact]
		public static void CreateError_OperationException_WithMessage() {
			var factory = new OperationErrorFactory();

			var error = factory.CreateError(new OperationException(EntityErrorCodes.NotFound, "The entity was not found"));

			Assert.NotNull(error);
			Assert.Equal(EntityErrorCodes.NotFound, error.ErrorCode);
			Assert.Equal("The entity was not found", error.Message);
		}
	}
}
