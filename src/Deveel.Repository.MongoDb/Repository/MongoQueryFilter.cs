using System;

using Deveel.Data;

using MongoDB.Driver;

namespace Deveel.Repository {
	public sealed class MongoQueryFilter<TDocument> : IQueryFilter where TDocument : class, IEntity {
		public MongoQueryFilter(FilterDefinition<TDocument> filter) {
			Filter = filter ?? throw new ArgumentNullException(nameof(filter));
		}

		public FilterDefinition<TDocument> Filter { get; }
	}
}
