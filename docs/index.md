# Getting Started

The _Deveel Repository_ is a framework that provides a set of abstractions and implementations of the _[Repository Pattern](https://en.wikipedia.org/wiki/Repository_pattern)_, to simplify the access to data sources in your application.

Below you will find a quick and generic guide to start using the framework in your application.

To learn about the specific usage of the framework, you can read the following documentation:

| Topic | Description |
| ------- | ------------- |
| _[Using the Entity Framework Core Repository](ef-core.md)_ | Learn how to use the Repository pattern with [Entity Framework Core](https://github.com/dotnet/efcore) |
| _[Using the MongoDB Repository](mongodb.md)_ | Accessing [MongoDB](https://mongodb.com) databases through the Repository pattern |
| _[Using the In-Memory Repository](in-memory.md)_ | Interface a volatile and in-process storage using a Repository pattern. |
| _[The Entity Manager](entity-manager.md)_ | Provide your application with a business layer on top of the Repository for additional functions (_logging_, _validation_, _normalization_, _caching_, _event sourcing_, etc.) |
| _[Extending the Repository](custom-repository.md)_ | Learn how to create a custom repository to access your data source, according to your specific data logic |
| _[Multi-Tenancy](multi-tenancy.md)_ | Learn how to use the framework in a multi-tenant application |


## Installation

The framework is based on a _kernel_ package, that provides the basic interfaces and abstractions, and a set of _drivers_ that implement the interfaces to access different data sources.

When installing any of the libraries of the framework, the _kernel_ package (`Deveel.Repository.Core`) will be installed as a dependency, so you don't need to install it explicitly.


### Requirements

Until the version 1.2.5, the library is built on top of the _[.NET 6.0](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)_ and requires a runtime that supports it: ensure that your application is configured to use the latest version of the runtime.

Since that version we have started targetting multiple runtimes of the .NET Framework, and the following table shows the compatibility matrix:

| Version | .NET Runtime |
| ------- | ------------ |
| =< 1.2.2 | .NET 6.0 |
| >= 1.2.5 | .NET 6.0, .NET 7.0 |

### The Kernel Package

All the specific drivers are built on top of the _kernel_ package, that provides the basic interfaces and abstractions to implement the repository pattern.

If you are interested developing a driver for a specific data source, you can use the _kernel_ package as a dependency, and implement the interfaces to access the data source: you will still receive many benefits by using the abstractions provided by the library, simplifying your development and usage.

To install the package, run the following command in the _Package Manager Console_ in your project folder:

```powershell
Install-Package Deveel.Repository.Core
```

or using the _.NET CLI_:

```bash
dotnet add package Deveel.Repository.Core
```

### The Drivers

The library provides a set of drivers to access different data sources, that can be used as a dependency in your project.

| Driver | Package | Description |
| ------ | ------- | ----------- |
| _In-Memory_ | `Deveel.Repository.InMemory` | A very simple implementation of the repository pattern that stores the data in-memory. |
| _MongoDB_ | `Deveel.Repository.MongoFramework` | An implementation of the repository pattern that stores the data in a MongoDB database (using the [MongoFramework](https://github.com/turnersoftware/mongoframework) library). |
| _Entity Framework Core_ | `Deveel.Repository.EntityFramework` | An implementation of the repository pattern that stores the data in a relational database, using the [Entity Framework Core](https://github.com/dotnet/efcore). |
| _MongoDB_ | `Deveel.Repository.MongoFramework` | An implementation of the repository pattern that stores the data in a [MongoDB](https://mongodb.com) database (using the [MongoFramework](https://github.com/turnersoftware/mongoframework) library). |

## Instrumentation

The library provides a set of common extensions to leverage the _[Dependency Injection](https://en.wikipedia.org/wiki/Dependency_injection)_ pattern, and to simplify the registration of the services in the dependency injection container.

To register a repository in the dependency injection container, and be ready to use it in your application, you can use the `AddRepository<TRepository>` extension method of the `IServiceCollection` interface.

For example, if you want to register the default _in-memory_ repository, you can use the following code:

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddRepository<InMemoryRepository<MyEntity>>();
}
```

If you have implemented your own repository, deriving from the `IRepository<TEntity>` interface, or from one of the _drivers-specific_ repositories (eg. `MongoRepository<TEntity>`, `EntityRepository<TEntity>`), or even if you have implemented your own driver, you can register it in the same way:

```csharp
public void ConfigureServices(IServiceCollection services) {
	services.AddRepository<MyCustomRepository>();
}
```

The type of the argument of the method is not the type of the entity, but the type of the repository: the library will use reflection to scan the type itself and find all the generic arguments of the `IRepository<TEntity>` interface, and register the repository in the dependency injection container.

### Consuming the Repository

In fact, after that exmaple call above, you will have the following services available to be injected in your application:

| Service | Description |
| ------- | ----------- |
| `MyCustomRepository` | The repository to access the data. |
| `IRepository<MyEntity>` | The repository to access the data. |
| `IMyCustomRepository` | The abstration that provides custom business logic and implementing the `IRepository<MyEntity>`. |
| `IQueryableRepository<MyEntity>` | The repository to access the data using the LINQ syntax (if the repository implements it). |
| `IPageableRepository<MyEntity>` | The repository to access the data using pagination (if the repository implements it). |
| `IFilterableRepository<MyEntity>` | The repository to access the data using filters (if the repository implements it). |

## Multi-Tenancy

Some scenarios require information and data to be isolated between different _tenants_ of the application.

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

#### The `IRepositoryProvider<TEntity>` interface

The `IRepositoryProvider<TEntity>` exposes a single method that allows to obtain an instance of `IRepository<TEntity>` for a specific tenant.

```csharp
Task<IRepository<TEntity>> GetRepositoryAsync(string tenantId, CancellationToken cancellationToken = default);
```
