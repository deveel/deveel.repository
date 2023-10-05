﻿namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="IQueryableRepository{TEntity}"/> interface
	/// to provide methods to query the repository.
	/// </summary>
	public static class QueryableRepositoryExtensions {
		/// <summary>
		/// Gets a page of entities from the repository,
		/// given a request object that defines the scope
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity handled by the repository.
		/// </typeparam>
		/// <param name="repository">
		/// The instance of the repository from which the entities are retrieved.
		/// </param>
		/// <param name="request">
		/// The request object that defines the scope of the page to retrieve.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPage{TEntity}"/> that
		/// is the result of the query.
		/// </returns>
		public static RepositoryPage<TEntity> GetPage<TEntity>(this IQueryableRepository<TEntity> repository, RepositoryPageRequest<TEntity> request)
			where TEntity : class {
			var query = repository.AsQueryable();

			if (request.Filter != null && !request.Filter.IsEmpty()) {
				query = request.Filter.Apply(query);
			}

			if ((request.ResultSorts?.Any() ?? false)) {
				foreach (var sort in request.ResultSorts) {
					query = sort.Apply(query);
				}
			}

			var total = query.Count();
			var items = query.Skip(request.Offset)
				.Take(request.Size)
				.ToList();

			return new RepositoryPage<TEntity>(request, total, items);
		}
	}
}