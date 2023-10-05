namespace Deveel.Data {
	[CollectionDefinition(nameof(SqlConnectionCollection))]
	public class SqlConnectionCollection : ICollectionFixture<SqlTestConnection> {
	}
}
