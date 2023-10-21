# Getting Started

The _Deveel Repository_ is a framework that provides a set of abstractions and implementations of the [_Repository Pattern_](https://en.wikipedia.org/wiki/Repository\_pattern), to simplify the access to data sources in your application.

Below you will find a quick and generic guide to start using the framework in your application.

To learn about the specific usage of the framework, you can read the following documentation:

| Topic                                                      | Description                                                                                                                                                                    |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [_Using the Entity Framework Core Repository_](ef-core.md) | Learn how to use the Repository pattern with [Entity Framework Core](https://github.com/dotnet/efcore)                                                                         |
| [_Using the MongoDB Repository_](mongodb.md)               | Accessing [MongoDB](https://mongodb.com) databases through the Repository pattern                                                                                              |
| [_Using the In-Memory Repository_](in-memory.md)           | Interface a volatile and in-process storage using a Repository pattern.                                                                                                        |
| [_The Entity Manager_](entity-manager/)                    | Provide your application with a business layer on top of the Repository for additional functions (_logging_, _validation_, _normalization_, _caching_, _event sourcing_, etc.) |
| [_Extending the Repository_](custom-repository.md)         | Learn how to create a custom repository to access your data source, according to your specific data logic                                                                      |
| [_Multi-Tenancy_](multi-tenancy.md)                        | Learn how to use the framework in a multi-tenant application                                                                                                                   |

## Installation

The framework is based on a _kernel_ package, that provides the basic interfaces and abstractions, and a set of _drivers_ that implement the interfaces to access different data sources.

When installing any of the libraries of the framework, the _kernel_ package (`Deveel.Repository.Core`) will be installed as a dependency, so you don't need to install it explicitly.

### Requirements

Until the version 1.2.5, the library is built on top of the [_.NET 6.0_](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) and requires a runtime that supports it: ensure that your application is configured to use the latest version of the runtime.

Since that version we have started targetting multiple runtimes of the .NET Framework, and the following table shows the compatibility matrix:

| Version  | .NET Runtime       |
| -------- | ------------------ |
| =< 1.2.2 | .NET 6.0           |
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

| Driver                                | Package                             | Description                                                                                                                                                                    |
| ------------------------------------- | ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| [_In-Memory_](in-memory.md)           | `Deveel.Repository.InMemory`        | A very simple implementation of the repository pattern that stores the data in-memory.                                                                                         |
| [_MongoDB_](mongodb.md)               | `Deveel.Repository.MongoFramework`  | An implementation of the repository pattern that stores the data in a MongoDB database (using the [MongoFramework](https://github.com/turnersoftware/mongoframework) library). |
| [_Entity Framework Core_](ef-core.md) | `Deveel.Repository.EntityFramework` | An implementation of the repository pattern that stores the data in a relational database, using the [Entity Framework Core](https://github.com/dotnet/efcore).                |

## Instrumentation

The library provides a set of common extensions to leverage the [_Dependency Injection_](https://en.wikipedia.org/wiki/Dependency\_injection) pattern, and to simplify the registration of the services in the dependency injection container.

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

In fact, after that example call above, you will have the following services available to be injected into your application:

| Service                           | Description                                                                                      |
| --------------------------------- | ------------------------------------------------------------------------------------------------ |
| `MyCustomRepository`              | The repository to access the data.                                                               |
| `IRepository<MyEntity>`           | The repository to access the data.                                                               |
| `IMyCustomRepository`             | The abstration that provides custom business logic and implementing the `IRepository<MyEntity>`. |
| `IQueryableRepository<MyEntity>`  | The repository to access the data using the LINQ syntax (if the repository implements it).       |
| `IPageableRepository<MyEntity>`   | The repository to access the data using pagination (if the repository implements it).            |
| `IFilterableRepository<MyEntity>` | The repository to access the data using filters (if the repository implements it).               |
