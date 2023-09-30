using System.Linq.Expressions;

using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
	/// <summary>
	/// An object that combines multiple <see cref="IQueryFilter"/> objects
	/// into a single one.
	/// </summary>
	public sealed class CombinedQueryFilter : IExpressionQueryFilter {
		/// <summary>
		/// Constructs the filter by combining the given list of filters.
		/// </summary>
		/// <param name="filters">
		/// The list of filters to combine.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// If the given list of filters is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the given list of filters is empty.
		/// </exception>
		public CombinedQueryFilter(ICollection<IQueryFilter> filters) {
			Guard.IsNotNull(filters, nameof(filters));
			Guard.IsNotEmpty(filters, nameof(filters));

			Filters = filters.ToList().AsReadOnly();
		}

		/// <summary>
		/// Gets the list of filters that are combined into this object.
		/// </summary>
		public IReadOnlyList<IQueryFilter> Filters { get; }

		/// <summary>
		/// Creates a new combination between the filters
		/// of this object and the given one.
		/// </summary>
		/// <param name="filter">
		/// The filter to combine with this object.
		/// </param>
		/// <returns>
		/// Returns a new <see cref="CombinedQueryFilter"/> that combines
		/// the filters of this object and the given one.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the given filter is <c>null</c>.
		/// </exception>
		public CombinedQueryFilter Combine(IQueryFilter filter) {
			Guard.IsNotNull(filter, nameof(filter));

			var filters = new List<IQueryFilter>(Filters) { filter};
			return new CombinedQueryFilter(filters);
		}

		/// <inheritdoc/>
		public Expression<Func<TEntity, bool>> AsLambda<TEntity>()
			where TEntity : class {

			if (Filters.Count == 0)
				throw new InvalidOperationException("No filters were combined");

			if (Filters.Count == 1)
				return Filters[0].AsLambda<TEntity>();

			Expression<Func<TEntity, bool>>? result = null;

			foreach (var filter in Filters) {
				if (filter == null || filter.IsEmpty())
					continue;

				var lambda = filter.AsLambda<TEntity>();
				if (result == null) {
					result = lambda;
				} else {
					var lambdaParam = lambda.Parameters[0];
					if (lambdaParam.Name != result.Parameters[0].Name)
						throw new InvalidOperationException("The parameters of the filters are not the same");

					var expr = Expression.AndAlso(result.Body, lambda.Body);
					result = Expression.Lambda<Func<TEntity, bool>>(expr, lambdaParam);
				}
			}

			return result ?? throw new InvalidOperationException("No filters were combined");
		}
	}
}
