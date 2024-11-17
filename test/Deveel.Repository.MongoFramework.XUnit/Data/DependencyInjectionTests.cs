using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data
{
	public static class DependencyInjectionTests {
		[Fact]
		public static void AddDefaultDbContext_DefaultConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbContext>());

			var dbContext = provider.GetService<IMongoDbContext>();

			Assert.IsType<MongoDbContext>(dbContext);

			Assert.NotNull(dbContext);
			Assert.NotNull(dbContext.Connection);
			Assert.IsAssignableFrom<IMongoDbConnection<MongoDbContext>>(dbContext.Connection);

			var connection = Assert.IsType<MongoDbConnection<MongoDbContext>>(dbContext.Connection);

			Assert.NotNull(connection);
			Assert.NotNull(connection.Url);
			Assert.Equal("mongodb://localhost/testdb", connection.Url.ToString());
		}

		[Fact]
		public static void AddDefaultDbContext_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseTenant();
			});

			services.AddSingleton(new MongoDbTenantInfo
			{
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbContext>());

			var dbContext = provider.GetService<IMongoDbContext>();

			Assert.IsType<MongoDbContext>(dbContext);
			Assert.NotNull(dbContext);

			Assert.NotNull(dbContext.Connection);
			Assert.IsAssignableFrom<IMongoDbConnection<MongoDbContext>>(dbContext.Connection);

			var connection = Assert.IsType<MongoDbConnection<MongoDbContext>>(dbContext.Connection);

			Assert.NotNull(connection);
			Assert.NotNull(connection.Url);
			Assert.Equal("mongodb://localhost/testdb", connection.Url.ToString());
		}

		[Fact]
		public static void AddTenantDbContext_DefaultConnection() {
			var services = new ServiceCollection();

			services.AddSingleton(new MongoDbTenantInfo { Id = Guid.NewGuid().ToString() });

			services.AddMongoDbContext<MongoDbTenantContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbTenantContext>());
			Assert.NotNull(provider.GetService<MongoDbTenantContext>());

			var dbContext = provider.GetService<IMongoDbTenantContext>();

			Assert.IsType<MongoDbTenantContext>(dbContext);

			Assert.NotNull(dbContext);
			Assert.NotNull(dbContext.Connection);
			Assert.IsAssignableFrom<IMongoDbConnection<MongoDbTenantContext>>(dbContext.Connection);
			var connection = Assert.IsType<MongoDbConnection<MongoDbTenantContext>>(dbContext.Connection);

			Assert.NotNull(connection);
			Assert.NotNull(connection.Url);
			Assert.Equal("mongodb://localhost/testdb", connection.Url.ToString());
		}

		[Fact]
		public static void AddDefaultMongoRepository() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddRepository<MongoRepository<MongoPerson>>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<MongoRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IPageableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IFilterableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IQueryableRepository<MongoPerson>>());
		}

		// Custom Repository

		[Fact]
		public static void AddCustomMongoRepository() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddRepository<MyMongoPersonRepositoryNoKey>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<MyMongoPersonRepositoryNoKey>());
			Assert.NotNull(provider.GetService<MongoRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IPageableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IFilterableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IQueryableRepository<MongoPerson>>());
		}

		[Fact]
		public static void AddMongoRepositoryProvider()
		{
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder =>
			{
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddMongoRepositoryProvider<MyMongoPersonRepository>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<MyMongoPersonRepository>>());
		}

		[Fact]
		public static void AddWrongRepositoryProvider()
		{
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder =>
			{
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			Assert.Throws<ArgumentException>(() => services.AddMongoRepositoryProvider<MongoPerson>());
		}


		interface IMyMongoPersonRepository : IRepository<MongoPerson> {
		}

		class MyMongoPersonRepositoryNoKey : MongoRepository<MongoPerson>, IMyMongoPersonRepository {
			public MyMongoPersonRepositoryNoKey(IMongoDbContext context, ILogger<MyMongoPersonRepositoryNoKey>? logger = null) : base(context, logger) {
			}
		}

		class MyMongoPersonRepository : MongoRepository<MongoPerson, ObjectId> {
			protected internal MyMongoPersonRepository(IMongoDbContext context, ILogger? logger = null) : base(context, logger) {
			}
		}
	}
}
