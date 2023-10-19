﻿// Copyright 2023 Deveel AS
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
	/// A query that can be applied to a repository
	/// to filter and sort the results.
	/// </summary>
	public readonly struct Query {
		/// <summary>
		/// Constructs the query with the given filter and sort.
		/// </summary>
		/// <param name="filter">
		/// The filter to apply to the query.
		/// </param>
		/// <param name="sort">
		/// An optional sort to apply to the query.
		/// </param>
		public Query(IQueryFilter filter, ISort? sort = null) {
			ArgumentNullException.ThrowIfNull(filter, nameof(filter));

			Filter = filter;
			Sort = sort;
		}

		/// <summary>
		/// Gets the filter to apply to the query.
		/// </summary>
		public IQueryFilter Filter { get; }

		/// <summary>
		/// Gets the sort to apply to the results
		/// of the query.
		/// </summary>
		public ISort? Sort { get; }

		/// <summary>
		/// Represents an empty query, that will apply
		/// no filter and no sort.
		/// </summary>
		public static Query Empty => new Query(QueryFilter.Empty);

		/// <summary>
		/// Applies the query to the given <see cref="IQueryable{TEntity}"/>
		/// to filter and sort the results.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to apply the query to.
		/// </typeparam>
		/// <param name="queryable">
		/// The queryable to apply the query to.
		/// </param>
		/// <returns>
		/// Returns the <see cref="IQueryable{TEntity}"/> that was
		/// filtered and sorted.
		/// </returns>
		public IQueryable<TEntity> Apply<TEntity>(IQueryable<TEntity> queryable)
			where TEntity : class {
			ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

			if (Filter != null)
				queryable = Filter.Apply<TEntity>(queryable);

			if (Sort != null)
				queryable = Sort.Apply<TEntity>(queryable);

			return queryable;
		}
	}
}
