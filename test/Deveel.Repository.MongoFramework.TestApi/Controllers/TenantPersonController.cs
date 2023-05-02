using Deveel.Data.Entities;

using Microsoft.AspNetCore.Mvc;

namespace Deveel.Data.Controllers {
    [ApiController]
    [Route("{tenantId}")]
    public class TenantPersonController : ControllerBase {
        private readonly TenantPersonRepository repository;

        public TenantPersonController(TenantPersonRepository repository) {
            this.repository = repository;
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
