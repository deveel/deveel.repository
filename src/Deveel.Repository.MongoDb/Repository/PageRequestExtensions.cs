using Deveel.Repository;

using MongoDB.Driver;

namespace Deveel.Data {
    static class PageRequestExtensions {
        public static MongoPageQuery<TDocument> AsPageQuery<TDocument>(this RepositoryPageRequest request, Func<string, FieldDefinition<TDocument, object>>? fieldSelector = null, Func<IQueryFilter?, FilterDefinition<TDocument>>? filterBuilder = null)
            where TDocument : class {
			var query = new MongoPageQuery<TDocument>(request.Page, request.Size) {
				Filter = filterBuilder?.Invoke(request.Filter)
			};

            if (request.SortBy != null) {
                SortDefinition<TDocument>? sortBy = null;

                foreach (var s in request.SortBy) {
                    var sort = s.AsMongoSort<TDocument>(fieldSelector);

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