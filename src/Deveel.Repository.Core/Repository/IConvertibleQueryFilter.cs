using System;

using Deveel.Data;

namespace Deveel.Repository {
	public interface IConvertibleQueryFilter : IQueryFilter {
		IQueryFilter ConvertFor<TEntity>();
	}
}
