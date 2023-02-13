using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public abstract class RepositoryPageModelBase {
		protected RepositoryPageModelBase(RepositoryPageRequestModelBase pageRequest, int totalItems, IList items) {
			PageRequest = pageRequest ?? throw new ArgumentNullException(nameof(pageRequest));
			TotalItems = totalItems;
			Items = items?.Cast<object>().ToList().AsReadOnly();
		}

		protected RepositoryPageRequestModelBase PageRequest { get; }

		/// <summary>
		/// The total number of items available
		/// </summary>
		[Required]
		public int TotalItems { get; }

		/// <summary>
		/// The items returned in the page
		/// </summary>
		public virtual IReadOnlyCollection<object>? Items { get; }

		/// <summary>
		/// The maximum size of items returned in the page
		/// </summary>
		[Required]
		public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageRequest.GetPageSize());

		public RouteValueDictionary RouteValues() => PageRequest.RouteValues();

	}
}
