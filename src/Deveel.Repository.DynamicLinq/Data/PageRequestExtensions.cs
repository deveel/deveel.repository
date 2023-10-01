using System;

namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IRepository{T}"/> interface with methods
	/// that allow to perform filtering and paging of the data.
	/// </summary>
	public static class PageRequestExtensions {
		/// <summary>
		/// Adds a dynamic filter to the request.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity to filter.
		/// </typeparam>
		/// <param name="request">
		/// The request to add the filter to.
		/// </param>
		/// <param name="paramName">
		/// The name of the parameter to use in the filter expression.
		/// </param>
		/// <param name="expression">
		/// The dynamic LINQ expression to use to filter the data.
		/// </param>
		/// <returns>
		/// Returns the instance of the <see cref="RepositoryPageRequest{TEntity}"/>
		/// with the filter appended.
		/// </returns>
		public static RepositoryPageRequest<TEntity> Where<TEntity>(this RepositoryPageRequest<TEntity> request, string paramName, string expression)
			where TEntity : class
			=> request.Where(FilterExpression.AsLambda<TEntity>(paramName, expression));

		/// <summary>
		/// Adds a dynamic filter to the request.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity to filter.
		/// </typeparam>
		/// <param name="request">
		/// The request to add the filter to.
		/// </param>
		/// <param name="expression">
		/// The dynamic LINQ expression to use to filter the data.
		/// </param>
		/// <remarks>
		/// This overload uses the <see cref="DynamicLinqFilter.DefaultParameterName">default
		/// parameter name</see> for the filter expression.
		/// </remarks>
		/// <returns>
		/// Returns the instance of the <see cref="RepositoryPageRequest{TEntity}"/>
		/// with the filter appended.
		/// </returns>
		public static RepositoryPageRequest<TEntity> Where<TEntity>(this RepositoryPageRequest<TEntity> request, string expression)
			where TEntity : class
			=> request.Where(DynamicLinqFilter.DefaultParameterName, expression);

	}
}
