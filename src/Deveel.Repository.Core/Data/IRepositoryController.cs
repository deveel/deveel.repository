using System;

namespace Deveel.Data {
	/// <summary>
	/// A service used to control the lifecycle of the repositories
	/// </summary>
	public interface IRepositoryController {
		/// <summary>
		/// Creates all the repositories for the current context
		/// </summary>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task CreateAllRepositoriesAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates all the repositories for the given tenant
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repositories are created
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task CreateTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a repository for the given entity type
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity managed by the repository
		/// </typeparam>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task CreateRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class;

		/// <summary>
		/// Creates a repository for the given entity type and tenant
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity managed by the repository
		/// </typeparam>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is created
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task CreateTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class;

		/// <summary>
		/// Drops all the repositories for the current context
		/// </summary>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task DropAllRepositoriesAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Drops all the repositories for the given tenant
		/// </summary>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repositories are dropped
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task DropTenantRepositoriesAsync(string tenantId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Drops the repository for the given entity type
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity managed by the repository
		/// </typeparam>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task DropRepositoryAsync<TEntity>(CancellationToken cancellationToken = default)
			where TEntity : class;

		/// <summary>
		/// Drops the repository for the given entity type and tenant
		/// </summary>
		/// <typeparam name="TEntity">
		/// The type of the entity managed by the repository
		/// </typeparam>
		/// <param name="tenantId">
		/// The identifier of the tenant for which the repository is dropped
		/// </param>
		/// <param name="cancellationToken">
		/// A token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be used to await the operation
		/// </returns>
		Task DropTenantRepositoryAsync<TEntity>(string tenantId, CancellationToken cancellationToken = default)
			where TEntity : class;
	}
}
