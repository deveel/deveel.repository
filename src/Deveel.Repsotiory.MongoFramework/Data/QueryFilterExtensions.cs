using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace Deveel.Data {
	static class QueryFilterExtensions {
		public static FilterDefinition<TEntity> AsMongoFilter<TEntity>(this IQueryFilter filter) where TEntity : class {
			if (filter is ExpressionQueryFilter<TEntity> exprFilter) {
				return Builders<TEntity>.Filter.Where(exprFilter.Expression);
			} else if (filter.IsEmpty()) {
				return Builders<TEntity>.Filter.Empty;
			} else if (filter is CombinedQueryFilter combined) {
				var mongo = Builders<TEntity>.Filter.Empty;

				foreach (var f in combined.Filters) {
					Builders<TEntity>.Filter.And(mongo, f.AsMongoFilter<TEntity>());
				}

				return mongo;
			}

			throw new NotSupportedException($"The filter of type '{filter.GetType()}' is not supported");
		}
	}
}
