using System;
using System.Linq.Dynamic.Core;

using Deveel.Filters;

namespace Deveel.Data {
	public static class RepositoryExtensions {
		#region Find

		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAsync<TEntity>("x", expression, cancellationToken);


		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string paramName, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAsync<TEntity>(filter.AsLambda<TEntity>(paramName), cancellationToken);

		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAsync<TEntity>("x", filter, cancellationToken);

		#endregion

		#region FindAll

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAllAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAllAsync<TEntity>("x", expression, cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string paramName, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAllAsync<TEntity>(filter.AsLambda<TEntity>(paramName), cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.FindAllAsync<TEntity>("x", filter, cancellationToken);

		#endregion

		#region Count

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.CountAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.CountAsync("x", expression, cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, string paramName, IFilter filter, CancellationToken cancellationToken)
			where TEntity : class, IEntity
			=> repository.CountAsync<TEntity>(filter.AsLambda<TEntity>(paramName), cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IFilterableRepository<TEntity> repository, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.CountAsync("x", filter, cancellationToken);

		#endregion

		#region Exists

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.ExistsAsync<TEntity>(FilterExpression.AsLambda<TEntity>(paramName, expression), cancellationToken);

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.ExistsAsync<TEntity>("x", expression, cancellationToken);

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, string paramName, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.ExistsAsync<TEntity>(filter.AsLambda<TEntity>(paramName), cancellationToken);

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, IFilter filter, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
			=> repository.ExistsAsync("x", filter, cancellationToken);

		#endregion
	}
}
