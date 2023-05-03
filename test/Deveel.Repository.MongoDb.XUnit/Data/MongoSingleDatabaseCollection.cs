using System;

namespace Deveel.Data {
	[CollectionDefinition("Mongo Single Database", DisableParallelization = true)]
	public class MongoSingleDatabaseCollection : ICollectionFixture<MongoDbTestFixture> {

	}
}
