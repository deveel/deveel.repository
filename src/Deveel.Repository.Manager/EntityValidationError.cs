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
	/// An implementation of <see cref="IOperationError"/> that
	/// describes a validation error.
	/// </summary>
	public sealed class EntityValidationError : IValidationError {
		/// <summary>
		/// Constructs the error with the given error code and
		/// the list of validation results.
		/// </summary>
		/// <param name="errorCode">
		/// The error code of the validation.
		/// </param>
		/// <param name="results">
		/// The list of validation results.
		/// </param>
		public EntityValidationError(string errorCode, IEnumerable<ValidationResult>? results = null) {
			ArgumentNullException.ThrowIfNull(errorCode, nameof(errorCode));

			ErrorCode = errorCode;
			ValidationResults = results?.ToList() ?? new List<ValidationResult>();
		}

		/// <inheritdoc/>
		public string ErrorCode { get; }

		string? IOperationError.Message { get; }

		/// <inheritdoc/>
		public IReadOnlyList<ValidationResult> ValidationResults { get; }
	}
}
