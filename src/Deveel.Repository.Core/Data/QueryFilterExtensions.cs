using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	public static class QueryFilterExtensions {
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
	}
}
