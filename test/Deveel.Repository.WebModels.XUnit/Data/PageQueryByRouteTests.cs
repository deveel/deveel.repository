using Bogus;
using Deveel.Repository.TestApi.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public class PageQueryByRouteTests : PageQueryTestBase {
		protected override string RouteName => "pageByRoute";
	}
}
