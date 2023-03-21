using System.Linq.Expressions;

namespace Deveel.Data {
    /// <summary>
    /// Describes the request to obtain a page of a given size
    /// from a repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <seealso cref="IRepository{TEntity}.GetPageAsync(RepositoryPageRequest{TEntity}, CancellationToken)"/>
    public class RepositoryPageRequest<TEntity> : RepositoryPageRequest where TEntity : class {
		public RepositoryPageRequest(int page, int size)
			: base(page, size) {
		}

		/// <summary>
		/// Gets or sets a filter expression that restricts the
		/// context of the page request
		/// </summary>
		public new Expression<Func<TEntity, bool>>? Filter {
			get => (base.Filter as ExpressionQueryFilter<TEntity>)?.Expression;
			set => base.Filter = value == null ? null : new ExpressionQueryFilter<TEntity>(value);
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
			if (expression is null)
				throw new ArgumentNullException(nameof(expression));

			var expr = Filter;
			if (expr == null) {
				expr = expression;
			} else {
				var body = Expression.AndAlso(expr.Body, expression.Body);
				expr = Expression.Lambda<Func<TEntity, bool>>(body, expr.Parameters[0]);
			}

			Filter = expr;

			return this;
		}

		public RepositoryPageRequest<TTarget> As<TTarget>() where TTarget : class {
			var page = new RepositoryPageRequest<TTarget>(Page, Size);

			if (Filter != null) {
				page.Filter = Filter.As<TTarget>();
			}
			if (SortBy != null)
				page.SortBy = SortBy;

			return page;
		}
	}
}