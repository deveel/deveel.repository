using Deveel.Utils;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using MongoFramework;

namespace Deveel.Data;

[Category("Unit")]
public class DependencyInjectionTests {
    #region MongoDbContext

    [Test]
    public async Task Should_ResolveMongoDbContext_When_DefaultConnectionStringProvided() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<IMongoDbContext>();

        // Assert
        await Assert.That(provider.GetService<MongoDbContext>()).IsNotNull();
        await Assert.That(dbContext).IsNotNull();
        await Assert.That(dbContext).IsTypeOf<MongoDbContext>();
        await Assert.That(dbContext!.Connection).IsNotNull();
        await Assert.That(dbContext.Connection).IsAssignableTo<IMongoDbConnection>();
        var connection = (MongoDbConnection<MongoDbContext>)dbContext.Connection;
        await Assert.That(connection.Url).IsNotNull();
        await Assert.That(connection.Url.ToString()).IsEqualTo("mongodb://localhost/testdb");
    }

    [Test]
    public async Task Should_ResolveMongoDbContext_When_TenantConnectionUsed() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbContext>(builder => builder.UseTenantConnection());
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant",
            ConnectionString = "mongodb://localhost:27017/testdb"
        });

        // Act
        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<IMongoDbContext>();

        // Assert
        await Assert.That(provider.GetService<MongoDbContext>()).IsNotNull();
        await Assert.That(dbContext).IsNotNull();
        await Assert.That(dbContext).IsTypeOf<MongoDbContext>();
        var connection = (MongoDbTenantConnection<MongoDbContext>)dbContext!.Connection;
        await Assert.That(connection.Url).IsNotNull();
        await Assert.That(connection.Url.ToString()).IsEqualTo("mongodb://localhost/testdb");
    }

    [Test]
    public async Task Should_ResolveMultiTenantContext_When_TenantConnectionStringProvided() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant"
        });
        services.AddMongoDbContext<MongoDbMultiTenantContext>(builder => {
            builder.UseTenantConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();
        var dbContext = provider.GetService<IMongoDbTenantContext>();

        // Assert
        await Assert.That(provider.GetService<MongoDbMultiTenantContext>()).IsNotNull();
        await Assert.That(dbContext).IsNotNull();
        await Assert.That(dbContext).IsTypeOf<MongoDbMultiTenantContext>();
        var connection = (MongoDbTenantConnection<MongoDbMultiTenantContext>)dbContext!.Connection;
        await Assert.That(connection.Url).IsNotNull();
        await Assert.That(connection.Url.ToString()).IsEqualTo("mongodb://localhost/testdb");
    }

    #endregion

    #region MongoRepository

    [Test]
    public async Task Should_ResolveAllRepositoryInterfaces_When_DefaultMongoRepositoryRegistered() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        services.AddRepository<MongoRepository<MongoPerson>>();
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<MongoRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IPageableRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IFilterableRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IQueryableRepository<MongoPerson>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomRepository_When_CustomMongoRepositoryRegistered() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        services.AddRepository<MyMongoPersonRepositoryNoKey>();
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<MyMongoPersonRepositoryNoKey>()).IsNotNull();
        await Assert.That(provider.GetService<MongoRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IPageableRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IFilterableRepository<MongoPerson>>()).IsNotNull();
        await Assert.That(provider.GetService<IQueryableRepository<MongoPerson>>()).IsNotNull();
    }

    #endregion

    #region Support Types

    private interface IMyMongoPersonRepository : IRepository<MongoPerson> { }

    private sealed class MyMongoPersonRepositoryNoKey : MongoRepository<MongoPerson>, IMyMongoPersonRepository {
        public MyMongoPersonRepositoryNoKey(IMongoDbContext context, ILogger<MyMongoPersonRepositoryNoKey>? logger = null)
            : base(context, logger) { }
    }

    private sealed class MyMongoPersonRepository : MongoRepository<MongoPerson, ObjectId> {
        public MyMongoPersonRepository(IMongoDbContext context, ILogger? logger = null)
            : base(context, logger) { }
    }

    #endregion
}

