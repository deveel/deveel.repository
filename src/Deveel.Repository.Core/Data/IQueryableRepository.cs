using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a repository that is capable of being queried
	/// </summary>
	/// <typeparam name="TEntity">
	/// The strongly typed entity that is stored in the repository
	/// </typeparam>
	public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
		/// <summary>
		/// Gets a queryable object that can be used to query the repository
		/// </summary>
		/// <returns>
		/// Returns an instance of <see cref="IQueryable{T}"/> that can be used
		/// to query the repository.
		/// </returns>
		IQueryable<TEntity> AsQueryable();
	}
}