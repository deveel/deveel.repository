using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data {
	public class RepositoryPageQueryModel<TEntity> : RepositoryPageRequestModelBase 
		where TEntity : class, IEntity {
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
				routeValues[pageParam] = Page ?? 1;

			var sizeParam = GetSizeParameterName();
			if (!string.IsNullOrWhiteSpace(sizeParam))
				routeValues[sizeParam] = GetPageSize();

			var sortParam = GetSortParameterName();
			if (!string.IsNullOrEmpty(sortParam) && SortBy != null)
				routeValues[sortParam] = SortBy?.ToArray();

			base.GetRouteValues(routeValues);
		}

		protected override IEnumerable<IResultSort>? GetResultSort() => SortBy?.Select(WebResultSortUtil.ParseSort);

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

		protected virtual Expression<Func<TEntity, bool>>? GetAggregatedFilter(IEnumerable<IQueryFilter> filters) {
			if (filters != null && filters.Any()) {
				Expression<Func<TEntity, bool>>? result = null;
				foreach (var filter in filters) {
					var exp = filter.AsLambda<TEntity>();

					if (result== null) {
						result = exp;
					} else {
						var param = result.Parameters[0];
						var body = Expression.AndAlso(result.Body, exp.Body);
						result = Expression.Lambda<Func<TEntity, bool>>(body, param);
					}
				}

				return result;
			}

			return null;
		}

		public virtual RepositoryPageRequest<TEntity> ToPageRequest() {
			return new RepositoryPageRequest<TEntity>(Page ?? 1, GetPageSize()) {
				Filter = GetAggregatedFilter(PageFilters()),
				SortBy = PageSort()
			};
		}

	}
}
