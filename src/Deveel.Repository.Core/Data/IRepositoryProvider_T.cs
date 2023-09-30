namespace Deveel.Data {
	/// <summary>
	/// Represents an provider of strongly-typed repositories that
	/// are isolating the entities of a given tenant
	/// </summary>
	/// <typeparam name="TEntity">The type of entity handled by the
	/// repository instances</typeparam>
	public interface IRepositoryProvider<TEntity> where TEntity : class {
        Task<IRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
    }
}
