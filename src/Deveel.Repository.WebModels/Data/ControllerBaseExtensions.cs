using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;


namespace Deveel.Data {
	public static class ControllerBaseExtensions {
		private static string? SetHost(this string? url, string? host) {
			if (String.IsNullOrWhiteSpace(url))
				return null;

			if (!String.IsNullOrWhiteSpace(host)) {
				var linkUri = new UriBuilder(url);
				linkUri.Host = host;
				url = linkUri.ToString();
			}

			return url;
		}

		private static string? FormatLink<TEntity>(IUrlHelper urlHelper, string? host, string routeName, object? routeValues, RepositoryPageQueryModel<TEntity>? query)
			where TEntity : class, IEntity {
			return urlHelper.Link<TEntity>(routeName, routeValues, query).SetHost(host);
		}

		private static string? FormatAction<TEntity>(IUrlHelper urlHelper, string? protocol, string? host, string action, string? controller, object? routValues, RepositoryPageQueryModel<TEntity>? query)
			where TEntity : class, IEntity
			=> urlHelper.Action(action, controller, routValues, query, protocol).SetHost(host);

		public static RepositoryPageQueryResultModel<TItem>? SetPageActionLinks<TItem>(this ControllerBase controller, RepositoryPageQueryResultModel<TItem>? result, string action, object? routeValues = null)
			where TItem : class, IEntity
			=> controller.SetPageActionLinks(result, action, null, routeValues);

		public static RepositoryPageQueryResultModel<TItem>? SetPageActionLinks<TItem>(this ControllerBase controller, RepositoryPageQueryResultModel<TItem>? result, string action, string? controllerName = null, object? routeValues = null)
			where TItem : class, IEntity {
			if (result == null)
				return null;

			var siteConfig = controller.HttpContext?.RequestServices?.GetService<PaginationModelOptions>();
			var urlHelper = controller.Url;
			var host = siteConfig?.Host;
			var protocol = controller.Request.Scheme;

			result.Self = FormatAction(urlHelper, protocol, host, action, controllerName, routeValues, result.Query);

			if (result.HasPages()) {
				result.First = FormatAction(urlHelper, protocol, host, action, controllerName, routeValues, result.FirstPage());
				result.Last = FormatAction(urlHelper, protocol, host, action, controllerName, routeValues, result.LastPage());
			}

			if (result.HasPrevious()) {
				result.Previous = FormatAction(urlHelper, protocol, host, action, controllerName, routeValues, result.PreviousPage());
			}

			if (result.HasNext()) {
				result.Next = FormatAction(urlHelper, protocol, host, action, controllerName, routeValues, result.NextPage());
			}

			return result;
		}

		public static RepositoryPageQueryResultModel<TItem>? SetPageLinks<TItem>(this ControllerBase controller, RepositoryPageQueryResultModel<TItem>? result, string routeName, object? routeValues = null)
			where TItem : class, IEntity {
			var siteConfig = controller.HttpContext?.RequestServices?.GetService<PaginationModelOptions>();

			if (result == null)
				return null;

			var urlHelper = controller.Url;
			var domainName = siteConfig?.Host;

			result.Self = FormatLink(urlHelper, domainName, routeName, routeValues, result.Query);

			if (result.HasPages()) {
				result.First = FormatLink(urlHelper, domainName, routeName, routeValues, result.FirstPage());
				result.Last = FormatLink(urlHelper, domainName, routeName, routeValues, result.LastPage());
			}

			if (result.HasPrevious()) {
				result.Previous = FormatLink(urlHelper, domainName, routeName, routeValues, result.PreviousPage());
			}

			if (result.HasNext()) {
				result.Next = FormatLink(urlHelper, domainName, routeName, routeValues, result.NextPage());
			}

			return result;
		}

	}
}
