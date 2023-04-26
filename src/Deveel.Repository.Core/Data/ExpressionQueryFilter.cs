using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of a query filter that uses a lambda expression
	/// </summary>
	/// <typeparam name="TEntity">The type of entity to construct
	/// the field</typeparam>
	public sealed class ExpressionQueryFilter<TEntity> : IQueryFilter where TEntity : class {
		/// <summary>
		/// Constructs the filter with the given expression
		/// </summary>
		/// <param name="expr">
		/// The expression that is used to filter the entities
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the expression is <c>null</c>.
		/// </exception>
		public ExpressionQueryFilter(Expression<Func<TEntity, bool>> expr) {
			Expression = expr ?? throw new ArgumentNullException(nameof(expr));
		}

		/// <summary>
		/// Gets the lambda filter expression
		/// </summary>
		public Expression<Func<TEntity, bool>> Expression { get; }
	}
}