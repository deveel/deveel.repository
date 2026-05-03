# Entity Framework Core Repository

| Feature | Status | Notes |
| ------- | :----: | ----- |
| Base Repository | ✅ | |
| Filterable | ✅ | |
| Queryable | ✅ | Native EF Core `IQueryable` |
| Pageable | ✅ | |
| Tracking | ✅ | EF Core change tracking |
| Multi-tenant | ✅ | Via [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) |

The _Deveel Repository_ `Deveel.Repository.EntityFramework` package provides an implementation of the repository pattern that uses [Entity Framework Core](https://github.com/dotnet/efcore), enabling access to any relational database that EF Core supports (SQL Server, PostgreSQL, SQLite, MySQL, and others).

The `EntityRepository<TEntity>` class wraps a `DbContext` and exposes the full `IRepository<TEntity>` interface, including filterable, queryable, pageable, and tracking capabilities.

## Installation

```bash
dotnet add package Deveel.Repository.EntityFramework
```

## Registration

You need to register a `DbContext` yourself (using the standard EF Core methods), and then register the repository on top of it. The library does not manage the `DbContext` registration.

```csharp
// Program.cs
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Generic form — registers EntityRepository<MyEntity> and all its interface projections
builder.Services.AddRepository<EntityRepository<MyEntity>>();

// Shortcut form provided by this package
builder.Services.AddEntityRepository<MyEntity>();
```

For a custom repository that derives from `EntityRepository<TEntity>`:

```csharp
builder.Services.AddRepository<MyEntityRepository>();
```

## Multi-tenant Support

Starting from version 1.4, multi-tenancy in EF Core repositories is handled externally via [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant). The library does not provide its own tenant-isolation logic for EF Core; instead, configure the `DbContext` to use tenant information from the `ITenantInfo` interface:

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

## Querying

`EntityRepository<TEntity>` implements both `IQueryableRepository<TEntity>` and `IFilterableRepository<TEntity>`.

**With lambda expressions (shorthand):**

```csharp
var entities = await repository.FindAllAsync(x => x.Name == "John");
```

**With `ExpressionQueryFilter<TEntity>`:**

```csharp
var filter = new ExpressionQueryFilter<MyEntity>(x => x.Name == "John");
var query   = new Query(filter);
var entities = await repository.FindAllAsync(query);
```

> The EF Core driver only supports `ExpressionQueryFilter<TEntity>` for filtering. Passing any other filter type will throw `NotSupportedException`.

## Notes

- Register your `DbContext` using the standard EF Core `AddDbContext` extension — the repository package does not wrap or replace that registration.
- Refer to the [Entity Framework Core documentation](https://learn.microsoft.com/en-us/ef/core/) for migration and schema configuration details.
- Refer to the [Finbuckle.MultiTenant documentation](https://www.finbuckle.com/MultiTenant) for multi-tenant EF Core configuration.
