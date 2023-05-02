using Deveel.Data.Entities;
using Deveel.Data.Mapping;
using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
    [Route("")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> Create([FromBody] PersonModel model) {
            var person = model.ToEntity();
            person.TenantId = GetTenantId();

            var result = await repository.CreateAsync(person, HttpContext.RequestAborted);

            return CreatedAtAction(nameof(Get), new {id = result}, person.ToModel());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id) {
            var person = await repository.FindByIdAsync(id, HttpContext.RequestAborted);
            if (person == null)
                return NotFound();

            return Ok(person);
        }
    }
}
