using System;
using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public abstract class RepositoryPageQueryResultBaseModel<TItem> : RepositoryPageModel<TItem>
		where TItem : class, IEntity {
		protected RepositoryPageQueryResultBaseModel(RepositoryPageQueryModel<TItem> pageRequest, int totalItems, IEnumerable<TItem>? items = null)
			: base(pageRequest, totalItems, items) {
		}

		protected RepositoryPageQueryResultBaseModel() {
		}

		protected internal RepositoryPageQueryModel<TItem>? PageQuery => (RepositoryPageQueryModel<TItem>?)PageRequest;

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
			var page = (RepositoryPageQueryModel<TItem>)Activator.CreateInstance(PageQuery.GetType());
			PageQuery.CopyTo(page);
			page.Page = number;
			return page;
		}

		public bool HasNext() => (PageQuery?.Page ?? 1) < TotalPages;

		public RepositoryPageQueryModel<TItem>? NextPage() {
			return HasNext() ? MakePage((PageQuery?.Page ?? 1) + 1) : null;
		}

		public bool HasPrevious() => (PageQuery?.Page ?? 1) > 1;

		public RepositoryPageQueryModel<TItem>? PreviousPage() {
			return HasPrevious() ? MakePage((PageQuery?.Page ?? 1) - 1) : null;
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
