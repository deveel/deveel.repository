using System;

namespace Deveel.Data {
	/// <summary>
	/// Represents a transaction provided by the underlying
	/// storage of the repository, to isolate operations
	/// of access to the data
	/// </summary>
	public interface IDataTransaction : IDisposable {
		Task CommitAsync(CancellationToken cancellationToken = default);

		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}
