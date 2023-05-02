using MongoFramework;

namespace Deveel.Data.Entities {
    public class TenantPersonRepository : MongoRepository<PersonEntity> {
        public TenantPersonRepository(TenantPersonsDbContext context, ILogger<TenantPersonRepository>? logger = null) 
            : base(context, logger) {
        }
    }
}
