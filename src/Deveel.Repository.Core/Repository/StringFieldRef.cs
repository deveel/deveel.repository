using System;

namespace Deveel.Repository {
	/// <summary>
	/// References a field of an entity by its name
	/// </summary>
	public sealed class StringFieldRef : IFieldRef {
		/// <summary>
		/// Constructs the reference with the name of the field
		/// </summary>
		/// <param name="fieldName">The name of the field</param>
		/// <exception cref="ArgumentException">
		/// Thrown if the field is null or empty.
		/// </exception>
		public StringFieldRef(string fieldName) {
			if (string.IsNullOrWhiteSpace(fieldName))
				throw new ArgumentException($"'{nameof(fieldName)}' cannot be null or whitespace.", nameof(fieldName));

			FieldName = fieldName;
		}

		/// <summary>
		/// Gets the name of the field referenced
		/// </summary>
		public string FieldName { get; }
	}
}
