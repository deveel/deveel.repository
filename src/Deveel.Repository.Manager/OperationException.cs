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
	/// An exception that is thrown when an operation fails.
	/// </summary>
	/// <remarks>
	/// Throwing exceptions of this type is discouraged, since
	/// it causes memory allocations and it's not a good practice,
	/// but if the operation is not expected to fail, this can be
	/// a provisional solution to expose the <see cref="IOperationError"/>
	/// to an operation result.
	/// </remarks>
	/// <seealso cref="IOperationError"/>
	public class OperationException : Exception, IOperationError {
		/// <summary>
		/// Constructs the exception with the given error code.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the error.
		/// </param>
		public OperationException(string errorCode) {
			ErrorCode = errorCode;
		}

		/// <summary>
		/// Constructs the exception with the given error code and message.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the error.
		/// </param>
		/// <param name="message">
		/// A message that describes the error.
		/// </param>
		public OperationException(string errorCode, string? message) : base(message) {
			ErrorCode = errorCode;
		}

		/// <summary>
		/// Constructs the exception with the given error code, message
		/// and the inner exception that caused the error.
		/// </summary>
		/// <param name="errorCode">
		/// The code that identifies the class of the error.
		/// </param>
		/// <param name="message">
		/// A message that describes the error.
		/// </param>
		/// <param name="innerException">
		/// An exception that caused the error.
		/// </param>
		public OperationException(string errorCode, string? message, Exception? innerException) : base(message, innerException) {
			ErrorCode = errorCode;
		}

		/// <inheritdoc />
		public string ErrorCode { get; }
	}
}
