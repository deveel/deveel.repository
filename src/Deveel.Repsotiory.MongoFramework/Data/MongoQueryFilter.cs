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

using MongoDB.Driver;

namespace Deveel.Data {
	/// <summary>
	/// A query filter that is based on a MongoDB filter definition.
	/// </summary>
	/// <typeparam name="TDocument">
	/// The type of document that is used to build the filter.
	/// </typeparam>
	public sealed class MongoQueryFilter<TDocument> : IQueryFilter where TDocument : class {
		/// <summary>
		/// Constructs the filter with the given MongoDB filter definition.
		/// </summary>
		/// <param name="filter">
		/// The instance of <see cref="FilterDefinition{TDocument}"/> that
		/// represents the filter to apply.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given <paramref name="filter"/> is <c>null</c>.
		/// </exception>
		public MongoQueryFilter(FilterDefinition<TDocument> filter) {
			Filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		/// <summary>
		/// Gets the MongoDB filter definition that is used to build the filter.
		/// </summary>
		public FilterDefinition<TDocument> Filter { get; }
	}
}