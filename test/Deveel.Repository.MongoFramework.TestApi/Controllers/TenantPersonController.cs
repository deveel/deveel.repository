using Deveel.Data.Entities;
using Deveel.Data.Mapping;
using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
    [ApiController]
    [Route("tenant")]
    public class TenantPersonController : ControllerBase {
        private readonly TenantPersonRepository repository;

        public TenantPersonController(TenantPersonRepository repository) {
            this.repository = repository;
        }

		[HttpPost("person")]
		[ProducesResponseType(201, Type = typeof(PersonModel))]
		public async Task<IActionResult> Create([FromBody] PersonModel model) {
			var person = model.ToEntity();
			var id = await repository.AddAsync(person, HttpContext.RequestAborted);
			var result = person.ToModel();
			return CreatedAtAction(nameof(Get), new { id }, result);
		}

		[HttpGet("person/{id}")]
		public async Task<IActionResult> Get(string id) {
			var person = await repository.FindByIdAsync(id, HttpContext.RequestAborted);
			if (person == null)
				return NotFound();

			return Ok(person);
		}

        [HttpPost("{tenantId}/person", Name = "createTenantPerson")]
        public async Task<IActionResult> Create(string tenantId, [FromBody] PersonModel model) {
            var person = model.ToEntity();
            person.TenantId = tenantId;
            var id = await repository.AddAsync(person, HttpContext.RequestAborted);
			var result = person.ToModel();
            return CreatedAtAction(nameof(Get), new {tenantId, id }, result);
        }

        [HttpGet("{tenantId}/person/{id}", Name = "getTenantPerson")]
        public async Task<IActionResult> Get(string tenantId, string id) {
            var person = await repository.FindByIdAsync(id, HttpContext.RequestAborted);
            if (person == null)
                return NotFound();

            return Ok(person);
        }
    }
}
