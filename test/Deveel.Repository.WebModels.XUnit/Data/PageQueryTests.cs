using System.Net.Http.Json;
using System.Web;

using Bogus;

using Deveel.Repository.TestApi.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
    public class PageQueryTests : PageQueryTestBase {
		protected override string RouteName => "page";

	}
}
