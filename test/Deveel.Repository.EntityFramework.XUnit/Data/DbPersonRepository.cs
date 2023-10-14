using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public class DbPersonRepository : EntityRepository<DbPerson> {
		public DbPersonRepository(PersonDbContext context, ILogger<EntityRepository<DbPerson>>? logger = null) : base(context, logger) {
		}

		public DbPersonRepository(PersonDbContext context, ITenantInfo? tenantInfo, ILogger<EntityRepository<DbPerson>>? logger = null) : base(context, tenantInfo, logger) {
		}

		public override IQueryable<DbPerson> AsQueryable() => base.Entities.Include(x => x.Relationships);

		protected override async Task<DbPerson> OnEntityFoundByKeyAsync(object key, DbPerson entity, CancellationToken cancellationToken = default) {
			await Context.Entry(entity).Collection(x => x.Relationships).LoadAsync(cancellationToken);

			return entity;
		}

		public Task SetEmailAsync(DbPerson person, string email, CancellationToken cancellationToken = default) {
			person.Email = email;

			return Task.CompletedTask;
		}
	}
}
