using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

using MongoFramework;

namespace Deveel.Data {
	[Collection("Mongo Single Database")]
	public class MultiTenantContextTests {
		private readonly MongoFrameworkTestFixture mongo;
		private readonly IServiceProvider serviceProvider;

		private readonly string tenantId = Guid.NewGuid().ToString();

		public MultiTenantContextTests(MongoFrameworkTestFixture mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();
		}

		private async Task ResolveTenant() {
			var accessor = serviceProvider.GetRequiredService<IMultiTenantContextAccessor<MongoTenantInfo>>();
			if (accessor.MultiTenantContext == null) {
				var resolver = serviceProvider.GetRequiredService<ITenantResolver<MongoTenantInfo>>();
				var context = await resolver.ResolveAsync(new object());
				accessor.MultiTenantContext = context;

				serviceProvider.GetRequiredService<ITenantInfo>();
			}
		}

		private void AddRepository(IServiceCollection services) {
			services.AddMultiTenant<MongoTenantInfo>()
				.WithInMemoryStore(config => {
					config.Tenants.Add(new MongoTenantInfo { Id = tenantId, Identifier = "test-tenant", Name = "Test Tenant" });
				})
				.WithStaticStrategy("test-tenant");

			services
				.AddMongoPerTenantConnection(options => options.DefaultConnectionString = mongo.SetDatabase("testdb"))
				.AddMongoTenantContext()
				.AddMongoRepository<MongoPerson>();
		}

		[Fact]
		public async Task GetRepositoryForTenant() {
			// emulate the middleware in ASP.NET
			await ResolveTenant();

			var repository = serviceProvider.GetService<MongoRepository<MongoPerson>>();

			Assert.NotNull(repository);

			var context = GetMember<MongoDbTenantContext>(repository, "Context");
			Assert.NotNull(context);
			Assert.Equal(tenantId, context.TenantId);
		}

		private static TMember? GetMember<TMember>(object obj, string memberName) where TMember : class {
			var members = obj.GetType().GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (members.Length == 0)
				return null;

			if (members[0] is PropertyInfo property)
				return (TMember?) property.GetValue(obj);
			if (members[0] is FieldInfo field)
				return (TMember?) field.GetValue(obj);

			return null;
		}

		protected class MongoPerson : IDataEntity {
			[Key]
			public string Id { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }
		}
	}
}
