using System;
using System.Linq.Dynamic.Core;

namespace Deveel.Data {
	public static class RepositoryExtensions {
		#region Find

		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.FindAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.FindAsync<TEntity>("x", expression, cancellationToken);

		#endregion

		#region FindAll

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.FindAllAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.FindAllAsync<TEntity>("x", expression, cancellationToken);

		#endregion

		#region Count

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.CountAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.CountAsync("x", expression, cancellationToken);

		#endregion

		#region Exists

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.ExistsAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.ExistsAsync<TEntity>("x", expression, cancellationToken);

		#endregion
	}
}
