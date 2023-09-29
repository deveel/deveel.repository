using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Finbuckle.MultiTenant;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using MongoFramework;

namespace Deveel.Data {
	[MultiTenant, Entity, Table("persons")]
	public class MongoTenantPerson : IPerson, IHaveTenantId, IHaveTimeStamp {
		[BsonId, Key]
		public ObjectId Id { get; set; }

		string? IPerson.Id => Id.ToEntityId();

		[Column("first_name")]
		public string FirstName { get; set; }

		[Column("last_name")]
		public string LastName { get; set; }

		[Column("birth_date")]
		public DateTime? BirthDate { get; set; }

		[Column("description")]
		public string? Description { get; set; }

		[Column("tenant")]
		public string TenantId { get; set; }

		[Column("created_at")]
		public DateTimeOffset? CreatedAtUtc { get; set; }

		[Column("updated_at")]
		public DateTimeOffset? UpdatedAtUtc { get; set; }
	}
}
