using Deveel.Data;
using Deveel.Data.Models;
using Deveel.Repository.TestApi.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deveel.Repository.TestApi.Controller {
	[Route("[controller]")]
	[ApiController]
	public class PersonController : ControllerBase {
		private readonly IRepository<TestPersonModel> repository;

		public PersonController(IRepository<TestPersonModel> repository) {
			this.repository = repository;
		}

		[HttpGet("page")]
		public async Task<IActionResult> Query([FromQuery] PersonPageQueryModel query) {
			var request = query.ToPageRequest();
			var page = await repository.GetPageAsync(request, HttpContext.RequestAborted);
			var response = new RepositoryPageModel<TestPersonModel>(query, page.TotalItems, page.Items);
			return Ok(response);
		}
	}
}
