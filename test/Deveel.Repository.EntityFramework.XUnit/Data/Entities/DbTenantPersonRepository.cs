﻿namespace Deveel.Data.Entities
{
	public sealed class DbTenantPersonRepository : EntityRepository<DbTenantPerson, Guid>
	{
		public DbTenantPersonRepository(PersonTenantDbContext dbContext) : base(dbContext)
		{
		}
	}
}
