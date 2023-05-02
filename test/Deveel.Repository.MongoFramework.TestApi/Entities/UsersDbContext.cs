using Finbuckle.MultiTenant;

using MongoFramework;

namespace Deveel.Data.Entities {
	public sealed class UsersDbContext : MongoDbContext {
		public UsersDbContext(IMongoDbConnection<UsersDbContext> connection) 
			: base(connection) {
		}

		protected override void OnConfigureMapping(MappingBuilder mappingBuilder) {
			mappingBuilder.Entity<UserEntity>();
		}
	}
}
