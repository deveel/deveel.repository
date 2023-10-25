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

namespace Deveel {
	/// <summary>
	/// Extensions for <see cref="IOperationResult"/> that provide
	/// more convenient methods to check the result of an operation.
	/// </summary>
	public static class OperationResultExtensions {
		/// <summary>
		/// Checks if the given result is a successful operation.
		/// </summary>
		/// <param name="result">
		/// The result object to check.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given <paramref name="result"/>
		/// represents a successful operation, otherwise <c>false</c>.
		/// </returns>
		public static bool IsSuccess(this IOperationResult result)
			=> result.ResultType == OperationResultType.Success;

		/// <summary>
		/// Checks if the given result is a failed operation.
		/// </summary>
		/// <param name="result">
		/// The result object to check.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given <paramref name="result"/>
		/// represents a failed operation, otherwise <c>false</c>.
		/// </returns>
		public static bool IsError(this IOperationResult result)
			=> result.ResultType == OperationResultType.Error;

		/// <summary>
		/// Checks if the given result represents an operation that
		/// caused no changes to the entity.
		/// </summary>
		/// <param name="result">
		/// The result object to check.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given <paramref name="result"/>
		/// represents an operation that caused no changes to the entity,
		/// otherwise <c>false</c>.
		/// </returns>
		public static bool IsNotModified(this IOperationResult result)
			=> result.ResultType == OperationResultType.NotModified;

		/// <summary>
		/// Checks if the given result represents a validation error.
		/// </summary>
		/// <param name="result">
		/// The result object to check.
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the given <paramref name="result"/>
		/// represents a validation error, otherwise <c>false</c>.
		/// </returns>
		/// <seealso cref="IsError(IOperationResult)"/>
		public static bool IsValidationError(this IOperationResult result)
			=> result.IsError() && result.Error is IValidationError;

        /// <summary>
        /// Handles the given result by executing the given action
        /// </summary>
        /// <param name="result">
        /// The result object to handle.
        /// </param>
        /// <param name="action">
        /// An action to execute to handle the result.
        /// </param>
        /// <returns>
        /// Returns a task that will be completed when the action
        /// is executed.
        /// </returns>
		public static Task HandleAsync(this IOperationResult result, Func<IOperationResult, Task> action)
            => action(result);

        /// <summary>
        /// Handles the given result by executing the given action
        /// for the specific result type.
        /// </summary>
        /// <param name="result">
        /// The result object to handle.
        /// </param>
        /// <param name="ifSuccess">
        /// The action to execute if the result is a successful operation.
        /// </param>
        /// <param name="ifFailed">
        /// The action to execute if the result is a failed operation.
        /// </param>
        /// <param name="ifNotModified">
        /// The action to execute if the result is an operation that
        /// caused no changes to the entity.
        /// </param>
        /// <returns>
        /// Returns a task that will be completed when the action
        /// is executed.
        /// </returns>
		public static Task HandleAsync(this IOperationResult result, Func<Task>? ifSuccess = null, Func<Task>? ifFailed = null, Func<Task>? ifNotModified = null) {
			if (result.IsSuccess()) {
				if (ifSuccess != null)
					return ifSuccess();
			} else if (result.IsNotModified()) {
				if (ifNotModified != null)
					return ifNotModified();
			} else {
				if (ifFailed != null)
					return ifFailed();
			}

			return Task.CompletedTask;
		}
        /// <summary>
        /// Handles the given result by executing the given action
        /// for the specific result type, passing the result object
        /// as argument.
        /// </summary>
        /// <param name="result">
        /// The result object to handle.
        /// </param>
        /// <param name="ifSuccess">
        /// The action to execute if the result is a successful operation.
        /// </param>
        /// <param name="ifFailed">
        /// The action to execute if the result is a failed operation.
        /// </param>
        /// <param name="ifNotModified">
        /// The action to execute if the result is an operation that
        /// caused no changes to the entity.
        /// </param>
        /// <returns>
        /// Returns a task that will be completed when the action
        /// is executed.
        /// </returns>
		public static Task HandleAsync(this IOperationResult result, Func<IOperationResult, Task>? ifSuccess = null, Func<IOperationResult, Task>? ifFailed = null, Func<IOperationResult, Task>? ifNotModified = null) {
			if (result.IsSuccess()) {
				if (ifSuccess != null)
					return ifSuccess(result);
			} else if (result.IsNotModified()) {
				if (ifNotModified != null)
					return ifNotModified(result);
			} else {
				if (ifFailed != null)
					return ifFailed(result);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// If the result is an error, it casts it to
		/// an exception.
		/// </summary>
		/// <param name="result">
		/// The result object to cast to an exception.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="OperationException"/>,
		/// if the result is an error, otherwise <c>null</c>.
		/// </returns>
		public static OperationException? AsException(this IOperationResult result) {
			if (result.ResultType != OperationResultType.Error ||
				result.Error == null)
				return null;
			
			return new OperationException(result.Error.ErrorCode, result.Error.Message);
		}
	}
}
