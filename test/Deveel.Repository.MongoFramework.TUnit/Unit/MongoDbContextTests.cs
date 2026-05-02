using Deveel.Utils;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;

using MongoFramework;

namespace Deveel.Data;

[Category("Unit")]
public class MongoDbContextTests {
    #region Default Context

    [Test]
    public async Task Should_ResolveDefaultContext_When_DirectConnectionConfigured() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MongoDbContext>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveDefaultContext_When_TenantConnectionUsed() {
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

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MongoDbContext>()).IsNotNull();
    }

    #endregion

    #region MultiTenant Context

    [Test]
    public async Task Should_ResolveMultiTenantContext_When_SharedConnectionConfigured() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant"
        });
        services.AddMongoDbContext<MongoDbMultiTenantContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MongoDbMultiTenantContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MongoDbMultiTenantContext>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveMultiTenantContext_When_TenantConnectionUsed() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MongoDbMultiTenantContext>(builder => builder.UseTenantConnection());
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant",
            ConnectionString = "mongodb://localhost:27017/testdb"
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MongoDbMultiTenantContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MongoDbMultiTenantContext>>()).IsNotNull();
    }

    #endregion

    #region Custom Context

    [Test]
    public async Task Should_ResolveCustomContext_When_SimpleContextRegistered() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MyMongoContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MyMongoContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MyMongoContext>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomContext_When_TenantConnectionConfigured() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MyMongoContext>(builder => builder.UseTenantConnection());
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant",
            ConnectionString = "mongodb://localhost:27017/testdb"
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MyMongoContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MyMongoContext>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveContextWithBaseConnection_When_ContextAcceptsBaseConnectionType() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MyMongoContextWithConnection>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MyMongoContextWithConnection>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomTenantContext_When_SharedConnectionConfigured() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant"
        });
        services.AddMongoDbContext<MyMongoTenantContext>(builder => {
            builder.UseConnection("mongodb://localhost:27017/testdb");
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MyMongoTenantContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MyMongoTenantContext>>()).IsNotNull();
    }

    [Test]
    public async Task Should_ResolveCustomTenantContext_When_TenantConnectionConfigured() {
        // Arrange
        var services = new ServiceCollection();
        services.AddMongoDbContext<MyMongoTenantContext>(builder => builder.UseTenantConnection());
        services.AddMongoTenantContext(new MongoDbTenantInfo {
            Id = Guid.NewGuid().ToString(),
            Identifier = "test-tenant",
            ConnectionString = "mongodb://localhost:27017/testdb"
        });

        // Act
        var provider = services.BuildServiceProvider();

        // Assert
        await Assert.That(provider.GetService<IMongoDbContext>()).IsNotNull();
        await Assert.That(provider.GetService<MyMongoTenantContext>()).IsNotNull();
        await Assert.That(provider.GetService<IMongoDbConnection<MyMongoTenantContext>>()).IsNotNull();
    }

    #endregion

    #region Support Types

    private sealed class MyMongoContext(IMongoDbConnection<MyMongoContext> connection) : MongoDbContext(connection) { }

    private sealed class MyMongoTenantContext(
        IMongoDbConnection<MyMongoTenantContext> connection,
        IMultiTenantContextAccessor multiTenantContextAccessor)
        : MongoDbMultiTenantContext(connection, multiTenantContextAccessor) { }

    private sealed class MyMongoContextWithConnection(IMongoDbConnection connection) : MongoDbContext(connection) { }

    #endregion
}

