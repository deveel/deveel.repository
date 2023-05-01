using Deveel.Data;
using Deveel.Data.Models;
using Deveel.Repository.TestApi.Data;
using Deveel.Repository.TestApi.Models;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Repository.TestApi.Controller {
	[Route("[controller]")]
	[ApiController]
	public class PersonController : ControllerBase {
		private readonly IRepository<PersonEntity> repository;

		public PersonController(IRepository<PersonEntity> repository) {
			this.repository = repository;
		}

		[HttpGet("page", Name = "GetPage")]
		public async Task<IActionResult> Query([FromQuery] PersonPageQueryModel query) {
			var request = query.ToPageRequest<PersonEntity>();
			var page = await ((IPageableRepository<PersonEntity>)repository).GetPageAsync(request, HttpContext.RequestAborted);

			var response = new PersonPageModel{
				Query = query,
				TotalItems = page.TotalItems,
				Items = page.Items?.Select(person => new TestPersonModel {
					Id = person.Id,
					FirstName = person.FirstName,
					LastName = person.LastName,
					BirthDate = person.BirthDate
				}).ToList()
			};

			response.SetActionLinks(this, "Query");

			return Ok(response);
		}
	}
}
