using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

namespace Deveel.Data {
	public static class TenantResolverTests {
		[Fact]
		public static async Task ResolveTenant() {
			var tenantId = Guid.NewGuid().ToString();
			var tenantIdentifier = "tenant1";

			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Id = tenantId,
						Identifier = tenantIdentifier,
						ConnectionString = "Data Source=.;Initial Catalog=tenant1;Integrated Security=True"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			var provider = services.BuildServiceProvider();

			var resolver = provider.GetRequiredService<IRepositoryTenantResolver>();

			var tenant = await resolver.FindTenantAsync(tenantId);

			Assert.NotNull(tenant);
			Assert.Equal(tenantId, tenant.Id);
			Assert.Equal(tenantIdentifier, tenant.Identifier);
			Assert.Equal("Data Source=.;Initial Catalog=tenant1;Integrated Security=True", tenant.ConnectionString);

			tenant = await resolver.FindTenantAsync(tenantIdentifier);

			Assert.NotNull(tenant);
			Assert.Equal(tenantId, tenant.Id);
			Assert.Equal(tenantIdentifier, tenant.Identifier);
			Assert.Equal("Data Source=.;Initial Catalog=tenant1;Integrated Security=True", tenant.ConnectionString);
		}

		[Fact]
		public static async Task ResolveTenantNotExisting() {
			var tenantId = Guid.NewGuid().ToString();
			var tenantIdentifier = "tenant1";

			var services = new ServiceCollection();

			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(options => {
					options.Tenants.Add(new TenantInfo {
						Id = tenantId,
						Identifier = tenantIdentifier,
						ConnectionString = "Data Source=.;Initial Catalog=tenant1;Integrated Security=True"
					});
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			var provider = services.BuildServiceProvider();

			var resolver = provider.GetRequiredService<IRepositoryTenantResolver>();

			var tenant = await resolver.FindTenantAsync("tenant2");

			Assert.Null(tenant);
		}
	}
}
