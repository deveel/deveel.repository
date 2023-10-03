using Bogus;
using System;

using Microsoft.Extensions.DependencyInjection;

using MongoFramework;
using System.Data;
using Finbuckle.MultiTenant;

namespace Deveel.Data {
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

			services.AddMongoDbContext<MongoDbContext>((tenant, builder) => {
				builder.UseConnection(tenant.ConnectionString);
			});

			services.AddSingleton<ITenantInfo>(new TenantInfo {
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

			services.AddSingleton<ITenantInfo>(new TenantInfo { Id = Guid.NewGuid().ToString() });

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
	}
}
