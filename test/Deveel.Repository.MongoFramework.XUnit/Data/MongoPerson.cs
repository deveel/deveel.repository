// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using MongoDB.Bson;

namespace Deveel.Data {
	[Table("persons")]
	public class MongoPerson : IPerson, IHaveTimeStamp {
		[Key, Column("_id")]
		public ObjectId Id { get; set; }


		string? IPerson.Id {
			get => Id.ToEntityId();
			set => Id = ObjectId.Parse(value);
		}

		[Column("first_name")]
		public string FirstName { get; set; }

		[Column("last_name")]
		public string LastName { get; set; }

		[Column("birthdate")]
		public DateTime? DateOfBirth { get; set; }

		[Column("description")]
		public string? Description { get; set; }

		[Column("email")]
		public string? Email { get; set; }

		[Column("phone")]
		public string? PhoneNumber { get; set; }

		[Column("created_at")]
		public DateTimeOffset? CreatedAtUtc { get; set; }

		[Column("updated_at")]
		public DateTimeOffset? UpdatedAtUtc { get; set; }

		[Column("relationships")]
		public List<MongoPersonRelationship>? Relationships { get; set; }

		IEnumerable<IRelationship> IPerson.Relationships => Relationships ?? Enumerable.Empty<IRelationship>();
	}

	public class MongoPersonRelationship : IRelationship {
		[Column("type")]
		public string Type { get; set; }

		[Column("full_name")]
		public string FullName { get; set; }
	}
}
