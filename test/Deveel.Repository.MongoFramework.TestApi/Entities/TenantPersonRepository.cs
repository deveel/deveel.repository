using MongoFramework;

namespace Deveel.Data.Entities {
    public class TenantPersonRepository : MongoRepository<TenantPersonsDbContext, PersonEntity> {
        public TenantPersonRepository(TenantPersonsDbContext context, ILogger<TenantPersonRepository>? logger = null) 
            : base(context, null, logger) {
        }
    }
}
