using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Deveel.Data.Entities {
	public class DbPersonRepository : EntityRepository<DbPerson, Guid> {
		public DbPersonRepository(PersonDbContext context, ILogger<EntityRepository<DbPerson, Guid>>? logger = null) 
			: base(context, logger) {
		}

		public override IQueryable<DbPerson> AsQueryable() => base.Entities.Include(x => x.Relationships);

		protected override async Task<DbPerson> OnEntityFoundByKeyAsync(Guid key, DbPerson entity, CancellationToken cancellationToken = default) {
			await Context.Entry(entity).Collection(x => x.Relationships).LoadAsync(cancellationToken);

			return entity;
		}

		public Task SetEmailAsync(DbPerson person, string email, CancellationToken cancellationToken = default) {
			person.Email = email;

			return Task.CompletedTask;
		}
	}
}
