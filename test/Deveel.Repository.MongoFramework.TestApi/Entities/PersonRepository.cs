using MongoFramework;

namespace Deveel.Data.Entities {
    public class PersonRepository : MongoRepository<PersonsDbContext, PersonEntity> {
        public PersonRepository(PersonsDbContext context, ILogger<PersonRepository>? logger = null) : base(context, logger) {
        }
    }
}
