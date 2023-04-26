using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// A utility class that provides a set of static methods to create
	/// default types of query filters.
	/// </summary>
	public static class QueryFilter {
		/// <summary>
		/// Identifies an empty query filter, that implementations
		/// of the <see cref="IFilterableRepository"/> can use to
		/// convert to a default query.
		/// </summary>
		public static readonly IQueryFilter Empty = new EmptyQueryFilter();

		/// <summary>
		/// Determines if the given filter is the empty one.
		/// </summary>
		/// <param name="filter">
		/// The filter to check if it is the empty one.
		/// </param>
		/// <remarks>
		/// The method verifies if the reference of the given filter
		/// if the same of the <see cref="Empty"/> one.
		/// </remarks>
		/// <returns>
		/// Returns <c>true</c> if the given filter is the empty one,
		/// or <c>false</c> otherwise.
		/// </returns>
		public static bool IsEmpty(this IQueryFilter filter) => Equals(filter, Empty);

		/// <summary>
		/// Converts the given filter to a LINQ expression that can be
		/// used to filter a <see cref="IQueryable{TEntity}"/> storage
		/// </summary>
		/// <typeparam name="TEntity"></typeparam>
		/// <param name="filter">
		/// The instance of the filter to convert to a LINQ expression.
		/// </param>
		/// <remarks>
		/// If the given filter is the empty one, the method returns
		/// a lambda expression that always returns <c>true</c>.
		/// </remarks>
		/// <returns>
		/// Returns an instance of <see cref="Expression{TDelegate}"/> that
		/// is obtained from the conversion of the given filter.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given filter is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown when the given filter is not an instance of <see cref="ExpressionQueryFilter{TEntity}"/>.
		/// </exception>
		public static Expression<Func<TEntity, bool>> AsLambda<TEntity>(this IQueryFilter filter)
			where TEntity : class {
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));
			if (filter.IsEmpty())
				return e => true;

			if (!(filter is ExpressionQueryFilter<TEntity> filterExpr))
				throw new ArgumentException("Only expression query filters are supported");

			return filterExpr.Expression;
		}

		/// <summary>
		/// Constructs a new query filter that is built from the given
		/// LINQ expression.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity that is the target of the filter.
		/// </typeparam>
		/// <param name="exp">
		/// The lambda expression that defines the filter.
		/// </param>
		/// <remarks>
		/// Various implementations of <see cref="IFilterableRepository"/> can support
		/// LINQ expressions to define the filter to apply to the query, and this
		/// method provides a factory to create a default implementation of
		/// this kind of filter.
		/// </remarks>
		/// <returns>
		/// Returns a new instance of <see cref="ExpressionQueryFilter{TEntity}"/>
		/// wrapping the given expression.
		/// </returns>
		public static ExpressionQueryFilter<TEntity> Where<TEntity>(Expression<Func<TEntity, bool>> exp)
			where TEntity : class
			=> new ExpressionQueryFilter<TEntity>(exp);

		readonly struct EmptyQueryFilter : IQueryFilter {
		}
	}
}