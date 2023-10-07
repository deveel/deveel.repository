using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(SqlConnectionCollection))]
	public abstract class EntityRepositoryTestSuite : RepositoryTestSuite<DbPerson> {
		private readonly SqlTestConnection sql;

		public EntityRepositoryTestSuite(SqlTestConnection sql, ITestOutputHelper? testOutput) : base(testOutput) {
			this.sql = sql;
		}

		protected string ConnectionString => sql.Connection.ConnectionString;

		protected override void ConfigureServices(IServiceCollection services) {
			services.AddDbContext<DbContext, PersonDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
				builder.UseSqlite(sql.Connection);
				builder.LogTo(TestOutput!.WriteLine);
			})
				.AddEntityRepository<DbPerson>();

			base.ConfigureServices(services);
		}

		protected override async Task InitializeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync() {
			var options = Services.GetRequiredService<DbContextOptions<PersonDbContext>>();
			using var dbContext = new PersonDbContext(options);

			dbContext.People!.RemoveRange(dbContext.People);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();
		}

		protected override IEnumerable<DbPerson> NaturalOrder(IEnumerable<DbPerson> source) => source.OrderBy(x => x.Id);
	}
}
