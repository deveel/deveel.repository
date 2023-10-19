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

using System.Linq.Expressions;

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
	/// <summary>
	/// Describes the request to obtain a page of a given size
	/// from a repository
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	/// <seealso cref="IPageableRepository{TEntity}.GetPageAsync(PageQuery{TEntity}, CancellationToken)"/>
	public class PageQuery<TEntity> where TEntity : class {
		/// <summary>
		/// Constructs a new page request with the given page number and size
		/// </summary>
		/// <param name="page">
		/// The number of the page to request
		/// </param>
		/// <param name="size">
		/// The maximum size of the page to return.
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If either the page number or the page size are smaller than 1.
		/// </exception>
		public PageQuery(int page, int size) {
			Guard.IsGreaterThanOrEqualTo(page, 1, nameof(page));
			Guard.IsGreaterThanOrEqualTo(size, 1, nameof(size));

			Page = page;
			Size = size;
		}

		/// <summary>
		/// Gets the number of the page to return
		/// </summary>
		public int Page { get; }

		/// <summary>
		/// Gets the maximum number of items to be returned.
		/// </summary>
		public int Size { get; }

		/// <summary>
		/// Gets the starting offet in the repository where to start
		/// collecting the items to return
		/// </summary>
		public int Offset => (Page - 1) * Size;

		/// <summary>
		/// The query that is applied to the request
		/// </summary>
		public Query? Query { get; set; }

		/// <summary>
		/// When the request has a query defined, gets the filter
		/// that is applied to the request.
		/// </summary>
		public IQueryFilter? Filter => Query?.Filter;

		/// <summary>
		/// When the request has a query defined, gets the sort
		/// rule that is applied to the request.
		/// </summary>
		public ISort? Sort => Query?.Sort;

		/// <summary>
		/// Sets or appends a new filter
		/// </summary>
		/// <param name="expression">The filter expression to add</param>
		/// <returns>
		/// Returns this page request with the new filter
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the <paramref name="expression"/> is <c>null</c>.
		/// </exception>
		public PageQuery<TEntity> Where(Expression<Func<TEntity, bool>> expression) {
			Guard.IsNotNull(expression, nameof(expression));

			if (Query == null || !Query.Value.HasFilter) {
				Query = Data.Query.Where(expression);
			} else if (Query != null) {
				Query = Query.Value.And(expression);
			} else if (Query == null) {
				Query = Data.Query.Where(expression);
			}

			return this;
		}

		/// <summary>
		/// Appends an ascending sort rule to the page request
		/// </summary>
		/// <param name="selector">
		/// The expression that selects the field to sort by.
		/// </param>
		/// <returns>
		/// Returns this instance of the page request with the
		/// appended sort rule.
		/// </returns>
		public PageQuery<TEntity> OrderBy(Expression<Func<TEntity, object?>> selector) {
			Guard.IsNotNull(selector, nameof(selector));

			return OrderBy(new ExpressionSort<TEntity>(selector));
		}

		/// <summary>
		/// Appends a descending sort rule to the page request
		/// </summary>
		/// <param name="selector">
		/// The expression that selects the field to sort by.
		/// </param>
		/// <returns>
		/// Returns this instance of the page request with the
		/// appended sort rule.
		/// </returns>
		public PageQuery<TEntity> OrderByDescending(Expression<Func<TEntity, object?>> selector) {
			Guard.IsNotNull(selector, nameof(selector));

			return OrderBy(new ExpressionSort<TEntity>(selector, SortDirection.Descending));
		}

		/// <summary>
		/// Appends the given sort order to the request
		/// </summary>
		/// <param name="sort">
		/// The 
		/// </param>
		/// <returns></returns>
		public PageQuery<TEntity> OrderBy(ISort sort) {
			Guard.IsNotNull(sort, nameof(sort));

			if (Query == null)
				Query = Data.Query.Empty;

			Query = Query.Value.OrderBy(sort);

			return this;
		}

		/// <summary>
		/// Appends an order by the given field name
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to sort by
		/// </param>
		/// <param name="direction">
		/// The sort direction to order by.
		/// </param>
		/// <returns>
		/// Returns this instance of the page request with the
		/// appended sort rule.
		/// </returns>
		public PageQuery<TEntity> OrderBy(string fieldName, SortDirection direction = SortDirection.Ascending) {
			Guard.IsNotNullOrEmpty(fieldName, nameof(fieldName));

			return OrderBy(new FieldSort(fieldName, direction));
		}

		/// <summary>
		/// Appends a descending sort rule to the page request.
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to sort by.
		/// </param>
		/// <returns>
		/// Returns this instance of the page request with the
		/// appended sort rule.
		/// </returns>
		public PageQuery<TEntity> OrderByDescending(string fieldName) {
			Guard.IsNotNullOrEmpty(fieldName, nameof(fieldName));

			return OrderBy(fieldName, SortDirection.Descending);
		}

		/// <summary>
		/// Applies the query to the given <see cref="IQueryable{TEntity}"/>,
		/// if this page request has a query defined.
		/// </summary>
		/// <param name="queryable">
		/// The queryable to apply the query to.
		/// </param>
		/// <returns>
		/// Returns a <see cref="IQueryable{TEntity}"/> that is the result
		/// of the application of the query to the given queryable.
		/// </returns>
		public IQueryable<TEntity> ApplyQuery(IQueryable<TEntity> queryable) {
			if (Query != null)
				queryable = Query.Value.Apply(queryable);

			return queryable;
		}
	}
}