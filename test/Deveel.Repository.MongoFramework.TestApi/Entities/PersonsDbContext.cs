﻿using MongoFramework;

namespace Deveel.Data.Entities {
    public class PersonsDbContext : MongoDbContext {
        public PersonsDbContext(IMongoDbConnection<PersonsDbContext> connection) : base(connection) {
        }

        protected override void OnConfigureMapping(MappingBuilder mappingBuilder) {
            mappingBuilder.Entity<PersonEntity>();
        }
    }
}
