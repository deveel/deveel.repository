namespace Deveel.Data {
	public class Person : IHaveTimeStamp {
		public string? Id { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set;}

		public DateTime? BirthDate { get; set; }

		public string? Email { get; set; }

		public string? Phone { get; set; }

		public DateTimeOffset? CreatedAtUtc { get; set; }

		public DateTimeOffset? UpdatedAtUtc { get; set; }
	}
}
