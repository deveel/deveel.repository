using System.Linq.Expressions;

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
    /// <summary>
    /// Describes the request to obtain a page of a given size
    /// from a repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <seealso cref="IPageableRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
    public class RepositoryPageRequest<TEntity> : RepositoryPageRequest where TEntity : class {
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
		public RepositoryPageRequest(int page, int size)
			: base(page, size) {
		}

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

		/// <inheritdoc/>
		public new RepositoryPageRequest<TEntity> OrderBy(IResultSort resultSort) {
			return (RepositoryPageRequest<TEntity>) base.OrderBy(resultSort);
		}

		public RepositoryPageRequest<TTarget> As<TTarget>() where TTarget : class {
			var page = new RepositoryPageRequest<TTarget>(Page, Size);

			if (Filter != null) {
				page.Filter = QueryFilter.Where(Filter.AsLambda<TEntity>().As<TTarget>());
			}
			if (ResultSorts != null)
				page.ResultSorts = ResultSorts;

			return page;
		}
	}
}