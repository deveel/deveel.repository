using System;
using System.Threading;
using System.Threading.Tasks;

using Deveel.Repository;

using MongoDB.Driver;

namespace Deveel.Data {
	class MongoTransaction : IDataTransaction {
		public MongoTransaction(IClientSessionHandle sessionHandle) {
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
