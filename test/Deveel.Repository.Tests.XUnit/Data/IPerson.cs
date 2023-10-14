namespace Deveel.Data {
	public interface IPerson {
		string? Id { get; set; }

		string FirstName { get; set; }

		string LastName { get; set; }

		string? Email { get; set; }

		DateTime? DateOfBirth { get; set; }

		string? PhoneNumber { get; set; }

		IEnumerable<IRelationship> Relationships { get; }
	}
}
