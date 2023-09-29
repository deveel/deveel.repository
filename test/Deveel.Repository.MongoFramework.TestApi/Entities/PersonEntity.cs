using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using MongoFramework;

namespace Deveel.Data.Entities {
    [Table("persons")]
    public class PersonEntity : IHaveTenantId {
        [BsonId, Key]
        public ObjectId Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; }

        [Column("last_name")]
        public string LastName { get; set; }

        [Column("birth_date")]
        public DateTime? BirthDate { get; set; }

        [Column("tenant_id")]
        public string TenantId { get; set; }
    }
}
