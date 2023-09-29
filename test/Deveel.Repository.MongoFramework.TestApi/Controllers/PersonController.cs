using Deveel.Data.Entities;
using Deveel.Data.Mapping;
using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
    [Route("person")]
    [ApiController]
    [Authorize]
	[Produces("application/json")]
	[Consumes("application/json")]
    public class PersonController : ControllerBase {
        private readonly PersonRepository repository;

        public PersonController(PersonRepository repository) {
            this.repository = repository;
        }

        private string GetTenantId() {
            var tenant = User.FindFirst("tenant")?.Value;
            if (String.IsNullOrWhiteSpace(tenant))
                throw new InvalidOperationException("Tenant not found");

            return tenant;
        }

        [HttpPost]
		[ProducesResponseType(201, Type = typeof(PersonModel))]
        public async Task<IActionResult> Create([FromBody] PersonModel model) {
            var person = model.ToEntity();
            person.TenantId = GetTenantId();

            var id = await repository.AddAsync(person, HttpContext.RequestAborted);
			var result = person.ToModel();

            return CreatedAtAction(nameof(Get), new {id}, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id) {
            var person = await repository.FindByIdAsync(id, HttpContext.RequestAborted);
            if (person == null)
                return NotFound();

            return Ok(person.ToModel());
        }
    }
}
