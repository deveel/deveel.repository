using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public static class ResultSortExtensions {
		public static IQueryable<TEntity> Apply<TEntity>(this IResultSort sort, IQueryable<TEntity> queriable)
			where TEntity : class {
			if (sort is ExpressionResultSort<TEntity> exprSort) {
				if (exprSort.Ascending) {
					return queriable.OrderBy(exprSort.FieldSelector);
				} else {
					return queriable.OrderByDescending(exprSort.FieldSelector);
				}
			}

			throw new NotSupportedException($"The sort of type '{sort.GetType()}' is not supported");
		}

		public static ExpressionResultSort<TEntity> Map<TEntity>(this IResultSort sort, Func<string, Expression<Func<TEntity, object>>> fieldSelector)
			where TEntity : class {
			if (sort is ExpressionResultSort<TEntity> exprSort) {
				return exprSort;
			} else if (sort is FieldResultSort fieldSort) {
				var field = fieldSelector(fieldSort.FieldName);
				return new ExpressionResultSort<TEntity>(field, fieldSort.Ascending);
			}

			throw new NotSupportedException($"The sort of type '{sort.GetType()}' is not supported");
		}
	}
}
