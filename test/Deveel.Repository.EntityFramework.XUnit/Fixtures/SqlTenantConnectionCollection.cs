namespace Deveel.Data {
	[CollectionDefinition(nameof(SqlTenantConnectionCollection))]
	public class SqlTenantConnectionCollection : ICollectionFixture<SqlTenantTestConnection> {
	}
}
