namespace Deveel.Data;

public class Person {
    public string? Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }
}
