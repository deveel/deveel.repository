using Deveel.Data.Entities;
using Deveel.Data.Mapping;
using Deveel.Data.WebModels;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
    [ApiController]
    [Route("{tenantId}")]
    public class TenantPersonController : ControllerBase {
        private readonly TenantPersonRepository repository;

        public TenantPersonController(TenantPersonRepository repository) {
            this.repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(string tenantId, [FromBody] PersonModel model) {
            var person = model.ToEntity();
            person.TenantId = tenantId;
            var result = await repository.CreateAsync(person, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(Get), new {id = result}, person.ToModel());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string tenantId, string id) {
            var person = await repository.FindByIdAsync(id, HttpContext.RequestAborted);
            if (person == null)
                return NotFound();

            return Ok(person);
        }
    }
}
