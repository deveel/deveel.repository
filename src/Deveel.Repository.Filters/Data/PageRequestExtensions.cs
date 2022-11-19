using System;

using Deveel.Filters;

namespace Deveel.Data {
	public static class PageRequestExtensions {
		public static PageRequest<TEntity> Where<TEntity>(this PageRequest<TEntity> request, string paramName, IFilter filter)
			where TEntity : class, IEntity
			=> request.Where(filter.AsLambda<TEntity>(paramName));

		public static PageRequest<TEntity> Where<TEntity>(this PageRequest<TEntity> request, IFilter filter)
			where TEntity : class, IEntity
			=> request.Where("x", filter);

		public static PageRequest<TEntity> Where<TEntity>(this PageRequest<TEntity> request, string paramName, string expression)
			where TEntity : class, IEntity
			=> request.Where(FilterExpression.AsLambda<TEntity>(expression, paramName));

		public static PageRequest<TEntity> Where<TEntity>(this PageRequest<TEntity> request, string expression)
			where TEntity : class, IEntity
			=> request.Where("x", expression);

	}
}
