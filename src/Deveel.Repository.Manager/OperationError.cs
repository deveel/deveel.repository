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
	/// An implementation of <see cref="IOperationError"/> that
	/// describes an error in an operation.
	/// </summary>
	public readonly struct OperationError : IOperationError {
		/// <summary>
		/// Constructs an error with the given error code and message.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the error.
		/// </param>
		/// <param name="message">
		/// A message that describes the error.
		/// </param>
		public OperationError(string errorCode, string? message = null) : this() {
			ErrorCode = errorCode;
			Message = message;
		}

		/// <inheritdoc/>
		public string ErrorCode { get; }

		/// <inheritdoc/>
		public string? Message { get; }
	}
}
