// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public class Person {
		[Key]
		public string? Id { get; set; }


		public string FirstName { get; set; }

		public string LastName { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? Email { get; set; }

		public string? Phone { get; set; }
	}
}
