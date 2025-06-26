﻿using System.Reflection;

using Finbuckle.MultiTenant;

#if NET7_0_OR_GREATER
using Finbuckle.MultiTenant.Abstractions;
#endif

using Microsoft.Extensions.DependencyInjection;

using MongoFramework;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MultiTenantContextTests {
		private readonly MongoSingleDatabase mongo;
		private readonly IServiceProvider serviceProvider;

		private readonly string tenantId = Guid.NewGuid().ToString();

		public MultiTenantContextTests(MongoSingleDatabase mongo) {
			this.mongo = mongo;

			var services = new ServiceCollection();
			AddRepository(services);

			serviceProvider = services.BuildServiceProvider();
		}

#if NET7_0_OR_GREATER
		private async Task ResolveTenant() {
			var accessor = serviceProvider.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
			if (accessor.MultiTenantContext == null) {
				var setter = serviceProvider.GetRequiredService<IMultiTenantContextSetter>();
				var context = new MultiTenantContext<TenantInfo> {
					TenantInfo = new MongoTenantInfo {
						Id = tenantId,
						Identifier = "test-tenant",
						Name = "Test Tenant",
						ConnectionString = mongo.ConnectionString
					}
				};
				setter.MultiTenantContext = context;

				serviceProvider.GetRequiredService<ITenantInfo>();
			}
		}

#else
		private async Task ResolveTenant() {
			var accessor = serviceProvider.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
			if (accessor.MultiTenantContext == null) {
				var resolver = serviceProvider.GetRequiredService<ITenantResolver<TenantInfo>>();
				var context = await resolver.ResolveAsync(new object());
				accessor.MultiTenantContext = context;

				serviceProvider.GetRequiredService<ITenantInfo>();
			}
		}
#endif

		private void AddRepository(IServiceCollection services) {
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(config => {
					config.Tenants.Add(new MongoTenantInfo { 
						Id = tenantId, 
						Identifier = "test-tenant", 
						Name = "Test Tenant",
						ConnectionString = mongo.ConnectionString
					});
				})
				.WithStaticStrategy("test-tenant");

			services.AddSingleton<IMultiTenantContext<TenantInfo>>(_ => {
				return new MultiTenantContext<TenantInfo> {
					TenantInfo = new MongoTenantInfo {
						Id = tenantId,
						Identifier = "test-tenant",
						Name = "Test Tenant",
						ConnectionString = mongo.ConnectionString
					}
				};
			});

			services.AddMongoDbContext<MongoDbTenantContext>((tenant, builder) => builder.UseConnection(tenant!.ConnectionString!));
			services.AddRepository<MongoRepository<MongoPerson>>();
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
				return property.GetValue(obj) as TMember;
			if (members[0] is FieldInfo field)
				return field.GetValue(obj) as TMember;

			return null;
		}
	}
}
