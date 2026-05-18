# Multi-Tenancy of Data Sources

Software-as-a-Service (SaaS) applications and Enterprise-level applications often need to segregate data between different _tenants_ of the application, that could be different customers or different departments of the same company.

The preferred approach of the library is to use the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework to implement multi-tenant applications, and to use the `ITenantInfo` interface to retrieve the current tenant information: this is obtained by scanning the current HTTP request, and retrieving the tenant information from the request.

| Driver | Multi-Tenancy | Notes |
| ------ | ------------- | ----- |
| _In-Memory_ | :x: | Not supported |
| _MongoDB_ | :white_check_mark: | Via `Deveel.Repository.MongoFramework.MultiTenant` |
| _Entity Framework Core_ | :white_check_mark: | Via [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) |

## Multi-Tenancy in Entity Framework Core

Multi-tenancy in EF Core repositories is handled externally via [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant). The library does not provide its own tenant-isolation logic for EF Core; instead, configure the `DbContext` to use tenant information from the `ITenantInfo` interface:

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore()
    .WithRouteStrategy();
```

Then wire the tenant connection string into your `DbContext`:

```csharp
public class MyDbContext : DbContext
{
    private readonly IMultiTenantContext<TenantInfo> _tenantContext;

    public MyDbContext(
        DbContextOptions<MyDbContext> options,
        IMultiTenantContext<TenantInfo> tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_tenantContext.TenantInfo!.ConnectionString);
    }
}
```

After that, inject and use `IRepository<MyEntity>` normally — tenant isolation is handled transparently by the `DbContext`.

## Multi-Tenancy in MongoDB

The repository implementation to interface the MongoDB database in the Deveel.Repository library is based on the [MongoFramework](https://github.com/TurnerSoftware/MongoFramework) project, that provides a set of abstractions to handle multi-tenancy in MongoDB.

To use multi-tenancy in MongoDB, you need to install the `Deveel.Repository.MongoFramework.MultiTenant` package, and configure it to be used in your application.

First, configure Finbuckle.MultiTenant:

```csharp
builder.Services.AddMultiTenant<MongoDbTenantInfo>()
    .WithConfigurationStore()
    .WithRouteStrategy("tenant");
```

Then register a tenant-aware MongoDB context (derived from `MongoDbTenantContext`) and the repository:

```csharp
builder.Services.AddMongoDbContext<MyMongoTenantContext>(connectionBuilder =>
    connectionBuilder.UseConnection("mongodb://..."));

builder.Services.AddRepository<MongoRepository<MyEntity>>();
```

The tenant context resolves the correct database connection for each tenant automatically.