using System;

namespace Deveel.Data {
	[CollectionDefinition(nameof(MongoSingleDatabaseCollection))]
	public class MongoSingleDatabaseCollection : ICollectionFixture<MongoSingleDatabase> {

	}
}
