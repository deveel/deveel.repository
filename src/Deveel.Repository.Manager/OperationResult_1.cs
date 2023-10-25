// Copyright 2023 Deveel AS
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.ComponentModel.DataAnnotations;

namespace Deveel {
	/// <summary>
	/// Represents the result of an operation that
	/// returns a value.
	/// </summary>
	/// <typeparam name="TValue">
	/// The type of the value returned by the operation.
	/// </typeparam>
	/// <remarks>
	/// The value returned by the operation is available
	/// only when the <see cref="ResultType"/> is not a 
	/// <see cref="OperationResultType.Error"/>.
	/// </remarks>
	public readonly struct OperationResult<TValue> : IOperationResult {
		private OperationResult(OperationResultType resultType, TValue? value, IOperationError? error) {
			ResultType = resultType;
			Value = value;
			Error = error;
		}

		/// <summary>
		/// Constructs the result of an operation with the given
		/// type and the value returned by the operation.
		/// </summary>
		/// <param name="resultType">
		/// The type of the result of the operation.
		/// </param>
		/// <param name="value">
		/// The value returned by the operation.
		/// </param>
		public OperationResult(OperationResultType resultType, TValue? value) : this(resultType, value, null) {
		}

		/// <summary>
		/// Constructs the result of an operation with the given error.
		/// </summary>
		/// <param name="error">
		/// The error that caused the failure.
		/// </param>
		public OperationResult(IOperationError error) : this(OperationResultType.Error, default, error) {
		}

		/// <inheritdoc/>
		public OperationResultType ResultType { get; }

		/// <inheritdoc/>
		public IOperationError? Error { get; }

		/// <summary>
		/// Gets the value returned by the operation.
		/// </summary>
		public TValue? Value { get; }

		/// <summary>
		/// Maps a result to a new value asynchronously.
		/// </summary>
		/// <param name="mapper">
		/// The function to use to map the value.
		/// </param>
		/// <returns>
		/// Returns a value that is the result of the mapping
		/// from this result.
		/// </returns>
		public Task<TValue?> MapAsync(Func<TValue?, Task<TValue?>> mapper) => mapper(Value);

		/// <summary>
		/// Maps a result to a new value.
		/// </summary>
		/// <param name="mapper">
		/// The function to use to map the value.
		/// </param>
		/// <returns>
		/// Returns a value that is the result of the mapping
		/// from this result.
		/// </returns>
		public TValue? Map(Func<TValue?, TValue?> mapper) => mapper(Value);

		/// <summary>
		/// Handles the result of the operation asynchronously.
		/// </summary>
		/// <param name="handler">
		/// The function used to handle the result value.
		/// </param>
		/// <returns>
		/// Returns a task that will handle the result value.
		/// </returns>
		public Task HandleAsync(Func<TValue?, Task> handler) => handler(Value);

		/// <summary>
		/// Handles the result of the operation.
		/// </summary>
		/// <param name="handler">
		/// The function used to handle the result value.
		/// </param>
		public void Handle(Action<TValue?> handler) => handler(Value);

		/// <summary>
		/// Maps the result of the operation to a new value,
		/// using the provided functions to handle the result.
		/// </summary>
		/// <param name="ifSuccess">
		/// The function to use to map the value if the operation
		/// was successful.
		/// </param>
		/// <param name="ifFailed">
		/// The function to use to map the value if the operation failed.
		/// </param>
		/// <param name="ifNotModified">
		/// The function to use to map the value if the operation
		/// caused no changes to the entity.
		/// </param>
		/// <returns></returns>
		public Task<TValue?> MapAsync(Func<TValue?, Task<TValue?>>? ifSuccess = null, Func<TValue?, Task<TValue?>>? ifFailed = null, Func<TValue?, Task<TValue?>>? ifNotModified = null) {
			return ResultType switch {
				OperationResultType.Success => ifSuccess?.Invoke(Value) ?? Task.FromResult(Value),
				OperationResultType.NotModified => ifNotModified?.Invoke(Value) ?? Task.FromResult(Value),
				_ => ifFailed?.Invoke(Value) ?? Task.FromResult(Value)
			};
		}

		/// <summary>
		/// Implicitly converts the given <paramref name="value"/>
		/// to an instance of <see cref="OperationResult{TValue}"/>
		/// that represents a successful operation.
		/// </summary>
		/// <param name="value">
		/// The value returned by the operation.
		/// </param>
		public static implicit operator OperationResult<TValue>(TValue value)
			=> new(OperationResultType.Success, value);

		/// <summary>
		/// Implicitly converts the given <paramref name="result"/>
		/// to an instance of <see cref="OperationResult{TValue}"/>
		/// that has no value.
		/// </summary>
		/// <param name="result">
		/// The result of the operation.
		/// </param>
		public static implicit operator OperationResult<TValue>(OperationResult result)
			=> new(result.ResultType, default, result.Error);

		/// <summary>
		/// Implicitly converts the given <paramref name="result"/>
		/// to the value returned by the operation.
		/// </summary>
		/// <param name="result">
		/// The operation result that is to be converted.
		/// </param>
		public static implicit operator TValue?(OperationResult<TValue> result) {
			if (result.ResultType == OperationResultType.Error)
				throw result.AsException()!;
				
			return result.Value;
		}

		/// <summary>
		/// Creates a result that indicates a successful operation.
		/// </summary>
		/// <param name="value">
		/// The value returned by the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult{TValue}"/> that
		/// represents a successful operation.
		/// </returns>
		public static OperationResult<TValue> Success(TValue? value = default)
			=> new(OperationResultType.Success, value);

		/// <summary>
		/// Creates a result that indicates a failed operation.
		/// </summary>
		/// <param name="error">
		/// An instance of <see cref="IOperationError"/> that
		/// describes the failure.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult{TValue}"/> that
		/// represents a failed operation.
		/// </returns>
		public static OperationResult<TValue> Fail(IOperationError? error)
			=> new(OperationResultType.Error, default, error);

		/// <summary>
		/// Creates a result that indicates a failed operation.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the error.
		/// </param>
		/// <param name="message">
		/// A message that describes the error.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult{TValue}"/> that
		/// represents a failed operation.
		/// </returns>
		public static OperationResult<TValue> Fail(string errorCode, string? message = null)
			=> new(OperationResultType.Error, default, new OperationError(errorCode, message));

		/// <summary>
		/// Creates a result that indicates a failed operation
		/// caused by a validation error.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the validation error.
		/// </param>
		/// <param name="results">
		/// A list of <see cref="ValidationResult"/> that describe
		/// the validation errors.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult{TValue}"/> that
		/// represents a failed operation caused by a validation error.
		/// </returns>
		public static OperationResult<TValue> ValidationFailed(string errorCode, IList<ValidationResult> results)
			=> Fail(new EntityValidationError(errorCode, results));

		/// <summary>
		/// Creates a result that indicates an operation that 
		/// caused no changes to the entity.
		/// </summary>
		/// <param name="value">
		/// The value returned by the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult{TValue}"/> that
		/// represents an operation that caused no changes to the entity.
		/// </returns>
		public static OperationResult<TValue> NotModified(TValue? value = default)
			=> new(OperationResultType.NotModified, value);
	}
}
