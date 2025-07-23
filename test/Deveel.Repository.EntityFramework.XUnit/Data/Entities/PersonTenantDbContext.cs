using Finbuckle.MultiTenant;
#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Finbuckle.MultiTenant.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace Deveel.Data.Entities
{
	public class PersonTenantDbContext : MultiTenantDbContext
	{
		public PersonTenantDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) 
			: base(multiTenantContextAccessor?.MultiTenantContext?.TenantInfo!, options)
		{
		}

		public virtual DbSet<DbTenantPerson>? Persons { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<DbTenantPerson>()
				.HasMany(x => x.Relationships)
				.WithOne(x => x.Person)
				.HasForeignKey(x => x.PersonId)
				.OnDelete(DeleteBehavior.Cascade);

			modelBuilder.Entity<DbTenantPerson>()
				.IsMultiTenant();

			modelBuilder.Entity<DbRelationship>()
				.HasOne(x => x.Person)
				.WithMany(x => x.Relationships)
				.HasForeignKey(x => x.PersonId)
				.IsRequired(false);
		}
	}
}
