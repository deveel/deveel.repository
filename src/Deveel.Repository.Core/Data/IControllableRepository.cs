using System;

namespace Deveel.Data {
	/// <summary>
	/// A contract that defines a <see cref="IRepository"/> as controllable
	/// through its lifecycle.
	/// </summary>
	public interface IControllableRepository : IRepository {
		/// <summary>
		/// Checks if the repository actually exists in the underlying infrastructure
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns as <see cref="Task"/> that has a value of <c>true</c> if the repository 
		/// actually exists in the underlying infrastructure, otherwise has <c>false</c> value.
		/// </returns>
		Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates the repository in the underlying infrastructure.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that executes the repository creation command.
		/// </returns>
		Task CreateAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Deletes the repository from the underlying infrastructure.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that executes the repository destroy command.
		/// </returns>
		Task DropAsync(CancellationToken cancellationToken = default);
	}
}
