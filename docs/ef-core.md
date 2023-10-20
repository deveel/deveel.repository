## Entity Framework Core Repositories

| Feature | Status | Notes
| --- | --- |--- |
| Base Repository | :white_check_mark: | |
| Filterable | :white_check_mark: |  |
| Queryable | :white_check_mark: | _Native EF Core queryable extensions_ |
| Pageable | :white_check_mark: | |
| Multi-tenant | :white_check_mark: | _Uses Finbuckle Multi-Tenant_ |

The _Deveel Repository_ framework provides an implementation of the repository pattern that uses the [Entity Framework Core](https://github.com/dotnet/efcore), and allows to access a wide range of relational databases.

The `EntityRepository<TEntity>` class is an implementation of the repository pattern that wraps around an instance of `DbContext` and provides the basic operations to query and manipulate the data.

To start using instances of the `EntityRepository<TEntity>` class, you need first to register a `DbContext` instance in the dependency injection container, that will be used to access the database, using one of the extensions methods of the `IServiceCollection` interface: you don't receive any special provisioning from the library, and you can use the standard methods provided by the Entity Framework Core itself.

The registration of the repository in the dependency injection container is the same provided by the kernel library, and is the following:

```csharp
services.AddRepository<EntityRepository<MyEntity>>();
services.AddRepository<MyEntityRepository>();
```

or using the shortcut method, that will register the default implementation of the repository:

```csharp
services.AddEntityRepository<MyEntity>();
```

Remember that you still need to register the `DbContext` in the dependency injection container, and that the `EntityRepository<TEntity>` class requires a constructor that accepts an instance of `DbContext` as parameter.

The simplest use case for this is the following set of calls:

```csharp
services.AddDbContext<MyDbContext>(options => options.UseSqlServer("<connection_string>"));
services.AddRepository<EntityRepository<MyEntity>>();
```

The library provides a shortcut method to register the DbContext in multi-tenant applications, using the ITenantInfo interface provided by the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework.

For example:

```csharp
services.AddDbContextForTenant<MyDbContext, TenantInfo>((tenant, options) => options.UseSqlServer(tenant.ConnectionString));
```

**Note**: Please, refer to the official documentation by Microsoft for more information on how to configure the DbContext in your application, and the documentation of the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework for more information on how to configure the multi-tenant support and its support for [Entity Framework Core](https://www.finbuckle.com/MultiTenant/EFCore).

#### Filtering Data

The `EntityRepository<TEntity>` implements both the `IQueryableRepository<TEntity>` and the `IFilterableRepository<TEntity>` interfaces, and allows to query the data only through the `ExpressionFilter<TEntity>` class or through lambda expressions of type `Expression<Func<TEntity, bool>>`.

## Repository Providers

Some scenarios of multi-tenant applications require to have a different repository for each tenant, and to be able to switch between the repositories according to the tenant that is currently active.

The preferred approach of the library is to use the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework to implement multi-tenant applications, and to use the `ITenantInfo` interface to retrieve the current tenant information: this is obtained by scanning the current HTTP request, and retrieving the tenant information from the request.

In some cases, like in background services, where the identity of the tenant is not available through the user (eg. _machine-to-machine_ communication), it is possible to obtain the repository for a specific tenant by using the `IRepositoryProvider<TEntity>` interface: these are still drivers-specific, and produce instances of the repository for a specific tenant and specific driver.
