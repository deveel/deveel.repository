using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a transaction provided by the underlying
	/// storage of the repository, to isolate operations
	/// of access to the data
	/// </summary>
	public interface IDataTransaction : IDisposable {
		/// <summary>
		/// Begins a new transaction on the underlying storage
		/// that isolates the access to the data
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task BeginAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Commits all the changes made to the underlying storage
		/// during the lifetime of this transaction
		/// </summary>
		/// <param name="cancellationToken">
		/// A token that can be used to cancel the operation
		/// </param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be awaited
		/// </returns>
		Task CommitAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Rolls back any changes made to the underlying storage
		/// during the lifetime of this transaction
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that can be awaited
		/// </returns>
		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}
