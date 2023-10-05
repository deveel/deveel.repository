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

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
	/// <summary>
	/// Describes a sorting rule using a string filter reference
	/// </summary>
	public sealed class FieldResultSort : IResultSort {
		/// <summary>
		/// Constructs the sorting rule using the given field name
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to sort the results
		/// </param>
		/// <param name="ascending">
		/// Whether the results should be sorted ascending
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if the given <paramref name="fieldName"/> is <c>null</c> or empty.
		/// </exception>
		public FieldResultSort(string fieldName, bool ascending = true) {
			Guard.IsNotNullOrWhiteSpace(fieldName, nameof(fieldName));

			FieldName = fieldName;
			Ascending = ascending;
		}

		IFieldRef IResultSort.Field => new StringFieldRef(FieldName);

		/// <summary>
		/// Gets the name of the field used to sort the results
		/// </summary>
		public string FieldName { get; }


		/// <summary>
		/// Gets a flag indicating whether the results should be
		/// sorted ascending
		/// </summary>
		public bool Ascending { get; }
	}
}
