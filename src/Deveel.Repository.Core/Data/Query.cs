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

using System.Linq.Expressions;

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
		/// Gets a value indicating if the query has a filter.
		/// </summary>
		public bool HasFilter => !(Filter?.IsEmpty() ?? true);

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

		/// <summary>
		/// Combines the current filter with the given one,
		/// using the logical AND operator.
		/// </summary>
		/// <param name="filter">
		/// The filter to combine with the current one.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current filter with the given one.
		/// </returns>
		public Query And(IQueryFilter filter)
			=> new Query(QueryFilter.Combine(Filter, filter), Sort);

		/// <summary>
		/// Combines the current filter with the given one,
		/// using the logical AND operator.
		/// </summary>
		/// <param name="filter">
		/// The filter to combine with the current one.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current filter with the given one.
		/// </returns>
		public Query And<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
			=> new Query(QueryFilter.Combine(Filter, QueryFilter.Where<TEntity>(filter)), Sort);

		/// <summary>
		/// Configures the query to sort by the
		/// given rule.
		/// </summary>
		/// <param name="sort">
		/// The sort rule to apply to the query.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current sort rule with the given one.
		/// </returns>
		public Query OrderBy(ISort sort)
			=> new Query(Filter, CombineSort(Sort, sort));

		/// <summary>
		/// Configures the query to sort by the field
		/// obtained by the given expression.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity that is used to select the
		/// field to sort by.
		/// </typeparam>
		/// <param name="field">
		/// The expression that selects the field to sort by.
		/// </param>
		/// <param name="direction">
		/// The direction of the sort.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current sort rule with the one obtained by the
		/// selection of the given field.
		/// </returns>
		public Query OrderBy<TEntity>(Expression<Func<TEntity, object?>> field, SortDirection direction = SortDirection.Ascending) 
			where TEntity : class
			=> OrderBy(Data.Sort.OrderBy(field, direction));

		/// <summary>
		/// Configures the query to sort by the field
		/// identified by the given name.
		/// </summary>
		/// <param name="field">
		/// The name of the field to sort by.
		/// </param>
		/// <param name="direction">
		/// The direction of the sort.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current sort rule with the one obtained by the
		/// selection of the given field.
		/// </returns>
		public Query OrderBy(string field, SortDirection direction = SortDirection.Ascending) 
			=> OrderBy(Data.Sort.OrderBy(field, direction));

		/// <summary>
		/// Configures the query to sort in a discending order 
		/// by the field obtained by the given expression.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity that is used to select the
		/// field to sort by.
		/// </typeparam>
		/// <param name="field">
		/// The expression that selects the field to sort by.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current sort rule with the one obtained by the
		/// selection of the given field.
		/// </returns>
		public Query OrderByDescending<TEntity>(Expression<Func<TEntity, object?>> field) 
			where TEntity : class
			=> OrderBy(field, SortDirection.Descending);

		/// <summary>
		/// Configures the query to sort in a discending order
		/// by the field identified by the given name.
		/// </summary>
		/// <param name="field">
		/// The name of the field to sort by.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that combines
		/// the current sort rule with the one created
		/// for the given field.
		/// </returns>
		public Query OrderByDescending(string field) 
			=> OrderBy(field, SortDirection.Descending);

		private static ISort CombineSort(ISort? sort, ISort other) {
			if (sort == null)
				return other;

			return sort.Combine(other);
		}

		/// <summary>
		/// Creates a new query with the given filter.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to filter.
		/// </typeparam>
		/// <param name="filter">
		/// The filter to apply to the query.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that applies
		/// the given filter.
		/// </returns>
		public static Query Where<TEntity>(Expression<Func<TEntity, bool>>? filter)
			where TEntity : class
			=> new Query(filter == null ? QueryFilter.Empty : QueryFilter.Where<TEntity>(filter));

		/// <summary>
		/// Creates a new query with the given filter.
		/// </summary>
		/// <param name="filter">
		/// The filter to apply to the query.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="Query"/> that applies
		/// the given filter.
		/// </returns>
		public static Query Where(IQueryFilter filter)
			=> new Query(filter);
	}
}
