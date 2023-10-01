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


		string? IPerson.Id => Id.ToEntityId();

		[Column("first_name")]
		public string FirstName { get; set; }

		[Column("last_name")]
		public string LastName { get; set; }

		[Column("birth_date")]
		public DateTime? BirthDate { get; set; }

		[Column("description")]
		public string? Description { get; set; }

		[Column("created_at")]
		public DateTimeOffset? CreatedAtUtc { get; set; }

		[Column("updated_at")]
		public DateTimeOffset? UpdatedAtUtc { get; set; }
	}

	public interface IPerson {
		string? Id { get; }

		public string FirstName { get; }

		public string LastName { get; }

		public DateTime? BirthDate { get; }

		public string? Description { get; }
	}

}
