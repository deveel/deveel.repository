namespace Deveel.Data {
	/// <summary>
	/// Defines a repository that is able to track changes 
	/// on entities returned by queries.
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity managed by the repository.
	/// </typeparam>
	public interface ITrackingRepository<TEntity> : ITrackingRepository<TEntity, object>
		where TEntity : class {
	}
}