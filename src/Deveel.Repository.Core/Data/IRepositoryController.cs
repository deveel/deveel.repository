using System;

namespace Deveel.Data {
	public interface IRepositoryController {
		Task CreateAllRepositoriesAsync(CancellationToken cancellationToken = default);

		Task CreateTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default);

		Task CreateRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class, IEntity;

		Task CreateTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity;

		Task DropAllRepositoriesAsync(CancellationToken cancellationToken = default);

		Task DropTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default);

		Task DropRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class, IEntity;

		Task DropTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity;
	}
}
