using System.ComponentModel.DataAnnotations;

namespace Deveel.Data {
	public abstract class PageQueryResultModel<TItem> : PageQueryResultBaseModel<TItem>
		where TItem : class {
		public PageQueryResultModel(PageQueryModel<TItem> pageRequest, int totalItems, IEnumerable<TItem>? items = null) 
			: base(pageRequest, totalItems, items) {
		}

		public PageQueryResultModel() {
		}

        [Required]
		public virtual PageQueryModel<TItem> Query {
			get => PageQuery ?? throw new InvalidOperationException("The query was not set");
			set => PageQuery = value;
		}
	}
}
