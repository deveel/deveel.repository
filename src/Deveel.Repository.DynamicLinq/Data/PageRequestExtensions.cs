using System;

namespace Deveel.Data {
	public static class PageRequestExtensions {
		public static RepositoryPageRequest<TEntity> Where<TEntity>(this RepositoryPageRequest<TEntity> request, string paramName, string expression)
			where TEntity : class
			=> request.Where(FilterExpression.AsLambda<TEntity>(paramName, expression));

		public static RepositoryPageRequest<TEntity> Where<TEntity>(this RepositoryPageRequest<TEntity> request, string expression)
			where TEntity : class
			=> request.Where("x", expression);

	}
}
