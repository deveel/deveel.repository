using Deveel.Data.Entities;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public static class DependencyInjectionTests {
		/// <summary>
		/// Returns <c>true</c> when the SpatiaLite native library can be loaded on the
		/// current platform, <c>false</c> otherwise (e.g. macOS without libspatialite).
		/// </summary>
		private static bool IsSpatialiteAvailable() {
			try {
				using var conn = new SqliteConnection("Data Source=:memory:");
				conn.Open();
				conn.EnableExtensions();
				SpatialiteLoader.Load(conn);
				return true;
			} catch {
				return false;
			}
		}

		private static void ConfigurePersonDbContext(DbContextOptionsBuilder builder, bool useSpatialite) {
			if (useSpatialite) {
				builder.UseSqlite("Data Source=:memory:", x => x.UseNetTopologySuite());
			} else {
				builder.UseSqlite("Data Source=:memory:");
				builder.ReplaceService<IModelCustomizer, NonSpatialModelCustomizer>();
			}
		}

        [Fact]
		public static void AddDefaultEntityRepositoryForEntity()
        {
			var services = new ServiceCollection();
			services.AddDbContext<DbContext, PersonDbContext>(options =>
				ConfigurePersonDbContext(options, IsSpatialiteAvailable()));
			services.AddEntityRepository<DbPerson>(ServiceLifetime.Scoped);

			var provider = services.BuildServiceProvider();
			var scope = provider.CreateScope();

			Assert.NotNull(scope.ServiceProvider.GetService<IRepository<DbPerson>>());
			Assert.NotNull(scope.ServiceProvider.GetService<EntityRepository<DbPerson>>());
		}

		[Fact]
		public static void AddEntityRepository() {
			var services = new ServiceCollection();
			services.AddDbContext<DbContext, PersonDbContext>(options =>
				ConfigurePersonDbContext(options, IsSpatialiteAvailable()));
			services.AddRepository<EntityRepository<DbPerson>>(ServiceLifetime.Scoped);

			var provider = services.BuildServiceProvider();
			var scope = provider.CreateScope();

			Assert.NotNull(scope.ServiceProvider.GetService<IRepository<DbPerson>>());
			Assert.NotNull(scope.ServiceProvider.GetService<EntityRepository<DbPerson>>());
		}

		class MyEntityRepositoryWithNoKey : EntityRepository<DbPerson> {
			public MyEntityRepositoryWithNoKey(PersonDbContext context) : base(context) {
			}
		}

		class MyEntityRepository : EntityRepository<DbPerson, Guid> {
			public MyEntityRepository(PersonDbContext context) : base(context) {
			}
		}
	}
}
