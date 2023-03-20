using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public class RepositoryPageModel<TITem> where TITem : class, IEntity {
		public RepositoryPageModel(RepositoryPageRequestModelBase pageRequest, int totalItems, IEnumerable<TITem>? items = null) {
			if (totalItems < 0)
				throw new ArgumentException("The total items must be greater than or equal to zero", nameof(totalItems));

			PageRequest = pageRequest ?? throw new ArgumentNullException(nameof(pageRequest));
			TotalItems = totalItems;
			Items = items?.ToList();
		}

		public RepositoryPageModel() {
		}

        protected RepositoryPageRequestModelBase? PageRequest { get; set; }

		/// <summary>
		/// The total number of items available
		/// </summary>
		[Required]
		public int TotalItems { get; set; }

		/// <summary>
		/// The items returned in the page
		/// </summary>
		public virtual IReadOnlyCollection<TITem>? Items { get; set; }

		/// <summary>
		/// The maximum size of items returned in the page
		/// </summary>
		[Required]
		public int TotalPages => PageRequest == null ? 0 : (int)Math.Ceiling((double)TotalItems / PageRequest.GetPageSize());

		public RouteValueDictionary? RouteValues() => PageRequest?.RouteValues();
	}
}
