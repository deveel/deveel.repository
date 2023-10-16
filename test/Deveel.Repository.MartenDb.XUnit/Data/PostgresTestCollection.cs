namespace Deveel.Data {
	[CollectionDefinition(nameof(PostgresTestCollection))]
	public class PostgresTestCollection : ICollectionFixture<PostgresDatabase> {
	}
}
