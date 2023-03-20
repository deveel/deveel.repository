using Microsoft.EntityFrameworkCore;

namespace Deveel.Data {
    public sealed class EntityTransactionFactory : IDataTransactionFactory {
        private readonly DbContext context;

        public EntityTransactionFactory(DbContext context) {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<IDataTransaction> CreateTransactionAsync(CancellationToken cancellationToken = default) {
            var transaction = new EntityDbTransaction(context);
            return Task.FromResult<IDataTransaction>(transaction);
        }
    }
}
