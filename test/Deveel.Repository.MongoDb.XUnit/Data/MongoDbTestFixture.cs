using EphemeralMongo;

namespace Deveel.Data {
    public class MongoDbTestFixture : IDisposable {
		private IMongoRunner mongo;

		public MongoDbTestFixture() {
			mongo = MongoRunner.Run(new MongoRunnerOptions {
				UseSingleNodeReplicaSet = true
			});
		}

		public string ConnectionString => mongo.ConnectionString;

		public void Dispose() {
			mongo?.Dispose();
		}
	}
}
