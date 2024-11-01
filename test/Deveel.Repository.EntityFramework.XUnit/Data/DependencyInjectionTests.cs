using Finbuckle.MultiTenant;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Deveel.Data {
	public static class DependencyInjectionTests {
		[Fact]
		public static void AddDefaultEntityRepositoryForEntity() {
			var services = new ServiceCollection();
			services.AddDbContext<DbContext, PersonDbContext>(options =>
				options.UseSqlite("Data Source=:memory:", x => x.UseNetTopologySuite()));
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
							options.UseSqlite("Data Source=:memory:", x => x.UseNetTopologySuite()));
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
