using Bogus;

using Deveel.Data.Entities;

using Finbuckle.MultiTenant;
#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data
{
	[Collection(nameof(SqlTenantConnectionCollection))]
	public class EntityTenantRepositoryTestSuite : MultiTenantRepositoryTestSuite<DbTenantInfo, DbTenantPerson, Guid>
	{
		private readonly SqlTestConnection sql;

		public EntityTenantRepositoryTestSuite(SqlTestConnection sql, ITestOutputHelper? testOutput) : base(testOutput)
		{
			this.sql = sql;
		}

		protected override Faker<DbTenantPerson> CreatePersonFaker(string tenantId) => new DbTenantPersonFaker(tenantId);

		protected override DbTenantInfo CreateTenantInfo(string tenantId) => new DbTenantInfo
		{
			Id = tenantId,
			Identifier = tenantId,
			ConnectionString = sql.Connection.ConnectionString
		};

		protected override Guid GeneratePersonId() => Guid.NewGuid();

		protected override void ConfigureServices(IServiceCollection services)
		{
			AddDbContext(services);

			services.AddRepository<DbTenantPersonRepository>();

			base.ConfigureServices(services);
		}

		protected virtual void AddDbContext(IServiceCollection services)
		{
			services.AddDbContext<PersonTenantDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
				builder.UseSqlite(sql.Connection, sqlite => {
					sqlite.UseNetTopologySuite();
				});
			});
		}

		protected override async Task InitializeAsync()
		{
			foreach (var tenantId in TenantIds)
			{
				await ExecuteInTenantScopeAsync(tenantId, async (IMultiTenantContextAccessor tenantContextAccessor, DbContextOptions<PersonTenantDbContext> options) =>
				{
					using var dbContext = new PersonTenantDbContext(tenantContextAccessor, options);

					await dbContext.Database.EnsureCreatedAsync();
				});
			}


			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync()
		{
			foreach (var tenantId in TenantIds)
			{
				await ExecuteInTenantScopeAsync(tenantId, async (IMultiTenantContextAccessor tenantContextAccessor, DbContextOptions<PersonTenantDbContext> options) =>
				{
					using var dbContext = new PersonTenantDbContext(tenantContextAccessor, options);
					dbContext.Persons!.RemoveRange(dbContext.Persons);
					await dbContext.SaveChangesAsync(true);
				});
			}
		}

	}
}
