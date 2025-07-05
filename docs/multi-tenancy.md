# Multi-Tenancy of Data Sources

Software-as-a-Service (SaaS) applications and Enterprise-level applications often need to segregate data between different _tenants_ of the application, that could be different customers or different departments of the same company.

By default, the _kernel_ library doesn't provides any set of abstractions and implementations to support multi-tenancy in your application, but the single drivers can provide it, accordingly to their specific capabilities.

| Driver | Multi-Tenancy | Notes
| ------ | ------------- |------|
| _In-Memory_ | :x: | |
| _MongoDB_ | :white_check_mark: | |
| _Entity Framework Core_ | :warning: | _Starting from version 1.4, the support was removed, and depdends from Finbuckle MultiTenant for EntityFramework_ |

### The Tenant Context

On a general basis, the tenant context is resolved through the identity of a user of the application, using mechanisms like _claims_ or _roles_ (see at [Finbuckle Multi-Tenant](https://github.com/Finbuckle/Finbuckle.MultiTenant) how this is implemented in ASP.NET Core).

Some scenarios anyway require the access to those segregated information from a _service_ or a _background task_, where the user identity is not available: for this reason the framework provides an abstraction named `IRepositoryProvider<TEntity>` that will be used to resolve the repository to access the data, for the tenant identifier.

To learn more about the usage of the `IRepositoryProvider<TEntity>` interface, you can read the documentation [here](multi-tenancy.md).

## Repository Providers

:warning: _The repository providers are not available anymore starting from the version 1.4 of the Deveel.Repository framework._

The preferred approach of the library is to use the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework to implement multi-tenant applications, and to use the `ITenantInfo` interface to retrieve the current tenant information: this is obtained by scanning the current HTTP request, and retrieving the tenant information from the request.

In some cases, like in background services, where the identity of the tenant is not available through the user (eg. _machine-to-machine_ communication), it is possible to obtain the repository for a specific tenant by using the `IRepositoryProvider<TEntity>` interface: these are still drivers-specific, and produce instances of the repository for a specific tenant and specific driver.


#### The `IRepositoryProvider<TEntity>` interface

The `IRepositoryProvider<TEntity>` exposes a single method that allows to obtain an instance of `IRepository<TEntity>` for a specific tenant.

```csharp
Task<IRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
```

Every driver that supports multi-tenancy will implement this interface, and the `IRepository<TEntity>` instance returned will be specific for the tenant identifier passed as parameter.

## Finbuckle.MultiTenant for Entity Framework Core

Starting from version 1.4, the support for multi-tenancy in Entity Framework Core was removed from the kernel library, and it is now provided by the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) library.

To enable multi-tenancy in a repository that is based on the Entity Framework Core, you need to install the `Finbuckle.MultiTenant.EntityFrameworkCore` package, and configure the `DbContext` to use the `ITenantInfo` interface to retrieve the tenant information.

To do this, you need to add the `Finbuckle.MultiTenant` services in the `ConfigureServices` method of your `Startup` class:

```csharp
services.AddMultiTenant()
	.WithConfigurationStore()
	.WithRouteStrategy();
```
Then, you can use the `ITenantInfo` interface to retrieve the current tenant information in your `DbContext`:

```csharp
public class MyDbContext : DbContext
{
	private readonly IMultiTenantContext _tenantContext;

	public MyDbContext(DbContextOptions<MyDbContext> options, IMultiTenantContext<DbTenantInfo> tenantContext)
		: base(options)
	{
		_tenantContext = tenantContext;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		// Configure the DbContext to use the tenant information
		optionsBuilder.UseSql(_tenantContext.Tenant.ConnectionString);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Use _tenantInfo to configure the model for the specific tenant
	}
}
```

This way, the `DbContext` will be configured to use the tenant information from the `ITenantInfo` interface, and you can use it to access the data for the specific tenant.

You can then use the `IRepository<TEntity>` interface to access the data for the specific tenant as you would normally do:

```csharp
var repository = serviceProvider.GetRequiredService<IRepository<MyEntity>>();

var entity = await repository.FindByIdAsync(entityId, cancellationToken);
```

In fact the data segregation is handled by the `DbContext` and the `ITenantInfo` interface, so you don't need to worry about it in your application code.

## Multi-Tenancy in MongoDB

The repository implementation to interface the MongoDB database in the Deveel.Repository library is based on the [MongoFramework](https://github.com/TurnerSoftware/MongoFramework) project, that provides a set of abstractions to handle multi-tenancy in MongoDB.

Unfortunately, some design limitations from the model used by the project made us to add some additional code to handle multi-tenancy in MongoDB, that is not available in the original project.

To use multi-tenancy in MongoDB, you need to install the `Deveel.Repository.MongoFramework` package, and configure it to be used in your application.

To do this, you need to add the `MongoRepository` service in the `ConfigureServices` method of your `Startup` class:

```csharp
services.AddMultiTenant<MongoDbTenantInfo>()
	.WithConfigurationStore()
	.WithRouteStrategy();

services.AddMongoRepository(options =>
{
	// this will instruct the MongoDB driver 
	// to use the tenant information from the
	// current context
	options.UseTenant();
});
```

Then, you can use the `IRepository<TEntity>` interface to access the data for the specific tenant as you would normally do:

```csharp
var repository = serviceProvider.GetRequiredService<IRepository<MyEntity>>();

var entity = await repository.FindByIdAsync(entityId, cancellationToken);
```