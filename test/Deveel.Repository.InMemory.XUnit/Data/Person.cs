// Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618

namespace Deveel.Data {
	//public interface IPerson {
	//	string? Id { get; }

	//	string FirstName { get; }

	//	string LastName { get; }

	//	DateTime? BirthDate { get; }
	//}

	public class Person : IPerson {
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string? Id { get; set; }

		public string Email { get; set; }

		public DateTime? DateOfBirth { get; set; }

		public string? PhoneNumber { get; set; }
	}
}
