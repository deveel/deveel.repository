namespace Deveel.Data {
	[CollectionDefinition(nameof(SqlTenantConnectionCollection), DisableParallelization = true)]
	public class SqlTenantConnectionCollection : SqlConnectionCollection {
	}
}
