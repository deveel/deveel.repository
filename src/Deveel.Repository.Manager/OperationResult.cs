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
	/// An implementation of <see cref="IOperationResult"/> that
	/// represents the result of an operation.
	/// </summary>
	public readonly struct OperationResult : IOperationResult {
		/// <summary>
		/// Constructs the result of an operation with the given
		/// type and optionally an error object.
		/// </summary>
		/// <param name="resultType"></param>
		/// <param name="error"></param>
		public OperationResult(OperationResultType resultType, IOperationError? error = null) : this() {
			ResultType = resultType;
			Error = error;
		}

		/// <inheritdoc/>
		public OperationResultType ResultType { get; }

		/// <inheritdoc/>
		public IOperationError? Error { get; }

		/// <summary>
		/// A result that indicates a successful operation.
		/// </summary>
		public static readonly OperationResult Success = new(OperationResultType.Success);

		/// <summary>
		/// A result that indicates an operation that caused no changes
		/// to the entity.
		/// </summary>
		public static readonly OperationResult NotModified = new(OperationResultType.NotModified);

		/// <summary>
		/// Creates a result that indicates a failed operation.
		/// </summary>
		/// <param name="error">
		/// The error that caused the failure.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
		public static OperationResult Fail(IOperationError? error = null)
			=> new(OperationResultType.Error, error);

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
		/// Returns an instance of <see cref="OperationResult"/> that
		/// represents a failed operation.
		/// </returns>
		public static OperationResult Fail(string errorCode, string? message = null)
			=> new(OperationResultType.Error, new OperationError(errorCode, message));

		/// <summary>
		/// Creates a result that indicates a failed operation
		/// caused by a validation error.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the validation error.
		/// </param>
		/// <param name="results"></param>
		/// <returns></returns>
		public static OperationResult ValidationFailed(string errorCode, IList<ValidationResult> results)
			=> Fail(new EntityValidationError(errorCode, results));
	}
}
