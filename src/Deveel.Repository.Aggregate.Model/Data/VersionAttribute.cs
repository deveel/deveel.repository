namespace Deveel.Data {
	/// <summary>
	/// An attribute that is used to identify a property of an
	/// aggregate that is used to obtain its version.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class VersionAttribute : Attribute {
	}
}
