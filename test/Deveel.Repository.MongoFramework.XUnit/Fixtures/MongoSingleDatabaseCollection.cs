using System;

namespace Deveel.Data {
	[CollectionDefinition(nameof(MongoSingleDatabaseCollection), DisableParallelization = true)]
	public class MongoSingleDatabaseCollection : ICollectionFixture<MongoSingleDatabase> {

	}
}
