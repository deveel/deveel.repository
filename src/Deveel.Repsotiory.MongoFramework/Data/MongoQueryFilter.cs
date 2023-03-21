using System;

using Deveel.Data;

using MongoDB.Driver;

namespace Deveel.Data
{
    public sealed class MongoQueryFilter<TDocument> : IQueryFilter where TDocument : class
    {
        public MongoQueryFilter(FilterDefinition<TDocument> filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        public FilterDefinition<TDocument> Filter { get; }
    }
}
