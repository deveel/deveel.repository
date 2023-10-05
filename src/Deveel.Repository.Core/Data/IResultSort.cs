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

using System;

namespace Deveel.Data {
	/// <summary>
	/// Describes a sorting rule for the results of a query
	/// </summary>
	/// <remarks>
	/// Implementations of repositories can use this interface
	/// to form queries to the underlying data store, or
	/// rather to sort the results of a query after the execution,
	/// depending on the nature of the data and the implementation.
	/// </remarks>
	public interface IResultSort {
		/// <summary>
		/// Gets a reference to the field used to sort
		/// the results
		/// </summary>
		IFieldRef Field { get; }

		/// <summary>
		/// Gets a flag indicating whether the result
		/// of the query should be sorted ascending, given
		/// the value of the field
		/// </summary>
		bool Ascending { get; }
	}
}