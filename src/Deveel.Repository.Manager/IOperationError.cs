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
	/// An object that describes an error that occurred
	/// during an operation.
	/// </summary>
	/// <remarks>
	/// The objective of this contract is not to provide a
	/// full stack trace of the error, but rather a simple
	/// descriptor that can be used to inform the user of
	/// the kind of error that occurred.
	/// </remarks>
	public interface IOperationError {
		/// <summary>
		/// Gets the code that identifies the class of the error.
		/// </summary>
		string ErrorCode { get; }

		/// <summary>
		/// Gets a message that describes the error.
		/// </summary>
		string? Message { get; }
	}
}
