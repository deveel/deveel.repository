using System.Security.Claims;

using Deveel.Data.Entities;
using Deveel.Data.Mapping;
using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
	[Route("user")]
	[ApiController]
	[Authorize]
	public class UserController : ControllerBase {
		private readonly IRepository<UserEntity> repository;

		public UserController(IRepository<UserEntity> repository) {
			this.repository = repository;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserModel))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Create([FromBody] UserModel model) {
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var entity = model.ToEntity();
			entity.TenantId = User.FindFirstValue("tenant");
			var id = await repository.AddAsync(entity);
			var result = entity.ToModel();

			return CreatedAtAction(nameof(Get), new { id }, entity.ToModel());
		}

		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserModel))]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Get(string id) {
			var entity = await repository.FindByIdAsync(id);
			if (entity == null)
				return NotFound();

			return Ok(entity.ToModel());
		}
	}
}
