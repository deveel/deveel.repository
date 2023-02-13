using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deveel.Data {
	public class RepositoryPageQueryResultModel<TItem> : RepositoryPageModel<TItem>
		where TItem : class, IEntity {
		public RepositoryPageQueryResultModel(RepositoryPageQueryModel<TItem> pageRequest, int totalItems, IEnumerable<TItem>? items = null) 
			: base(pageRequest, totalItems, items) {
		}

		[Required]
		public RepositoryPageQueryModel<TItem> Query => (RepositoryPageQueryModel<TItem>)PageRequest;

		/// <summary>
		/// The URL to the current page of the result
		/// </summary>
		[Url]
		public string? Self { get; set; }

		/// <summary>
		/// The URL to the first page in the result
		/// </summary>
		[Url]
		public string? First { get; set; }

		/// <summary>
		/// The URL to the next page of the result
		/// </summary>
		[Url]
		public string? Next { get; set; }

		/// <summary>
		/// The URL to the previous page of the result
		/// </summary>
		[Url]
		public string? Previous { get; set; }

		/// <summary>
		/// The URL to the last page of the result
		/// </summary>
		[Url]
		public string? Last { get; set; }

		private RepositoryPageQueryModel<TItem> MakePage(int number) {
			var page = (RepositoryPageQueryModel<TItem>)Activator.CreateInstance(Query.GetType());
			Query.CopyTo(page);
			page.Page = number;
			return page;
		}

		public bool HasNext() => (Query.Page ?? 1) < TotalPages;

		public RepositoryPageQueryModel<TItem>? NextPage() {
			return HasNext() ? MakePage((Query.Page ?? 1) + 1) : null;
		}

		public bool HasPrevious() => (Query.Page ?? 1) > 1;

		public RepositoryPageQueryModel<TItem>? PreviousPage() {
			return HasPrevious() ? MakePage((Query.Page ?? 1) - 1) : null;
		}

		public bool HasPages() => TotalPages > 0;

		public RepositoryPageQueryModel<TItem>? LastPage() {
			return HasPages() ? MakePage(TotalPages) : null;
		}

		public RepositoryPageQueryModel<TItem>? FirstPage() {
			return HasPages() ? MakePage(1) : null;
		}
	}
}
