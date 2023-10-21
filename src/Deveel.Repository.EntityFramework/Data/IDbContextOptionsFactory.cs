using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;

namespace Deveel.Data {
	public interface IDbContextOptionsFactory<TContext> where TContext : DbContext {
		DbContextOptions<TContext> Create(ITenantInfo tenantInfo);
	}
}
