// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

namespace Deveel.Data {
	public class Person : IPerson {
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string? Id { get; set; }

		public string? Email { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? PhoneNumber { get; set; }

		public DateTimeOffset? CreatedAtUtc { get; set; }

		public DateTimeOffset? UpdatedAtUtc { get; set; }

		public List<PersonRelationship> Relationships { get; set; }

		IEnumerable<IRelationship> IPerson.Relationships => Relationships ?? Enumerable.Empty<IRelationship>();
	}

	public class PersonRelationship : IRelationship {
		public string Type { get; set; }

		public string FullName { get; set; }
	}
}
