using Bogus;

using Finbuckle.MultiTenant;

using Microsoft.Extensions.DependencyInjection;

using MongoDB.Bson;

using MongoFramework;

using Xunit.Abstractions;

namespace Deveel.Data {
	[Collection(nameof(MongoSingleDatabaseCollection))]
	public class MongoRepositoryProviderTests : MongoRepositoryTestSuite<MongoTenantPerson> {
		public MongoRepositoryProviderTests(MongoSingleDatabase mongo, ITestOutputHelper outputHelper) : base(mongo, outputHelper) {
			PersonFaker = new MongoTenantPersonFaker(TenantId);
		}

		protected override Faker<MongoTenantPerson> PersonFaker { get; }

		protected string TenantId { get; } = Guid.NewGuid().ToString("N");

		protected IRepositoryProvider<MongoTenantPerson, ObjectId> RepositoryProvider => 
			Services.GetRequiredService<IRepositoryProvider<MongoTenantPerson, ObjectId>>();

		protected override IRepository<MongoTenantPerson, ObjectId> Repository => 
			RepositoryProvider.GetRepository(TenantId);

		protected override void ConfigureServices(IServiceCollection services) {
			AddRepositoryProvider(services);

			base.ConfigureServices(services);
		}

		protected virtual void AddRepositoryProvider(IServiceCollection services) {
			services.AddMultiTenant<TenantInfo>()
				.WithInMemoryStore(config => {
					config.Tenants.Add(new TenantInfo {
						Id = TenantId,
						Identifier = "test",
						Name = "Test Tenant",
						ConnectionString = ConnectionString
					}) ;
				});

			services.AddRepositoryTenantResolver<TenantInfo>();

			AddMongoDbContext(services);
			
			services.AddRepositoryController();
		}

		protected virtual void AddMongoDbContext(IServiceCollection services) {
			//var builder = services.AddMongoTenantContext();
			//builder.UseTenantConnection();

			services.AddRepositoryProvider<MongoRepositoryProvider<MongoDbTenantContext, MongoTenantPerson, ObjectId>>();
		}

        protected override async Task InitializeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.CreateTenantRepositoryAsync<MongoTenantPerson, ObjectId>(TenantId);

			var repository = await RepositoryProvider.GetRepositoryAsync(TenantId);
			
			await SeedAsync(repository);
		}

		protected override async Task DisposeAsync() {
			var controller = Services.GetRequiredService<IRepositoryController>();
			await controller.DropTenantRepositoryAsync<MongoTenantPerson, ObjectId>(TenantId);
		}
	}
}
