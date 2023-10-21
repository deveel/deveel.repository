using Bogus;
using System;

using Microsoft.Extensions.DependencyInjection;

using MongoFramework;
using System.Data;
using Finbuckle.MultiTenant;
using Microsoft.Extensions.Logging;

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
				builder.UseConnection(tenant!.ConnectionString!);
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

		[Fact]
		public static void AddDefaultMongoRepositoryProvider_SharedConnection() {
			var tenantId = Guid.NewGuid().ToString();

			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Name = "test-tenant",
						Id = tenantId,
						Identifier = tenantId,
						ConnectionString = "mongodb://localhost:27017/testdb"
					});
				});

			services.AddMongoDbContext<MongoDbTenantContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddRepositoryProvider<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<MongoPerson>>());
			Assert.NotNull(provider.GetService<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<MongoPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);

			Assert.IsType<MongoRepository<MongoPerson>>(repository);
		}

		[Fact]
		public static void AddDefaultMongoRepositoryProvider_TenantConnection() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						ConnectionString = "mongodb://localhost:27017/testdb"
					});
				});

			services.AddMongoDbContext<MongoDbTenantContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddRepositoryProvider<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<MongoPerson>>());
			Assert.NotNull(provider.GetService<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<MongoPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsType<MongoRepository<MongoPerson>>(repository);
		}

		// Custom Repository

		[Fact]
		public static void AddCustomMongoRepository() {
			var services = new ServiceCollection();

			services.AddMongoDbContext<MongoDbContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddRepository<MyMongoPersonRepository>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<MyMongoPersonRepository>());
			Assert.NotNull(provider.GetService<MongoRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IPageableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IFilterableRepository<MongoPerson>>());
			Assert.NotNull(provider.GetService<IQueryableRepository<MongoPerson>>());
		}

		[Fact]
		public static void AddCustomMongoRepositoryProvider_SharedConnection() {
			var tenantId = Guid.NewGuid().ToString();

			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Name = "test-tenant",
						Id = tenantId,
						Identifier = tenantId,
						ConnectionString = "mongodb://localhost:27017/testdb"
					});
				});

			services.AddMongoDbContext<MongoDbTenantContext>(builder => {
				builder.UseConnection("mongodb://localhost:27017/testdb");
			});

			services.AddRepositoryProvider<MyMongoPersonRepositoryProvider>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<MyMongoPersonRepositoryProvider>());
			Assert.NotNull(provider.GetService<IRepositoryProvider<MongoPerson>>());
			Assert.NotNull(provider.GetService<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<MongoPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);

			Assert.IsNotAssignableFrom<IMyMongoPersonRepository>(repository);
			Assert.IsType<MongoRepository<MongoPerson>>(repository);
		}

		[Fact]
		public static void AddCustomMongoRepositoryProvider_TenantConnection() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						ConnectionString = "mongodb://localhost:27017/testdb"
					});
				});

			services.AddMongoDbContext<MongoDbTenantContext>((tenant, builder) => {
				builder.UseConnection(tenant!.ConnectionString!);
			});

			services.AddRepositoryProvider<MyMongoPersonRepositoryProvider>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<MyMongoPersonRepositoryProvider>());
			Assert.NotNull(provider.GetService<IRepositoryProvider<MongoPerson>>());
			Assert.NotNull(provider.GetService<MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<MongoPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsNotAssignableFrom<IMyMongoPersonRepository>(repository);
			Assert.IsType<MongoRepository<MongoPerson>>(repository);
		}

		interface IMyMongoPersonRepository : IRepository<MongoPerson> {
		}

		interface IMyMongoPersonRepositoryProvider : IRepositoryProvider<MongoPerson> {
			new Task<IMyMongoPersonRepository> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken);
		}

		class MyMongoPersonRepository : MongoRepository<MongoPerson>, IMyMongoPersonRepository {
			public MyMongoPersonRepository(IMongoDbContext context, ILogger<MyMongoPersonRepository>? logger = null) : base(context, logger) {
			}
		}

		class MyMongoPersonRepositoryProvider : MongoRepositoryProvider<MongoDbTenantContext, MongoPerson, TenantInfo>, IMyMongoPersonRepositoryProvider {
			public MyMongoPersonRepositoryProvider(IEnumerable<IMultiTenantStore<TenantInfo>> tenantStores, ILoggerFactory? loggerFactory = null) 
				: base(tenantStores, loggerFactory) {
			}

			public new async Task<IMyMongoPersonRepository> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken) {
				return (MyMongoPersonRepository) await base.GetRepositoryAsync(tenantId, cancellationToken);
			}
		}
	}
}
