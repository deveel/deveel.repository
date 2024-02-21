using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlConnectionCollection))]
	public class EntityManagementTestSuite : EntityManagerTestSuite<EntityManager<DbPerson, Guid>, DbPerson, Guid> {
		private readonly SqlTestConnection sql;

		public EntityManagementTestSuite(SqlTestConnection sql, ITestOutputHelper testOutput) : base(testOutput) {
			this.sql = sql;
		}

		protected override Faker<DbPerson> PersonFaker { get; } = new DbPersonFaker();

		protected override Guid GenerateKey() => Guid.NewGuid();

		protected override void SetKey(DbPerson person, Guid key) {
			person.Id = key;
		}

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<DbContext, PersonDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
				builder.UseSqlite(sql.Connection, sqlite => {
					sqlite.UseNetTopologySuite();
				});
				builder.EnableSensitiveDataLogging();
			})
			.AddRepository<DbPersonRepository>();

			base.ConfigureServices(services);
		}

		public override async Task InitializeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		public override async Task DisposeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			dbContext.People!.RemoveRange(dbContext.People);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();

			await dbContext.DisposeAsync();

			// await base.DisposeAsync();
		}
	}
}
