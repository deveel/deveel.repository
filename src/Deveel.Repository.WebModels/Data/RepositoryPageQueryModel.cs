using System;
using System.Reflection;
using System.Xml.Linq;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data {
	public class RepositoryPageQueryModel : RepositoryPageRequestModelBase {
		public RepositoryPageQueryModel(int page, int size)
			: base(page, size) {
		}

		public RepositoryPageQueryModel() {
		}

		[MinValue(1), FromQuery(Name = "p")]
		public override int? Page { get => base.Page; set => base.Page = value; }

		[MaxValue(150), FromQuery(Name = "c")]
		public override int? Size { get => base.Size; set => base.Size = value; }

		[FromQuery(Name = "s")]
		public virtual List<string>? SortBy { get; set; }

		public virtual string? GetPageParameterName() {
			var attr = GetType().GetProperty(nameof(Page))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		public virtual string? GetSizeParameterName() {
			var attr = GetType().GetProperty(nameof(Size))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		public virtual string? GetSortParameterName() {
			var attr = GetType().GetProperty(nameof(SortBy))?.GetCustomAttribute<FromQueryAttribute>(false);
			return attr?.Name;
		}

		protected override void GetRouteValues(IDictionary<string, object> routeValues) {
			var pageParam = GetPageParameterName();
			if (!string.IsNullOrWhiteSpace(pageParam))
				routeValues[pageParam] = Page;

			var sizeParam = GetSizeParameterName();
			if (!string.IsNullOrWhiteSpace(sizeParam))
				routeValues[sizeParam] = Size;

			var sortParam = GetSortParameterName();
			if (!string.IsNullOrEmpty(sortParam) && SortBy != null)
				routeValues[sortParam] = SortBy?.ToArray();

			base.GetRouteValues(routeValues);
		}

		public IEnumerable<IResultSort>? GetResultSorts() => SortBy?.Select(WebResultSortUtil.ParseSort);

		public virtual void CopyTo(RepositoryPageQueryModel page) {
			if (page == null)
				return;

			var flags = BindingFlags.Instance | BindingFlags.Public;

			var properties = GetType().GetProperties(flags)
				.Where(x => Attribute.IsDefined(x, typeof(FromQueryAttribute)));

			foreach (var property in properties) {
				var otherProperty = page.GetType()
					.GetProperty(property.Name, property.PropertyType);
				if (otherProperty != null && otherProperty.CanWrite)
					otherProperty.SetValue(page, property.GetValue(this, null));
			}
		}

		protected IQueryFilter GetAggregatedFilter(Func<IEnumerable<IQueryFilter>, IQueryFilter>? filterAggregator = null) {
			var pageFilters = PageFilters();
			if (pageFilters != null && pageFilters.Any()) {
				var filterList = pageFilters.ToList();
				if (filterList.Count == 1) {
					return filterList[0];
				} else {
					if (filterAggregator == null)
						throw new ArgumentNullException(nameof(filterAggregator), "The filter aggregator is required when the query has more than one filter");

					return filterAggregator(pageFilters);
				}
			}

			return QueryFilter.Empty;
		}

		public virtual RepositoryPageRequest ToPageRequest(Func<IEnumerable<IQueryFilter>, IQueryFilter>? filterAggregator = null) {
			return new RepositoryPageRequest(Page ?? 1, GetPageSize()) {
				Filter = GetAggregatedFilter(filterAggregator),
				SortBy = PageSort()
			};
		}

	}
}
