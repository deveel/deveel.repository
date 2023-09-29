using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data {
    public static class PageResultExtensions {
		public static void SetLinks<TITem>(this PageQueryResultBaseModel<TITem> result, IUrlHelper urlHelper, string routeName, object? routeValues = null)
			where TITem : class
			=> urlHelper.SetLinks(result, routeName, routeValues);

		public static void SetActionLinks<TItem>(this PageQueryResultBaseModel<TItem> result, IUrlHelper urlHelper, string action, string? controller, object? routeValues = null)
			where TItem : class
			=> urlHelper.SetActionLinks(result, action, controller, routeValues);

		public static void SetActionLinks<TItem>(this PageQueryResultBaseModel<TItem> result, IUrlHelper urlHelper, string action, object? routeValues = null)
			where TItem : class
			=> urlHelper.SetActionLinks(result, action, routeValues);

		public static void SetLinks<TItem>(this PageQueryResultBaseModel<TItem> result, ControllerBase controller, string routeName, object? routeValues = null)
			where TItem : class
			=> controller.SetPageLinks(result, routeName, routeValues);

		public static void SetActionLinks<TItem>(this PageQueryResultBaseModel<TItem> result, ControllerBase controller, string action, string? controllerName, object? routeValues = null)
			where TItem : class
			=> controller.SetPageActionLinks(result, action, controllerName, routeValues);

		public static void SetActionLinks<TItem>(this PageQueryResultBaseModel<TItem> result, ControllerBase controller, string action, object? routeValues = null)
			where TItem : class
			=> controller.SetPageActionLinks(result, action, routeValues);
	}
}
