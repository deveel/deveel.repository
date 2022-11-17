using System;

namespace Deveel.Repository {
	public interface IConvertibleQueryFilter : IQueryFilter {
		IQueryFilter ConvertFor<TEntity>();
	}
}
