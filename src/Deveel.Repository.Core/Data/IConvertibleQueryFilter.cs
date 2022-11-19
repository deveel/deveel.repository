using System;

namespace Deveel.Data
{
    public interface IConvertibleQueryFilter : IQueryFilter
    {
        IQueryFilter ConvertFor<TEntity>();
    }
}
