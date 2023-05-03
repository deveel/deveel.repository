using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data.Entities {
	public sealed class UsersDbContext : MongoDbTenantContext {
		public UsersDbContext(IMongoDbConnection<UsersDbContext> connection, IMultiTenantContext<TenantInfo> tenantContext) 
			: base(connection, tenantContext?.TenantInfo?.Id ?? throw new InvalidOperationException("Cannot determine the tenant")) {
		}

		protected override void OnConfigureMapping(MappingBuilder mappingBuilder) {
			mappingBuilder.Entity<UserEntity>();
		}
	}
}
