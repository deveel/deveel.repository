using System;
using System.Linq.Dynamic.Core;

namespace Deveel.Data {
	/// <summary>
	/// Provides a set of extension methods for the <see cref="IRepository{TEntity}"/> contract
	/// that allows to perform filtering using a dynamic LINQ expression.
	/// </summary>
	public static class RepositoryExtensions {
		#region Find

		/// <summary>
		/// Finds a single entity in the repository that matches the given dynamic LINQ expression.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to find in the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository to perform the search.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to use in the expression.
		/// </param>
		/// <param name="expression">
		/// The dynamic LINQ expression to use to filter the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that matches the given expression,
		/// otherwise <c>null</c> if no entity is found.
		/// </returns>
		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().FindAsync(new DynamicLinqFilter(paramName, expression), cancellationToken);

		/// <summary>
		/// Finds a single entity in the repository that matches the given dynamic LINQ expression.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to find in the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The repository to perform the search.
		/// </param>
		/// <param name="expression">
		/// The dynamic LINQ expression to use to filter the entities.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <typeparamref name="TEntity"/> that matches the given expression,
		/// otherwise <c>null</c> if no entity is found.
		/// </returns>
		public static Task<TEntity?> FindAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().FindAsync(new DynamicLinqFilter(expression), cancellationToken);

		#endregion

		#region FindAll

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().FindAllAsync(new DynamicLinqFilter(paramName, expression), cancellationToken);

		public static Task<IList<TEntity>> FindAllAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().FindAllAsync(new DynamicLinqFilter(expression), cancellationToken);

		#endregion

		#region Count

		public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, string paramName, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().CountAsync(new DynamicLinqFilter(paramName, expression), cancellationToken);

		public static Task<long> CountAsync<TEntity>(this IRepository<TEntity> repository, string expression, CancellationToken cancellationToken = default)
			where TEntity : class
			=> repository.AsFilterable().CountAsync(new DynamicLinqFilter(expression), cancellationToken);

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
