namespace Deveel.Data {
	[CollectionDefinition(nameof(SqlConnectionCollection), DisableParallelization = true)]
	public class SqlConnectionCollection : ICollectionFixture<SqlTestConnection> {
	}
}
