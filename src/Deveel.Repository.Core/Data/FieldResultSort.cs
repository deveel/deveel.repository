using CommunityToolkit.Diagnostics;

namespace Deveel.Data {
	/// <summary>
	/// Describes a sorting rule using a string filter reference
	/// </summary>
	public sealed class FieldResultSort : IResultSort {
		/// <summary>
		/// Constructs the sorting rule using the given field name
		/// </summary>
		/// <param name="fieldName">
		/// The name of the field to sort the results
		/// </param>
		/// <param name="ascending">
		/// Whether the results should be sorted ascending
		/// </param>
		/// <exception cref="ArgumentException">
		/// Thrown if the given <paramref name="fieldName"/> is <c>null</c> or empty.
		/// </exception>
		public FieldResultSort(string fieldName, bool ascending = true) {
			Guard.IsNotNullOrWhiteSpace(fieldName, nameof(fieldName));

			FieldName = fieldName;
			Ascending = ascending;
		}

		IFieldRef IResultSort.Field => new StringFieldRef(FieldName);

		/// <summary>
		/// Gets the name of the field used to sort the results
		/// </summary>
		public string FieldName { get; }


		/// <summary>
		/// Gets a flag indicating whether the results should be
		/// sorted ascending
		/// </summary>
		public bool Ascending { get; }
	}
}
