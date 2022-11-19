using MongoDB.Driver;

namespace Deveel.Data {
    public sealed class MongoTransaction : IDataTransaction {
        internal MongoTransaction(IClientSessionHandle sessionHandle) {
            SessionHandle = sessionHandle;
        }

        internal IClientSessionHandle SessionHandle { get; }

        public Task BeginAsync(CancellationToken cancellationToken = default) {
            SessionHandle.StartTransaction();
            return Task.CompletedTask;
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) 
            => SessionHandle.CommitTransactionAsync(cancellationToken);

        public void Dispose() => SessionHandle?.Dispose();

        public Task RollbackAsync(CancellationToken cancellationToken = default) 
            => SessionHandle.AbortTransactionAsync(cancellationToken);
    }
}