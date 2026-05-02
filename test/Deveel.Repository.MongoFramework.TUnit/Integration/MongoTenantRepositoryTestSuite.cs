using MongoDB.Bson;

namespace Deveel.Data;

[Category("Integration")]
public class MongoTenantRepositoryTestSuite : MultiTenantRepositoryTestSuite<MongoDbTenantInfo, MongoTenantPerson, ObjectId>, IAsyncInitializer, IAsyncDisposable
{
    private MongoSingleDatabase _mongo = default!;

    protected override Faker<MongoTenantPerson> CreatePersonFaker(string tenantId) => new MongoTenantPersonFaker(tenantId);

    protected override MongoDbTenantInfo CreateTenantInfo(string tenantId) => new MongoDbTenantInfo
    {
        Id = Guid.NewGuid().ToString(),
        Identifier = tenantId,
        ConnectionString = _mongo.ConnectionString
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
                builder.UseTenantConnection(_mongo.ConnectionString);
            })
            .AddRepositoryController();

        services.AddRepository<MongoRepository<MongoTenantPerson, ObjectId>>();
    }

    // Explicitly implement IAsyncInitializer to start MongoDB before base initialization
    async Task IAsyncInitializer.InitializeAsync() {
        _mongo = new MongoSingleDatabase();
        await _mongo.StartAsync();

        await base.InitializeAsync();
    }

    protected override async ValueTask OnInitializedAsync()
    {
        foreach (var tenantId in TenantIds)
        {
            await ExecuteInTenantScopeAsync(tenantId, async (IRepositoryController controller) =>
            {
                await controller.CreateRepositoryAsync<MongoTenantPerson, ObjectId>();
            });
        }

        await base.OnInitializedAsync();
    }

    protected override async ValueTask CleanupAsync()
    {
        foreach (var tenantId in TenantIds)
        {
            await ExecuteInTenantScopeAsync(tenantId, async (IRepositoryController controller) =>
            {
                await controller.DropRepositoryAsync<MongoTenantPerson, ObjectId>();
            });
        }
    }

    // Explicitly implement IAsyncDisposable to stop MongoDB after base disposes the scope
    async ValueTask IAsyncDisposable.DisposeAsync() {
        await base.DisposeAsync();
        await _mongo.DisposeAsync();
    }
}


