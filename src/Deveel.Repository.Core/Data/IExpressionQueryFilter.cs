using System.Linq.Expressions;

namespace Deveel.Data {
	/// <summary>
	/// A type of query filter that is convertible
	/// to a LINQ lambda expression for filtering.
	/// </summary>
	public interface IExpressionQueryFilter : IQueryFilter {
		/// <summary>
		/// Converts the filter to a LINQ expression that can be
		/// used to filter a <see cref="IQueryable{TEntity}"/> storage
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity to be filtered
		/// </typeparam>
		/// <returns>
		/// Returns an instance of <see cref="LambdaExpression"/> that
		/// can be used to filter a <see cref="IQueryable{TEntity}"/>.
		/// </returns>
		Expression<Func<TEntity, bool>> AsLambda<TEntity>()
			where TEntity : class;
	}
}
