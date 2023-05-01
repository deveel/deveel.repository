using System;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public abstract class PageQueryResultBaseModel<TItem> : PageResultModel<TItem> where TItem : class {
		protected PageQueryResultBaseModel(PageQueryModel<TItem> pageRequest, int totalItems, IEnumerable<TItem>? items = null)
			: base(pageRequest, totalItems, items) {
		}

		protected PageQueryResultBaseModel() {
		}

		protected internal PageQueryModel<TItem>? PageQuery {
			get => (PageQueryModel<TItem>?)PageRequest;
			set => PageRequest = value;
		}

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

		private PageQueryModel<TItem> MakePage(int number) {
			var page = (PageQueryModel<TItem>)Activator.CreateInstance(PageQuery.GetType());
			PageQuery.CopyTo(page);
			page.Page = number;
			return page;
		}

		public bool HasNext() => (PageQuery?.Page ?? 1) < TotalPages;

		public PageQueryModel<TItem>? NextPage() {
			return HasNext() ? MakePage((PageQuery?.Page ?? 1) + 1) : null;
		}

		public bool HasPrevious() => (PageQuery?.Page ?? 1) > 1;

		public PageQueryModel<TItem>? PreviousPage() {
			return HasPrevious() ? MakePage((PageQuery?.Page ?? 1) - 1) : null;
		}

		public bool HasPages() => TotalPages > 0;

		public PageQueryModel<TItem>? LastPage() {
			return HasPages() ? MakePage(TotalPages) : null;
		}

		public PageQueryModel<TItem>? FirstPage() {
			return HasPages() ? MakePage(1) : null;
		}
	}
}
