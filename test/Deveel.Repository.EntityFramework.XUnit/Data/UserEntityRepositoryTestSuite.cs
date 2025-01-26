using Bogus;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Xunit.Abstractions;

namespace Deveel.Data
{
	[Collection(nameof(SqlUserConnectionCollection))]
	public class UserEntityRepositoryTestSuite : UserRepositoryTestSuite<DbBook, Guid, string>
	{
		private readonly SqlTestConnection sql;

		public UserEntityRepositoryTestSuite(ITestOutputHelper? outputHelper, SqlTestConnection sql) 
			: base(outputHelper)
		{
			this.sql = sql;
			BookFaker = new DbBookFaker(UserId);
		}

		protected override Faker<DbBook> BookFaker { get; }

		protected override Guid GenerateBookId() => Guid.NewGuid();

		protected override string GenerateUserId() => Guid.NewGuid().ToString("N");

		protected string ConnectionString => sql.Connection.ConnectionString;

		protected override void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<DbContext, BookDbContext>(builder => {
				builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
				builder.UseSqlite(sql.Connection, sqlite => {
					sqlite.UseNetTopologySuite();
				});
			})
			.AddRepository<DbBookRepository>();

			base.ConfigureServices(services);
		}

		protected override async Task InitializeAsync()
		{
			var userAccessor = Services.GetRequiredService<IUserAccessor<string>>();
			var options = Services.GetRequiredService<DbContextOptions<BookDbContext>>();
			using var dbContext = new BookDbContext(options, userAccessor);

			await dbContext.Database.EnsureDeletedAsync();
			await dbContext.Database.EnsureCreatedAsync();

			await base.InitializeAsync();
		}

		protected override async Task SeedAsync()
		{
			var userAccessor = Services.GetRequiredService<IUserAccessor<string>>();
			var options = Services.GetRequiredService<DbContextOptions<BookDbContext>>();
			using var dbContext = new BookDbContext(options, userAccessor);

			if (Books != null)
			{
				await dbContext.Books.AddRangeAsync(Books);
				await dbContext.SaveChangesAsync(true);
			}
		}

		protected override async Task DisposeAsync()
		{
			var userAccessor = Services.GetRequiredService<IUserAccessor<string>>();
			var options = Services.GetRequiredService<DbContextOptions<BookDbContext>>();
			using var dbContext = new BookDbContext(options, userAccessor);

			dbContext.Books!.RemoveRange(dbContext.Books);
			await dbContext.SaveChangesAsync(true);

			await dbContext.Database.EnsureDeletedAsync();
		}

	}
}
