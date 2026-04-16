using System.Linq.Expressions;
using Bogus;

using Deveel.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data
{
	[Collection(nameof(SqlUserConnectionCollection))]
	public class UserEntityRepositoryTestSuite : UserRepositoryTestSuite<DbBookWithOwner, Guid, string>
	{
		private readonly SqlTestConnection sql;

		public UserEntityRepositoryTestSuite(ITestOutputHelper? outputHelper, SqlTestConnection sql) 
			: base(outputHelper)
		{
			this.sql = sql;
			BookFaker = new DbBookFaker(UserId);
		}

		protected override Faker<DbBookWithOwner> BookFaker { get; }
        
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

		protected override async ValueTask InitializeAsync()
		{
			var userAccessor = Services.GetRequiredService<IUserAccessor<string>>();
			var options = Services.GetRequiredService<DbContextOptions<BookDbContext>>();
			await using var dbContext = new BookDbContext(options, userAccessor);

			await dbContext.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
			await dbContext.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

			await base.InitializeAsync();
		}
        
		protected override async ValueTask DisposeAsync()
		{
			var userAccessor = Services.GetRequiredService<IUserAccessor<string>>();
			var options = Services.GetRequiredService<DbContextOptions<BookDbContext>>();
			await using var dbContext = new BookDbContext(options, userAccessor);

			dbContext.Books!.RemoveRange(dbContext.Books);
			await dbContext.SaveChangesAsync(true, TestContext.Current.CancellationToken);

			await dbContext.Database.EnsureDeletedAsync();
		}
    }
}
