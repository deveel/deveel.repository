namespace Deveel.Data
{
	[System.AttributeUsage(System.AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class DataOwnerAttribute : Attribute
	{
	}
}
