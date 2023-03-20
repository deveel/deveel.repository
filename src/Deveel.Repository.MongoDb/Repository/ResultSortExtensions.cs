using System;

using Deveel.Data;

using MongoDB.Driver;

namespace Deveel.Repository {
	static class ResultSortExtensions {
		public static SortDefinition<TEntity>? AsMongoSort<TEntity>(this IResultSort? resultSort, Func<string, FieldDefinition<TEntity, object>>? fieldSelector = null)
			where TEntity : class, IDataEntity {

			if (resultSort == null)
				return null;

			if (resultSort.Field == null)
				throw new ArgumentException("The field reference of the result sort is missing");

			if (resultSort.Field is ExpressionFieldRef<TEntity> exprRef) {
				return resultSort.Ascending ? 
					Builders<TEntity>.Sort.Ascending(exprRef.Expression) :
					Builders<TEntity>.Sort.Descending(exprRef.Expression);
			} else if (resultSort.Field is StringFieldRef stringRef) {
				var fieldDef = fieldSelector?.Invoke(stringRef.FieldName) ?? stringRef.FieldName;

				return resultSort.Ascending ?
					Builders<TEntity>.Sort.Ascending(fieldDef) :
					Builders<TEntity>.Sort.Descending(fieldDef);
			}

			throw new NotSupportedException("The field reference of type '{resultSort.Field.GetType()}' is not supported by Mongo repositories");
		}
	}
}
