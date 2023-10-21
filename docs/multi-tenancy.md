# Multi-Tenancy of Data Sources

Software-as-a-Service (SaaS) applications and Enterprise-level applications often need to segregate data between different _tenants_ of the application, that could be different customers or different departments of the same company.

By default, the _kernel_ library doesn't provides any set of abstractions and implementations to support multi-tenancy in your application, but the single drivers can provide it, accordingly to their specific capabilities.

| Driver | Multi-Tenancy |
| ------ | ------------- |
| _In-Memory_ | :x: |
| _MongoDB_ | :white_check_mark: |
| _Entity Framework Core_ | :white_check_mark: |

### The Tenant Context

On a general basis, the tenant context is resolved through the identity of a user of the application, using mechanisms like _claims_ or _roles_ (see at [Finbuckle Multi-Tenant](https://github.com/Finbuckle/Finbuckle.MultiTenant) how this is implemented in ASP.NET Core).

Some scenarios anyway require the access to those segregated information from a _service_ or a _background task_, where the user identity is not available: for this reason the framework provides an abstraction named `IRepositoryProvider<TEntity>` that will be used to resolve the repository to access the data, for the tenant identifier.

To learn more about the usage of the `IRepositoryProvider<TEntity>` interface, you can read the documentation [here](multi-tenancy.md).

## Repository Providers

The preferred approach of the library is to use the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework to implement multi-tenant applications, and to use the `ITenantInfo` interface to retrieve the current tenant information: this is obtained by scanning the current HTTP request, and retrieving the tenant information from the request.

In some cases, like in background services, where the identity of the tenant is not available through the user (eg. _machine-to-machine_ communication), it is possible to obtain the repository for a specific tenant by using the `IRepositoryProvider<TEntity>` interface: these are still drivers-specific, and produce instances of the repository for a specific tenant and specific driver.


#### The `IRepositoryProvider<TEntity>` interface

The `IRepositoryProvider<TEntity>` exposes a single method that allows to obtain an instance of `IRepository<TEntity>` for a specific tenant.

```csharp
Task<IRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
```

Every driver that supports multi-tenancy will implement this interface, and the `IRepository<TEntity>` instance returned will be specific for the tenant identifier passed as parameter.