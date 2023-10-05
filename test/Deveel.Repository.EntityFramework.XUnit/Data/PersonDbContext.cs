using Microsoft.EntityFrameworkCore;

namespace Deveel.Data {
	public class PersonDbContext : DbContext {
		public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) {
		}

		public DbSet<DbPerson>? People { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<DbPerson>()
				.HasMany(x => x.Relationships)
				.WithOne(x => x.Person)
				.HasForeignKey(x => x.PersonId);

			modelBuilder.Entity<DbPersonRelationship>();
		}
	}
}
