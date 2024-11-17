namespace Deveel.Data
{
	public interface IRepositoryProvider<TRepository>
		where TRepository : class
	{
		Task<TRepository?> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
	}
}
