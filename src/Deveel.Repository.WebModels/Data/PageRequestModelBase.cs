using System.Collections.ObjectModel;

using Deveel.Data;

using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public abstract class PageRequestModelBase {
		protected PageRequestModelBase(int page, int size) {
			Page = page;
			Size = size;
		}

		protected PageRequestModelBase() {
		}

		[MinValue(1)]
		public virtual int? Page { get; set; }

		public virtual int? Size { get; set; }

		// TODO: discover the default page size through attributes
		public virtual int GetPageSize() => Size ?? 10;

		public IEnumerable<IQueryFilter> PageFilters() => GetFilters();

		protected virtual IEnumerable<IQueryFilter> GetFilters() {
			var filters = new List<IQueryFilter>();

			GetFilters(filters);

			return filters.AsReadOnly();
		}

		protected virtual void GetFilters(IList<IQueryFilter> filter) {

		}

		public IEnumerable<IResultSort>? PageSort() => GetResultSort();

		protected virtual IEnumerable<IResultSort>? GetResultSort() {
			var sort = new List<IResultSort>();
			GetResultSort(sort);
			return sort.AsReadOnly();
		}

		protected virtual void GetResultSort(IList<IResultSort> sort) {

		}

		protected virtual IDictionary<string, object> GetRouteValues() {
			var routeValues = new Dictionary<string, object>();
			GetRouteValues(routeValues);

			return new ReadOnlyDictionary<string, object>(routeValues);
		}

		protected virtual void GetRouteValues(IDictionary<string, object> routeValues) {
		}

		public RouteValueDictionary RouteValues() {
			var routeValues = new RouteValueDictionary();

			var data = GetRouteValues();
			foreach (var item in data) {
				routeValues.Add(item.Key, item.Value);
			}

			return routeValues;
		}
	}
}
