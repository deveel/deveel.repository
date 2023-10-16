namespace Deveel.Data {
	public interface IPerson {
		string? Id { get; }

		string FirstName { get; }

		string LastName { get; }

		string? Email { get; }

		DateTime? DateOfBirth { get; }

		string? PhoneNumber { get; }

		IEnumerable<IRelationship> Relationships { get; }
	}
}
