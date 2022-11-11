using System;

namespace Deveel.Data {
	/// <summary>
	/// Describes the request to obtain a page of a repository of entities,
	/// given a set of filters and a sort order of the results
	/// </summary>
	/// <remarks>
	/// The overall number of pages available in a repository
	/// is given by a formula that divides the total number
	/// of items by the size of the page requested.
	/// </remarks>
	public class PageRequest {
		/// <summary>
		/// Constructs the request with the given page number and size
		/// </summary>
		/// <param name="page">The number of the page to request</param>
		/// <param name="size">The maximum number of items to be returned in the page</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// If either the page number or the page size are smaller than 1.
		/// </exception>
		public PageRequest(int page, int size) {
			if (page < 1)
				throw new ArgumentOutOfRangeException(nameof(page), "The page must be at least the first");
			if (size < 1)
				throw new ArgumentOutOfRangeException(nameof(size), "The size of a page must be of at least one item");

			Page = page;
			Size = size;
		}

		/// <summary>
		/// Gets the number of the page to return
		/// </summary>
		public int Page { get; }

		/// <summary>
		/// Gets the maximum number of items to be returned.
		/// </summary>
		public int Size { get; }

		/// <summary>
		/// Gets the starting offet in the repository where to start
		/// collecting the items to return
		/// </summary>
		public int Offset => (Page - 1) * Size;

		/// <summary>
		/// Gets or sets a filter to restrict the context of the query
		/// </summary>
		public IQueryFilter? Filter { get; set; }

		/// <summary>
		/// Gets or sets an optional set of orders to sort the
		/// result of the request
		/// </summary>
		public IEnumerable<IResultSort>? SortBy { get; set; }
	}
}
