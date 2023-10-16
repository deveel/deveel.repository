namespace Deveel.Data {
	public record PersonCreated(string? Id = null, string? FirstName = null, string? LastName = null, DateTime? DateOfBirth = null, string? Email = null, string? PhoneNumber = null);

	public record PersonIdChanged(string Id);

	public record PersonNameChanged(string? FirstName = null, string? LastName = null);

	public record PersonEmailChanged(string Email);

	public record RelationshipAdded(string Type, string FullName);

	public record RelationshipRemoved(string Type, string FullName);
}
