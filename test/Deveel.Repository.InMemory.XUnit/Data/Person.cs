namespace Deveel.Data {
	public interface IPerson {
		string? Id { get; }

		string FirstName { get; }

		string LastName { get; }

		DateTime? BirthDate { get; }
	}

	public class Person : IPerson {
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public DateTime? BirthDate { set; get; }

		public string Id { get; set; }
	}
}
