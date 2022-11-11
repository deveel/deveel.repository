using System;

using Deveel.Data;

namespace Deveel.Repository {
	public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity {
		IQueryable<TEntity> AsQueryable();
	}
}
