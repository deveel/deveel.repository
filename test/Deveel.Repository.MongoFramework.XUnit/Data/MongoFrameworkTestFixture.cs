using System;

using Microsoft.Extensions.Logging.Abstractions;

using Mongo2Go;

namespace Deveel.Data {
	public class MongoFrameworkTestFixture : IDisposable {
		private MongoDbRunner mongo;

		public MongoFrameworkTestFixture() {
			mongo = MongoDbRunner.Start(logger: NullLogger.Instance, singleNodeReplSet: true);
		}

		public string ConnectionString => mongo.ConnectionString;

		public void Dispose() {
			mongo?.Dispose();
		}

	}
}
