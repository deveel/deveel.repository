﻿using System.Drawing;
using System.Linq.Expressions;

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
    /// <summary>
    /// Describes the request to obtain a page of a given size
    /// from a repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <seealso cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
    public class RepositoryPageRequest<TEntity> where TEntity : class {
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
		public RepositoryPageRequest(int page, int size) {
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
		/// Gets or sets a filter to restrict the context of the query
		/// </summary>
		public IQueryFilter? Filter { get; set; }

		/// <summary>
		/// Gets or sets an optional set of orders to sort the
		/// result of the request
		/// </summary>
		public IEnumerable<IResultSort>? ResultSorts { get; set; }

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
		public RepositoryPageRequest<TEntity> Where(Expression<Func<TEntity, bool>> expression) {
			Guard.IsNotNull(expression, nameof(expression));

			var filter = Filter;
			if (filter == null) {
				filter = QueryFilter.Where(expression);
			} else {
				filter = QueryFilter.Combine(filter, QueryFilter.Where(expression));
			}

			Filter = filter;

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
		public RepositoryPageRequest<TEntity> OrderBy(Expression<Func<TEntity, object>> selector) {
			Guard.IsNotNull(selector, nameof(selector));

			return (RepositoryPageRequest<TEntity>) OrderBy(new ExpressionResultSort<TEntity>(selector));
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
		public RepositoryPageRequest<TEntity> OrderByDescending(Expression<Func<TEntity, object>> selector) {
			Guard.IsNotNull(selector, nameof(selector));

			return (RepositoryPageRequest<TEntity>) OrderBy(new ExpressionResultSort<TEntity>(selector, false));
		}

		/// <summary>
		/// Appends the given sort order to the request
		/// </summary>
		/// <param name="resultSort">
		/// The 
		/// </param>
		/// <returns></returns>
		public RepositoryPageRequest<TEntity> OrderBy(IResultSort resultSort) {
			Guard.IsNotNull(resultSort, nameof(resultSort));

			if (ResultSorts == null)
				ResultSorts = new List<IResultSort>();

			ResultSorts = ResultSorts.Append(resultSort);

			return this;
		}

		public RepositoryPageRequest<TEntity> OrderBy(string fieldName, bool ascending = true) {
			Guard.IsNotNullOrEmpty(fieldName, nameof(fieldName));

			return OrderBy(new FieldResultSort(fieldName, ascending));
		}
	}
}