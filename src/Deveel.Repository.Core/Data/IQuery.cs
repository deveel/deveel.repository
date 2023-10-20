namespace Deveel.Data {
	/// <summary>
	/// Defines the contract for a query that can 
	/// be executed against a repository.
	/// </summary>
	public interface IQuery {
		/// <summary>
		/// Gets the filter that is applied to the query
		/// to select the entities.
		/// </summary>
		IQueryFilter? Filter { get; }

		/// <summary>
		/// Gets the sort that is applied to the query
		/// to order the entities.
		/// </summary>
		IQueryOrder? Order { get; }
	}
}
