using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data {
    public static class PageResultExtensions {
		public static void SetLinks<TITem>(this RepositoryPageQueryResultBaseModel<TITem> result, IUrlHelper urlHelper, string routeName, object? routeValues = null)
			where TITem : class, IEntity
			=> urlHelper.SetLinks(result, routeName, routeValues);

		public static void SetActionLinks<TItem>(this RepositoryPageQueryResultBaseModel<TItem> result, IUrlHelper urlHelper, string action, string? controller, object? routeValues = null)
			where TItem : class, IEntity
			=> urlHelper.SetActionLinks(result, action, controller, routeValues);

		public static void SetActionLinks<TItem>(this RepositoryPageQueryResultBaseModel<TItem> result, IUrlHelper urlHelper, string action, object? routeValues = null)
			where TItem : class, IEntity
			=> urlHelper.SetActionLinks(result, action, routeValues);

		public static void SetLinks<TItem>(this RepositoryPageQueryResultBaseModel<TItem> result, ControllerBase controller, string routeName, object? routeValues = null)
			where TItem : class, IEntity
			=> controller.SetPageLinks(result, routeName, routeValues);

		public static void SetActionLinks<TItem>(this RepositoryPageQueryResultBaseModel<TItem> result, ControllerBase controller, string action, string? controllerName, object? routeValues = null)
			where TItem : class, IEntity
			=> controller.SetPageActionLinks(result, action, controllerName, routeValues);

		public static void SetActionLinks<TItem>(this RepositoryPageQueryResultBaseModel<TItem> result, ControllerBase controller, string action, object? routeValues = null)
			where TItem : class, IEntity
			=> controller.SetPageActionLinks(result, action, routeValues);
	}
}
