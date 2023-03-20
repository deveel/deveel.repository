using System;

namespace Deveel.Data {
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public sealed class VersionAttribute : Attribute {
		public VersionFormat? Format { get; set; }
	}
}
