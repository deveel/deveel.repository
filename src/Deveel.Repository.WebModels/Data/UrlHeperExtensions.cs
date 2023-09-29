using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public static class UrlHeperExtensions {
		private static RouteValueDictionary GetRouteValues<TEntity>(object? values, PageQueryModel<TEntity>? query)
			where TEntity : class {
			var routeValues = new RouteValueDictionary(values);

			if (query != null) {
				foreach (var item in query.RouteValues()) {
					routeValues[item.Key] = item.Value;
				}
			}

			return routeValues;
		}

		public static string? Link<TEntity>(this IUrlHelper urlHelper, string routeName, object? routeValues, PageQueryModel<TEntity>? query)
			where TEntity : class
			=> urlHelper.Link(routeName, GetRouteValues(routeValues, query));

		public static string? Link<TEntity>(this IUrlHelper urlHelper, string routeName, PageQueryModel<TEntity> query)
			where TEntity : class
			=> urlHelper.Link(routeName, query);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string actionName, object? routeValues, PageQueryModel<TEntity> query, string? protocol = null)
			where TEntity : class
			=> urlHelper.Action(actionName, null, GetRouteValues(routeValues, query), protocol);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string action, string? controller, object? routeValues, PageQueryModel<TEntity>? query, string? protocol = null)
			where TEntity : class
			=> urlHelper.Action(action, controller, GetRouteValues(routeValues, query), protocol);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string action, string? controller, PageQueryModel<TEntity> query, string? protocol = null) 
			where TEntity : class
			=> urlHelper.Action(action, controller, null, query, protocol);


		public static PageQueryResultBaseModel<TItem>? SetLinks<TItem>(this IUrlHelper urlHelper, PageQueryResultBaseModel<TItem>? result, string routeName, object? routeValues)
			where TItem : class {

			if (result == null)
				return null;

			result.Self = urlHelper.Link(routeName, result);

			if (result.HasPages()) {
				result.First = urlHelper.Link(routeName, routeValues, result.FirstPage());
				result.Last = urlHelper.Link(routeName, routeValues, result.LastPage());

				if (result.HasPrevious())
					result.Previous = urlHelper.Link(routeName, routeValues, result.PreviousPage());
				if (result.HasNext())
					result.Next = urlHelper.Link(routeName, routeValues, result.NextPage());
			}

			return result;
		}


		public static PageQueryResultBaseModel<TItem>? SetActionLinks<TItem>(this IUrlHelper urlHelper, PageQueryResultBaseModel<TItem> result, string action, object? routeValues)
			where TItem : class
			=> urlHelper.SetActionLinks(result, action, null, routeValues);

		public static PageQueryResultBaseModel<TItem>? SetActionLinks<TItem>(this IUrlHelper urlHelper, PageQueryResultBaseModel<TItem> result, string action, string? controller, object? routeValues)
			where TItem : class {

			if (result == null)
				return null;

			result.Self = urlHelper.Action(action, controller, routeValues, result.PageQuery);

			if (result.HasPages()) {
				result.First = urlHelper.Action(action, controller, routeValues, result.FirstPage());
				result.Last = urlHelper.Action(action, controller, routeValues, result.LastPage());

				if (result.HasPrevious())
					result.Previous = urlHelper.Action(action, controller, routeValues, result.PreviousPage());
				if (result.HasNext())
					result.Next = urlHelper.Action(action, controller, routeValues, result.NextPage());
			}

			return result;
		}

	}
}
