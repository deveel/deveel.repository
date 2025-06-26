using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;

using MongoFramework;

namespace Deveel.Data {
	public static class MongoDbContextTests {
		[Fact]
		public static void AddDefaultContext() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbContext>());
		}

		[Fact]
		public static void AddDefaultContext_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddSingleton<ITenantInfo>(new MongoTenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbContext>());
		}

		[Fact]
		public static void AddDefaultTenantContext_SharedConnection() {
			var services = new ServiceCollection();

			services.AddSingleton<ITenantInfo>(new TenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant"
			});

			services.AddMongoDbContext<MongoDbTenantContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbTenantContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MongoDbTenantContext>>());
		}

		[Fact]
		public static void AddDefaultTenantContext_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbTenantContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddSingleton<ITenantInfo>(new MongoTenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MongoDbTenantContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MongoDbTenantContext>>());
		}

		[Fact]
		public static void AddSimpleCustomContext() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MyMongoContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MyMongoContext>>());
		}

		[Fact]
		public static void AddSimpleCustomContext_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MyMongoContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddSingleton<ITenantInfo>(new MongoTenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MyMongoContext>>());
		}

		[Fact]
		public static void AddCustomContextWithConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MyMongoContextWithConnection>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoContextWithConnection>());
			Assert.NotNull(provider.GetService<IMongoDbConnection>());
		}

		[Fact]
		public static void AddCustomContextWithConnection_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MyMongoContextWithConnection>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddSingleton<ITenantInfo>(new MongoTenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoContextWithConnection>());
			Assert.NotNull(provider.GetService<IMongoDbConnection>());
		}

		[Fact]
		public static void AddCustomTenantContext_SharedConnection() {
			var services = new ServiceCollection();

			services.AddSingleton<ITenantInfo>(new TenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant"
			});

			services.AddMongoDbContext<MyMongoTenantContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoTenantContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MyMongoTenantContext>>());
		}

		[Fact]
		public static void AddCustomTenantContext_TenantConnection() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MyMongoTenantContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddSingleton<ITenantInfo>(new MongoTenantInfo {
				Id = Guid.NewGuid().ToString(),
				Identifier = "test-tenant",
				ConnectionString = "mongodb://localhost:27017/testdb"
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IMongoDbContext>());
			Assert.NotNull(provider.GetService<MyMongoTenantContext>());
			Assert.NotNull(provider.GetService<IMongoDbConnection<MyMongoTenantContext>>());
		}

		class MyMongoContext : MongoDbContext {
			public MyMongoContext(IMongoDbConnection<MyMongoContext> connection) : base(connection) {
			}
		}

		class MyMongoTenantContext : MongoDbTenantContext {
			public MyMongoTenantContext(IMongoDbConnection<MyMongoTenantContext> connection, ITenantInfo tenantInfo) 
				: base(connection, tenantInfo.Id) {
			}
		}

		class MyMongoContextWithConnection : MongoDbContext {
			public MyMongoContextWithConnection(IMongoDbConnection connection) : base(connection) {
			}
		}
	}
}
