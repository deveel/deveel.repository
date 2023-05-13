using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	/// <summary>
	/// Extends the <see cref="RepositoryPageRequest"/> to provide
	/// helper functions to order by a set of fields coming from 
	/// a query string
	/// </summary>
	public static class RepositoryPageRequestExtensions {
		/// <summary>
		/// Requests the page to be sorted by the given 
		/// set of fields coming from a query string
		/// </summary>
		/// <param name="request">
		/// The page request to the repository.
		/// </param>
		/// <param name="sort">
		/// The list of fields to sort the result by.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPageRequest"/> that has
		/// the result sort applied.
		/// </returns>
		public static RepositoryPageRequest OrderByQueryString(this RepositoryPageRequest request, IList<string> sort) { 
			if (sort != null && sort.Count > 0) {
				foreach (var s in sort) {
					if (QueryStringResultSort.TryParse(s, out var result)) {
						foreach (var item in result) {
							request = request.OrderBy(item);
						}
					}
				}
			}

			return request;
		}

		/// <summary>
		/// Requests the page to be sorted by the given 
		/// set of fields coming from a query string
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity to sort.
		/// </typeparam>
		/// <param name="request">
		/// The page request to the repository.
		/// </param>
		/// <param name="sort">
		/// The list of fields to sort the result by.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="RepositoryPageRequest{TEntity}"/> that has
		/// the result sort applied.
		/// </returns>
		public static RepositoryPageRequest<TEntity> OrderByQueryString<TEntity>(this RepositoryPageRequest<TEntity> request, IList<string> sort, Func<string, Expression<Func<TEntity, object>>?>? mapper = null)
			where TEntity : class {
			if (sort != null && sort.Count > 0) {
				foreach (var s in sort) {
					if (QueryStringResultSort.TryParse(s, out var result)) {
						foreach (var item in result) {
							if (mapper != null && item is FieldResultSort fieldSort) {
								var field = mapper(fieldSort.FieldName);
								if (field != null) {
									if (fieldSort.Ascending) {
										request = request.OrderBy(field);
									} else {
										request = request.OrderByDescending(field);
									}
								}
							} else {
								request = request.OrderBy(item);
							}
						}
					}
				}
			}

			return request;
		}
	}
}
