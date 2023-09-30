using System.Linq.Expressions;
using System.Reflection;

namespace Deveel.Data {
	public static class ResultSortExtensions {
		/// <summary>
		/// Applies a sorting rule to the given queryable.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity that is the target of the sorting and that
		/// is the source of the field to sort.
		/// </typeparam>
		/// <param name="sort">
		/// The sorting rule to apply to the queryable.
		/// </param>
		/// <param name="queriable">
		/// The queryable object to sort.
		/// </param>
		/// <returns>
		/// Returns a new queryable object that is the result of the sorting
		/// rule applied to the given parameter.
		/// </returns>
		public static IQueryable<TEntity> Apply<TEntity>(this IResultSort sort, IQueryable<TEntity> queriable)
			where TEntity : class {
			var mapped = sort.Map<TEntity>(null);

			return mapped.Ascending 
				? (IQueryable<TEntity>) queriable.OrderBy(mapped.FieldSelector) 
				: (IQueryable<TEntity>)queriable.OrderByDescending(mapped.FieldSelector);
		}

		/// <summary>
		/// Maps the given sorting rule to an expression-based sorting rule
		/// that can be applied to a queryable.
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of entity that is the target of the sorting and that
		/// is the source of the field to sort.
		/// </typeparam>
		/// <param name="sort">
		/// The sorting rule to map.
		/// </param>
		/// <param name="fieldSelector">
		/// An optional function that can be used to map the field name
		/// to an expression that selects the field to sort.
		/// </param>
		/// <returns>
		/// Returns an expression-based sorting rule that can be applied
		/// to a queryable.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the <paramref name="fieldSelector"/> is <c>null</c>
		/// and the <paramref name="sort"/> is a <see cref="FieldResultSort"/>
		/// and the field name is not a valid member of the entity.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// Thrown if the <paramref name="sort"/> is not supported.
		/// </exception>
		public static ExpressionResultSort<TEntity> Map<TEntity>(this IResultSort sort, Func<string, Expression<Func<TEntity, object>>>? fieldSelector = null)
			where TEntity : class {
			if (sort is ExpressionResultSort<TEntity> exprSort) {
				return exprSort;
			} else if (sort is FieldResultSort fieldSort) {
				if (fieldSelector != null) {
					var field = fieldSelector(fieldSort.FieldName);
					return new ExpressionResultSort<TEntity>(field, fieldSort.Ascending);
				} else {
					// TODO: support dot-delimited field names to access nested properties
					var member = typeof(TEntity).GetMembers(BindingFlags.Public | BindingFlags.Instance)
						.Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
						.FirstOrDefault(x => String.Equals(x.Name, fieldSort.FieldName, StringComparison.OrdinalIgnoreCase));

					if (member == null)
						throw new InvalidOperationException($"The field '{fieldSort.FieldName}' is not a valid member of the entity '{typeof(TEntity).Name}'");

					// TODO: find a way to identify a variable name
					//       that doesn't conflict with other variables
					var param = Expression.Parameter(typeof(TEntity), "x");
					var exp = Expression.MakeMemberAccess(param, member);
					
					return new ExpressionResultSort<TEntity>(Expression.Lambda<Func<TEntity, object>>(exp, param), fieldSort.Ascending);
				}
			}

			throw new NotSupportedException($"The sort of type '{sort.GetType()}' is not supported");
		}
	}
}
