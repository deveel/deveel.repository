using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a transaction provided by the underlying
	/// storage of the repository, to isolate operations
	/// of access to the data
	/// </summary>
	public interface IDataTransaction : IDisposable, IAsyncDisposable {
		/// <summary>
		/// Starts the transaction asynchronously
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that encapsulates the transaction
		/// start command.
		/// </returns>
		Task BeginAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Commits all changes within the scope of this transaction
		/// to the underlying repository
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that encapsulates the transaction
		/// commitment command.
		/// </returns>
		Task CommitAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Rollbacks any changes within the scope of this transaction,
		/// preventing being reflected to the underlying repository
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// Returns a <see cref="Task"/> that encapsulates the transaction
		/// rollback command.
		/// </returns>
		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}