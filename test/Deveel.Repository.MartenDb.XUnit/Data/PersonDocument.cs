using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public class PersonDocument : IPerson {
		[Key]
		public string? Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string? Email { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? PhoneNumber { get; set; }

		IEnumerable<IRelationship> IPerson.Relationships => Relationships;

		public List<PersonRelationship> Relationships { get; set; }

		public DateTimeOffset CreatedAt { get; set; }
	}
}
