using System.ComponentModel.DataAnnotations;

namespace Deveel {
	public static class OperationResultTests {
		[Fact]
		public static void SuccessOperation() {
			var result = OperationResult.Success;

			Assert.Equal(OperationResultType.Success, result.ResultType);
			Assert.True(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.False(result.IsNotModified());
			Assert.False(result.IsValidationError());
			Assert.Null(result.Error);
		}

		[Fact]
		public static void ErrorOperation() {
			var result = OperationResult.Fail("test", "Test error");

			Assert.Equal(OperationResultType.Error, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.True(result.IsError());
			Assert.False(result.IsNotModified());
			Assert.False(result.IsValidationError());

			Assert.NotNull(result.Error);

			var error = Assert.IsType<OperationError>(result.Error);
			Assert.Equal("test", error.ErrorCode);
			Assert.Equal("Test error", error.Message);
		}

		[Fact]
		public static void ValidationErrorOperation() {
			var result = OperationResult.ValidationFailed("test", new List<ValidationResult> {
				new ValidationResult("Test error", new []{ "Property" })
			});

			Assert.Equal(OperationResultType.Error, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.True(result.IsError());
			Assert.True(result.IsValidationError());
			Assert.False(result.IsNotModified());

			Assert.NotNull(result.Error);

			var error = Assert.IsType<EntityValidationError>(result.Error);
			Assert.Equal("test", error.ErrorCode);
			Assert.Single(error.ValidationResults);
			Assert.Equal("Test error", error.ValidationResults[0].ErrorMessage);
			Assert.Equal("Property", error.ValidationResults[0].MemberNames.First());
		}

		[Fact]
		public static void NotModifiedOperation() {
			var result = OperationResult.NotModified;

			Assert.Equal(OperationResultType.NotModified, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.True(result.IsNotModified());
			Assert.False(result.IsValidationError());
			Assert.Null(result.Error);
		}

		[Fact]
		public static void SuccessOperationWithValue() {
			var result = OperationResult<int>.Success(22);

			Assert.Equal(OperationResultType.Success, result.ResultType);
			Assert.True(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.False(result.IsNotModified());
			Assert.False(result.IsValidationError());
			Assert.Null(result.Error);

			Assert.Equal(22, result.Value);
		}

		[Fact]
		public static void ErrorOperationWithValue() {
			var result = OperationResult<int>.Fail("test", "Test error");

			Assert.Equal(OperationResultType.Error, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.True(result.IsError());
			Assert.False(result.IsNotModified());
			Assert.False(result.IsValidationError());

			Assert.NotNull(result.Error);

			var error = Assert.IsType<OperationError>(result.Error);
			Assert.Equal("test", error.ErrorCode);
			Assert.Equal("Test error", error.Message);

			Assert.Equal(default, result.Value);
		}

		[Fact]
		public static void ValidationErrorOperationWithValue() {
			var result = OperationResult<int>.ValidationFailed("test", new List<ValidationResult> {
				new ValidationResult("Test error", new []{ "Property" })
			});

			Assert.Equal(OperationResultType.Error, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.True(result.IsError());
			Assert.True(result.IsValidationError());
			Assert.False(result.IsNotModified());

			Assert.NotNull(result.Error);

			var error = Assert.IsType<EntityValidationError>(result.Error);
			Assert.Equal("test", error.ErrorCode);
			Assert.Single(error.ValidationResults);
			Assert.Equal("Test error", error.ValidationResults[0].ErrorMessage);
			Assert.Equal("Property", error.ValidationResults[0].MemberNames.First());

			Assert.Equal(default, result.Value);
		}

		[Fact]
		public static void NotModifiedOperationWithValue() {
			var result = OperationResult<int>.NotModified();

			Assert.Equal(OperationResultType.NotModified, result.ResultType);
			Assert.False(result.IsSuccess());
			Assert.False(result.IsError());
			Assert.True(result.IsNotModified());
			Assert.False(result.IsValidationError());
			Assert.Null(result.Error);

			Assert.Equal(default, result.Value);
		}

		[Theory]
		[InlineData(OperationResultType.Success, "success invoked")]
		[InlineData(OperationResultType.Error, "failed invoked")]
		[InlineData(OperationResultType.NotModified, "not modified invoked")]
		public static async Task MapAsync_NoArgs(OperationResultType resultType, string expected) {
			var result = new OperationResult(resultType);

			string? invoked = null;
			await result.MapAsync(
				ifSuccess: () => {
					invoked = "success invoked";
					return Task.CompletedTask;
			},
				ifFailed: () => {
					invoked = "failed invoked";
					return Task.CompletedTask;
				},
				ifNotModified: () => {
					invoked = "not modified invoked";
					return Task.CompletedTask;
				});

			Assert.Equal(expected, invoked);
		}

		[Theory]
		[InlineData(OperationResultType.Success, "success invoked")]
		[InlineData(OperationResultType.Error, "failed invoked")]
		[InlineData(OperationResultType.NotModified, "not modified invoked")]
		public static async Task MapAsync_WithArgs(OperationResultType resultType, string expected) {
			var result = new OperationResult(resultType);

			string? invoked = null;
			await result.MapAsync(
				ifSuccess: success => {
					invoked = "success invoked";
					return Task.CompletedTask;
				},
				ifFailed: failed => {
					invoked = "failed invoked";
					return Task.CompletedTask;
				},
				ifNotModified: notModified => {
					invoked = "not modified invoked";
					return Task.CompletedTask;
				});

			Assert.Equal(expected, invoked);
		}

		[Fact]
		public static void ImplicitConvertResultValue_Success() {
			var value = 22;
			OperationResult<int> result = value;

			Assert.Equal(OperationResultType.Success, result.ResultType);
			Assert.Equal(value, result.Value);
		}

		[Fact]
		public static async Task MapValueAsync() {
			var result = OperationResult<string>.Success("test");

			var mapped = await result.MapAsync(x => Task.FromResult<string?>($"{x} mapped"));

			Assert.Equal("test mapped", mapped);
		}

		[Fact]
		public static async Task MapValueAsync_Success() {
			var result = OperationResult<string>.Success("test");

			var mapped = await result.MapAsync(
				ifSuccess: x => Task.FromResult<string?>($"{x} mapped"),
				ifFailed: x => Task.FromResult<string?>($"{x} failed"),
				ifNotModified: x => Task.FromResult<string?>($"{x} not modified"));

			Assert.Equal("test mapped", mapped);
		}

		[Fact]
		public static async Task MapValueAsync_Failed() {
			var result = OperationResult<string>.Fail("test", "Test error");

			var mapped = await result.MapAsync(
				ifSuccess: x => Task.FromResult<string?>($"{x} mapped"),
				ifFailed: x => Task.FromResult<string?>("failed"),
				ifNotModified: x => Task.FromResult<string?>($"{x} not modified"));

			Assert.Equal("failed", mapped);
		}

		[Fact]
		public static async Task MapValueAsync_NotModified() {
			var result = OperationResult<string>.NotModified("test");

			var mapped = await result.MapAsync(
				ifSuccess: x => Task.FromResult<string?>($"{x} mapped"),
				ifFailed: x => Task.FromResult<string?>($"{x} failed"),
				ifNotModified: x => Task.FromResult<string?>($"{x} not modified"));

			Assert.Equal("test not modified", mapped);
		}
	}
}
