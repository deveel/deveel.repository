using MongoDB.Driver;

namespace Deveel.Data {
    class MongoTransactionFactory : IDataTransactionFactory {
        private readonly MongoSessionProvider sessionProvider;

        public MongoTransactionFactory(MongoSessionProvider sessionProvider) {
            this.sessionProvider = sessionProvider;
        }

        public async Task<IDataTransaction> CreateTransactionAsync(CancellationToken cancellationToken) {
            var session = await sessionProvider.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
            return new MongoTransaction(session);
        }
    }
}