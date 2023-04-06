using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	public static class QueryFilter {
		public static readonly IQueryFilter Empty = new EmptyQueryFilter();

		public static bool IsEmpty(this IQueryFilter filter) => Equals(filter, Empty);

		public static ExpressionQueryFilter<TEntity> Where<TEntity>(Expression<Func<TEntity, bool>> exp)
			where TEntity : class
			=> new ExpressionQueryFilter<TEntity>(exp);

		readonly struct EmptyQueryFilter : IQueryFilter {
		}
	}
}