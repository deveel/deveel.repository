// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NetTopologySuite.Geometries;

namespace Deveel.Data {
	[Table("people")]
	public class DbPerson : IPerson {
		[Key]
		public Guid? Id { get; set; }

		string? IPerson.Id {
			get => Id.ToString();
			set => Id = value == null ? null : Guid.Parse(value);
		}

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string? Description { get; set; }

		public Point? Location { get; set; }

		public virtual List<DbRelationship>? Relationships { get; set; }

		IEnumerable<IRelationship> IPerson.Relationships
			=> Relationships ?? Enumerable.Empty<IRelationship>();

		public string? Email { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? PhoneNumber { get; set; }
	}

	[Table("person_relationships")]
	public class DbRelationship : IRelationship {
		[Key]
		public Guid Id { get; set; }

		public Guid? PersonId { get; set; }

		public virtual DbPerson Person { get; set; }

		public string Type { get; set; }

		public string FullName { get; set; }
	}
}
