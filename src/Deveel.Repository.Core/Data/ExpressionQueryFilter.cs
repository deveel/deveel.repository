using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// An implementation of a query expr that uses a lambda expression
	/// </summary>
	/// <typeparam name="TEntity">The type of entity to construct
	/// the expr</typeparam>
	public sealed class ExpressionQueryFilter<TEntity> : IQueryFilter where TEntity : class, IDataEntity {
		/// <summary>
		/// Constructs a new query filter with a given expressions
		/// </summary>
		/// <param name="expr">The LINQ expression that is used to filter</param>
		/// <exception cref="ArgumentNullException"></exception>
		public ExpressionQueryFilter(Expression<Func<TEntity, bool>> expr) {
			Expression = expr ?? throw new ArgumentNullException(nameof(expr));
		}

		/// <summary>
		/// Gets the LINQ expression as filter
		/// </summary>
		public Expression<Func<TEntity, bool>> Expression { get; }
	}
}
