namespace Deveel.Data {
	/// <summary>
	/// Represents an provider of strongly-typed repositories that
	/// are isolating the entities of a given tenant
	/// </summary>
	/// <typeparam name="TEntity">
	/// The type of entity handled by the repository instances
	/// </typeparam>
	public interface IRepositoryProvider<TEntity> where TEntity : class {
		/// <summary>
		/// Gets a repository instance that is isolating the entities
		/// for a tenant.
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is provided.
		/// </param>
		/// <param name="cancellationToken">
		/// A cancellation token that can be used to cancel the operation.
		/// </param>
		/// <returns>
		/// Returns an instance of <see cref="IRepository{TEntity}"/> that
		/// is isolating the entities for the given tenant.
		/// </returns>
        Task<IRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
