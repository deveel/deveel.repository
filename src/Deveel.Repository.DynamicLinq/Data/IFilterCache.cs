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

namespace Deveel.Data {
	/// <summary>
	/// A cache of compiled filter expressions,
	/// that is aimed to be used to avoid re-compilation
	/// </summary>
	public interface IFilterCache {
		/// <summary>
		/// Tries to get a compiled filter expression
		/// from the cache.
		/// </summary>
		/// <param name="expression">
		/// The expression string to be parsed
		/// </param>
		/// <param name="labda">
		/// The compiled lambda expression that was cached,
		/// or <c>null</c> if the expression was not found
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the expression was found
		/// in the cache, otherwise <c>false</c>.
		/// </returns>
		bool TryGet(string expression, out Delegate? labda);

		/// <summary>
		/// Sets a compiled filter expression in the cache
		/// </summary>
		/// <param name="expression">
		/// The expression string to be parsed
		/// </param>
		/// <param name="lambda">
		/// The compiled lambda expression to be cached
		/// </param>
		void Set(string expression, Delegate lambda);
	}
}
