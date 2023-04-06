using System;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Deveel.Data {
    public sealed class EntityDbTransaction : IDataTransaction {
        private readonly DbContext context;
        private IDbContextTransaction? contextTransaction;
        private bool disposedValue;

        public EntityDbTransaction(DbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        ~EntityDbTransaction() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        private void ThrowIfDisposed() {
            if (disposedValue)
                throw new ObjectDisposedException(GetType().Name);
        }

        private void ThrowIfNotStarted() {
            if (contextTransaction == null)
                throw new RepositoryException("The transaction was not started");
        }

        public async Task BeginAsync(CancellationToken cancellationToken = default) {
            ThrowIfDisposed();

            contextTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public Task CommitAsync(CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return contextTransaction!.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default) {
            ThrowIfDisposed();
            ThrowIfNotStarted();

            return contextTransaction!.RollbackAsync(cancellationToken);
        }

        private void Dispose(bool disposing) {
			DisposeAsync(disposing).GetAwaiter().GetResult();
        }

		private async ValueTask DisposeAsync(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					if (contextTransaction != null)
						await contextTransaction.DisposeAsync();
				}

				contextTransaction = null;
				disposedValue = true;
			}
		}

		public async ValueTask DisposeAsync() {
			await DisposeAsync(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose() {
			DisposeAsync(true).GetAwaiter().GetResult();
        }
    }
}
