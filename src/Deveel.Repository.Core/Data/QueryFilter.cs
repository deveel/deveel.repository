using System;

namespace Deveel.Data
{
    public static class QueryFilter
    {
        public static readonly IQueryFilter Empty = new EmptyQueryFilter();

        public static bool IsEmpty(this IQueryFilter filter) => Equals(filter, Empty);

        struct EmptyQueryFilter : IQueryFilter
        {
        }
    }
}
