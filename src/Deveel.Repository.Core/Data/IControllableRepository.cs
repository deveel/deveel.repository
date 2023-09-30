using System;

namespace Deveel.Data {
	/// <summary>
	/// A repository whose lifecycle can be controlled by the user
	/// </summary>
	public interface IControllableRepository {
		/// <summary>
		/// Checks if the repository exists in the underlying storage
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns <c>true</c> if the repository exists, or <c>false</c>
		/// </returns>
		Task<bool> ExistsAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates the repository in the underlying storage
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that completes when the repository
		/// is created in the underlying storage
		/// </returns>
		Task CreateAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Drops the repository from the underlying storage
		/// </summary>
		/// <param name="cancellationToken">
		/// A cancellation token used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that completes when the repository
		/// is dropped from the underlying storage
		/// </returns>
		Task DropAsync(CancellationToken cancellationToken = default);
	}
}
