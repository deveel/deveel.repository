using System;

namespace Deveel.Data {
	/// <summary>
	/// Describes a sorting rule for the results of a query
	/// </summary>
	/// <remarks>
	/// Implementations of repositories can use this interface
	/// to form queries to the underlying data store, or
	/// rather to sort the results of a query after the execution,
	/// depending on the nature of the data and the implementation.
	/// </remarks>
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