using System;

namespace Deveel.Repository {
	/// <summary>
	/// Describes a sorting rule for the results of a query
	/// </summary>
	public interface IResultSort {
		/// <summary>
		/// Gets a reference to the field used to sort
		/// the results
		/// </summary>
		IFieldRef Field { get; }

		/// <summary>
		/// Gets a flag indicating whether the result
		/// of the query should be sorted ascending, given
		/// the value of the field
		/// </summary>
		bool Ascending { get; }
	}
}
