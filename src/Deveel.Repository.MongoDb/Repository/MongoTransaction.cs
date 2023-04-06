using MongoDB.Driver;

namespace Deveel.Data {
    public sealed class MongoTransaction : IDataTransaction {
		private bool disposedValue;
		private IClientSessionHandle? sessionHandle;

		internal MongoTransaction(IClientSessionHandle sessionHandle) {
            this.sessionHandle = sessionHandle;
        }

		~MongoTransaction() {
			Dispose(false);
		}

		internal IClientSessionHandle SessionHandle {
			get {
				ThrowIfDisposed();

				if (sessionHandle == null)
					throw new NullReferenceException();

				return sessionHandle;
			}
		}

		private void ThrowIfDisposed() {
			if (disposedValue)
				throw new ObjectDisposedException(GetType().Name);
		}

        public Task BeginAsync(CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

            SessionHandle.StartTransaction();
            return Task.CompletedTask;
        }

		public Task CommitAsync(CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			return SessionHandle.CommitTransactionAsync(cancellationToken);
		}

		public Task RollbackAsync(CancellationToken cancellationToken = default) {
			ThrowIfDisposed();

			return SessionHandle.AbortTransactionAsync(cancellationToken); 
		}

		public async ValueTask DisposeAsync() {
			await DisposeAsync(true);
			GC.SuppressFinalize(this);
		}

		private async ValueTask DisposeAsync(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (sessionHandle != null) {
						await sessionHandle.AbortTransactionAsync();
						sessionHandle.Dispose();
					}
				}

				sessionHandle = null;
				disposedValue = true;
			}
		}

		private void Dispose(bool disposing) {
			DisposeAsync(disposing).GetAwaiter().GetResult();
		}

		public void Dispose() {
		}
	}
}