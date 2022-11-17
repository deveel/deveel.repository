using System;

namespace Deveel.Repository {
	public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity {
		IQueryable<TEntity> AsQueryable();
	}
}
