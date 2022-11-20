using MongoDB.Driver;

namespace Deveel.Data {
    static class PageRequestExtensions {
        public static MongoPageQuery<TDocument> AsPageQuery<TDocument>(this RepositoryPageRequest request, Func<string, FieldDefinition<TDocument, object>>? fieldSelector = null)
            where TDocument : class, IEntity {
            var query = new MongoPageQuery<TDocument>(request.Page, request.Size);

            if (request.Filter != null) {
                var filter = Builders<TDocument>.Filter.Empty;

                if (request.Filter is ExpressionQueryFilter<TDocument> expr) {
                    filter = Builders<TDocument>.Filter.Where(expr.Expression);
                } else if (request.Filter is MongoQueryFilter<TDocument> filterDef) {
                    filter = filterDef.Filter;
                }

                query.Filter = filter;
            }

            if (request.SortBy != null) {
                SortDefinition<TDocument>? sortBy = null;

                foreach (var s in request.SortBy) {
                    SortDefinition<TDocument>? sort = null;

                    if (s.Field is ExpressionFieldRef<TDocument> expr) {
                        sort = s.Ascending ?
                            Builders<TDocument>.Sort.Ascending(expr.Expression) :
                            Builders<TDocument>.Sort.Descending(expr.Expression);
                    } else if (s.Field is StringFieldRef stringRef) {
						if (fieldSelector == null)
							throw new NotSupportedException($"No field selector was provider: '{stringRef.FieldName}' cannot be derefereced");

                        var field = fieldSelector(stringRef.FieldName);

                        sort = s.Ascending ?
                            Builders<TDocument>.Sort.Ascending(field) :
                            Builders<TDocument>.Sort.Descending(field);
                    } else {
                        throw new NotSupportedException();
                    }

                    if (sort != null) {
                        if (sortBy == null) {
                            sortBy = sort;
                        } else {
                            sortBy = Builders<TDocument>.Sort.Combine(sortBy, sort);
                        }
                    }
                }

                query.SortBy = sortBy;
            }

            return query;
        }
    }
}