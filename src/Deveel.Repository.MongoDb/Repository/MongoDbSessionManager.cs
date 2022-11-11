using System;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace Deveel.Data {
	class MongoDbSessionManager : IDataTransactionFactory {
		private readonly MongoSessionProvider sessionProvider;

		public MongoDbSessionManager(MongoSessionProvider sessionProvider) {
			this.sessionProvider = sessionProvider;
		}

		public async Task<IDataTransaction> CreateTransactionAsync(CancellationToken cancellationToken) {
			var session = await sessionProvider.StartSessionAsync(new ClientSessionOptions(), cancellationToken);
			return new MongoDbSession(session);
		}
	}
}
