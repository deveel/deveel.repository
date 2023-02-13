using System;
using System.Linq.Expressions;

namespace Deveel.Data {
	public interface IEntityFieldMapper<TEntity> where TEntity : class {
		Expression<Func<TEntity, object>> Map(string propertyName);
	}
}
