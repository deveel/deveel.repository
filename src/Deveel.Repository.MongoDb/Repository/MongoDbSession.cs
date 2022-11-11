using System;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace Deveel.Data {
	class MongoDbSession : IDataTransaction {
		public MongoDbSession(IClientSessionHandle sessionHandle) {
			this.SessionHandle = sessionHandle;
		}

		public IClientSessionHandle SessionHandle { get; }

		public Task BeginAsync(CancellationToken cancellationToken) {
			SessionHandle.StartTransaction();
			return Task.CompletedTask;
		}

		public Task CommitAsync(CancellationToken cancellationToken) => SessionHandle.CommitTransactionAsync(cancellationToken);

		public void Dispose() => SessionHandle?.Dispose();

		public Task RollbackAsync(CancellationToken cancellationToken) => SessionHandle.AbortTransactionAsync(cancellationToken);
	}
}
