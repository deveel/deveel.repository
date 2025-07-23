﻿using Bogus;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;
using MongoDB.Driver.Core.Configuration;

using MongoFramework;

using Xunit.Abstractions;

namespace Deveel.Data
{
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoTenantRepositoryTestSuite : MultiTenantRepositoryTestSuite<MongoDbTenantInfo, MongoTenantPerson, ObjectId>
	{
		private MongoSingleDatabase mongo;

		public MongoTenantRepositoryTestSuite(MongoSingleDatabase mongo, ITestOutputHelper? testOutput)
			: base(testOutput)
		{
			this.mongo = mongo;
		}

		protected override Faker<MongoTenantPerson> CreatePersonFaker(string tenantId) => new MongoTenantPersonFaker(tenantId);

		protected override MongoDbTenantInfo CreateTenantInfo(string tenantId) =>  new MongoDbTenantInfo
		{
			Id = Guid.NewGuid().ToString(),
			Identifier = tenantId,
			ConnectionString = mongo.ConnectionString
		};

		protected override ObjectId GeneratePersonId() => ObjectId.GenerateNewId();

		protected override void ConfigureServices(IServiceCollection services)
		{
			AddRepository(services);

			base.ConfigureServices(services);
		}

		protected virtual void AddRepository(IServiceCollection services)
		{
			services
				.AddMongoDbContext<MongoDbMultiTenantContext>(builder => {
					builder.UseTenantConnection(mongo.ConnectionString);
				})
				.AddRepositoryController();

			services.AddRepository<MongoRepository<MongoTenantPerson, ObjectId>>();
		}

		protected override async Task InitializeAsync()
		{
			foreach (var tenantId in TenantIds)
			{
				await ExecuteInTenantScopeAsync(tenantId, async (IRepositoryController controller) =>
				{
					await controller.CreateRepositoryAsync<MongoTenantPerson, ObjectId>();
				});
			}

			await base.InitializeAsync();
		}

		protected override async Task DisposeAsync()
		{
			foreach (var tenantId in TenantIds)
			{
				await ExecuteInTenantScopeAsync(tenantId, async (IRepositoryController controller) =>
				{
					await controller.DropRepositoryAsync<MongoTenantPerson, ObjectId>();
				});
			}

			await base.DisposeAsync();
		}
	}
}
