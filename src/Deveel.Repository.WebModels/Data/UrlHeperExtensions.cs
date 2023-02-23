using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Deveel.Data {
	public static class UrlHeperExtensions {
		private static RouteValueDictionary GetRouteValues<TEntity>(object? values, RepositoryPageQueryModel<TEntity>? query)
			where TEntity : class, IEntity {
			var routeValues = new RouteValueDictionary(values);

			if (query != null) {
				foreach (var item in query.RouteValues()) {
					routeValues[item.Key] = item.Value;
				}
			}

			return routeValues;
		}

		public static string? Link<TEntity>(this IUrlHelper urlHelper, string routeName, object? routeValues, RepositoryPageQueryModel<TEntity>? query)
			where TEntity : class, IEntity
			=> urlHelper.Link(routeName, GetRouteValues(routeValues, query));

		public static string? Link<TEntity>(this IUrlHelper urlHelper, string routeName, RepositoryPageQueryModel<TEntity> query)
			where TEntity : class, IEntity
			=> urlHelper.Link(routeName, query);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string actionName, object? routeValues, RepositoryPageQueryModel<TEntity> query, string? protocol = null)
			where TEntity : class, IEntity
			=> urlHelper.Action(actionName, null, GetRouteValues(routeValues, query), protocol);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string action, string? controller, object? routeValues, RepositoryPageQueryModel<TEntity>? query, string? protocol = null)
			where TEntity : class, IEntity
			=> urlHelper.Action(action, controller, GetRouteValues(routeValues, query), protocol);

		public static string? Action<TEntity>(this IUrlHelper urlHelper, string action, string? controller, RepositoryPageQueryModel<TEntity> query, string? protocol = null) 
			where TEntity : class, IEntity
			=> urlHelper.Action(action, controller, null, query, protocol);


		public static RepositoryPageQueryResultBaseModel<TItem>? SetLinks<TItem>(this IUrlHelper urlHelper, RepositoryPageQueryResultBaseModel<TItem>? result, string routeName, object? routeValues)
			where TItem : class, IEntity {

			if (result == null)
				return null;

			//result.First = urlHelper.Link(routeName, new RouteValueDictionary(routeValues) {
			//	{ pageParamName, 1},
			//	{sizeParamName, result.Request.Size}
			//});

			//result.Last = urlHelper.Link(routeName, new RouteValueDictionary(routeValues) {
			//	{pageParamName, result.TotalPages},
			//	{sizeParamName, result.Request.Size}
			//});

			//result.Self = urlHelper.Link(routeName, new RouteValueDictionary(routeValues) {
			//	{pageParamName, result.Request.Page},
			//	{sizeParamName, result.Request.Size}
			//});

			//if (result.Request.Page > 1) {
			//	result.Previous = urlHelper.Link(routeName, new RouteValueDictionary(routeValues) {
			//		{pageParamName, result.Request.Page - 1},
			//		{sizeParamName, result.Request.Size}
			//	});
			//}

			//if (result.Request.Page < result.TotalPages) {
			//	result.Next = urlHelper.Link(routeName, new RouteValueDictionary(routeValues) {
			//		{ pageParamName, result.Request.Page + 1},
			//		{ sizeParamName, result.Request.Size}
			//	});
			//}

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


		public static RepositoryPageQueryResultBaseModel<TItem>? SetActionLinks<TItem>(this IUrlHelper urlHelper, RepositoryPageQueryResultBaseModel<TItem> result, string action, object? routeValues)
			where TItem : class, IEntity
			=> urlHelper.SetActionLinks(result, action, null, routeValues);

		public static RepositoryPageQueryResultBaseModel<TItem>? SetActionLinks<TItem>(this IUrlHelper urlHelper, RepositoryPageQueryResultBaseModel<TItem> result, string action, string? controller, object? routeValues)
			where TItem : class, IEntity {

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
