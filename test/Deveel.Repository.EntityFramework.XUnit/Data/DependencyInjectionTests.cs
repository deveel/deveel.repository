using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

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

		[Fact]
		public static void AddDefaultEntityRepositoryProvider_SharedConnection() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new DbTenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						Name = "Test Tenant"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();
			services.AddDbContext<DbContext, PersonDbContext>((sp, options) =>
					options.UseSqlite("Data Source=:memory:", x => x.UseNetTopologySuite()));

			services.AddEntityRepositoryProvider<PersonDbContext, DbPerson>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<DbPerson>>());
			Assert.NotNull(provider.GetService<EntityRepositoryProvider<PersonDbContext, DbPerson>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<DbPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsType<EntityRepository<DbPerson>>(repository);
		}

		[Fact]
		public static void AddDefaultEntityRepositoryProvider_PerTenantConnection() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new DbTenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						Name = "Test Tenant",
						ConnectionString = $"Data Source=:memory:;TenantID={tenantId}"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			services.AddEntityRepositoryProvider<DbPerson, PersonDbContext>((tenant, builder) => {
				builder.UseSqlite(tenant.ConnectionString!, x => x.UseNetTopologySuite());
			});

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<DbPerson>>());
			Assert.NotNull(provider.GetService<EntityRepositoryProvider<PersonDbContext, DbPerson>>());

			var repository = provider.GetRequiredService<IRepositoryProvider<DbPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsType<EntityRepository<DbPerson>>(repository);
		}

		[Fact]
		public static void AddCustomRepositoryProviderWithKey() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new DbTenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						Name = "Test Tenant",
						ConnectionString = $"Data Source=:memory:;TenantID={tenantId}"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			services.AddDbContextOptionsFactory<PersonDbContext>((tenant, builder) => {
				builder.UseSqlite(tenant.ConnectionString!, x => x.UseNetTopologySuite());
			});

			services.AddRepositoryProvider<MyEntityRepositoryProviderWithNoKey>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<DbPerson>>());
			Assert.NotNull(provider.GetService<MyEntityRepositoryProviderWithNoKey>());

			var repository = provider.GetRequiredService<IRepositoryProvider<DbPerson>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsType<MyEntityRepositoryWithNoKey>(repository);
		}

		[Fact]
		public static void AddCustomRepositoryProvider() {
			var tenantId = Guid.NewGuid().ToString();
			var services = new ServiceCollection();
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new DbTenantInfo {
						Id = tenantId,
						Identifier = tenantId,
						Name = "Test Tenant",
						ConnectionString = $"Data Source=:memory:;TenantID={tenantId}"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			services.AddDbContextOptionsFactory<PersonDbContext>((tenant, builder) => {
				builder.UseSqlite(tenant.ConnectionString!, x => x.UseNetTopologySuite());
			});

			services.AddRepositoryProvider<MyEntityRepositoryProvider>();

			var provider = services.BuildServiceProvider();

			Assert.NotNull(provider.GetService<IRepositoryProvider<DbPerson, Guid>>());
			Assert.NotNull(provider.GetService<MyEntityRepositoryProvider>());

			var repository = provider.GetRequiredService<IRepositoryProvider<DbPerson, Guid>>().GetRepository(tenantId);

			Assert.NotNull(repository);
			Assert.IsType<MyEntityRepository>(repository);
		}


		class MyEntityRepositoryWithNoKey : EntityRepository<DbPerson> {
			public MyEntityRepositoryWithNoKey(PersonDbContext context) : base(context) {
			}
		}

		class MyEntityRepositoryProviderWithNoKey : EntityRepositoryProvider<PersonDbContext, DbPerson> {
			public MyEntityRepositoryProviderWithNoKey(IDbContextOptionsFactory<PersonDbContext> factory, IRepositoryTenantResolver tenantResolver, ILoggerFactory? loggerFactory = null) 
				: base(factory, tenantResolver, loggerFactory) {
			}

			protected override EntityRepository<DbPerson> CreateRepository(PersonDbContext dbContext, ITenantInfo tenantInfo) 
				=> new MyEntityRepositoryWithNoKey(dbContext);
		}

		class MyEntityRepository : EntityRepository<DbPerson, Guid> {
			public MyEntityRepository(PersonDbContext context) : base(context) {
			}
		}

		class MyEntityRepositoryProvider : EntityRepositoryProvider<PersonDbContext, DbPerson, Guid> {
			public MyEntityRepositoryProvider(IDbContextOptionsFactory<PersonDbContext> optionsFactory, IRepositoryTenantResolver tenantResolver, ILoggerFactory? loggerFactory = null) : base(optionsFactory, tenantResolver, loggerFactory) {
			}

			public MyEntityRepositoryProvider(DbContextOptions<PersonDbContext> options, IRepositoryTenantResolver tenantResolver, ILoggerFactory? loggerFactory = null) : base(options, tenantResolver, loggerFactory) {
			}

			protected override EntityRepository<DbPerson, Guid> CreateRepository(PersonDbContext dbContext, ITenantInfo tenantInfo)
				=> new MyEntityRepository(dbContext);
		}
	}
}
