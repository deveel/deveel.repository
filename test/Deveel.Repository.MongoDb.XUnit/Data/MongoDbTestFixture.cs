using System;

using Microsoft.Extensions.Logging.Abstractions;

using Mongo2Go;

namespace Deveel.Data {
	public class MongoDbTestFixture : IDisposable {
		private MongoDbRunner mongo;

		public MongoDbTestFixture() {
			mongo = MongoDbRunner.Start(logger: NullLogger.Instance, singleNodeReplSet: true);
		}

		public string ConnectionString => mongo.ConnectionString;

		public void Dispose() {
			mongo?.Dispose();
		}
	}
}
