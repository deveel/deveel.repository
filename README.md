[![Apache 2.0](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://www.apache.org/licenses/LICENSE-2.0) [![Repository CI/CD](https://github.com/deveel/deveel.repository/actions/workflows/ci.yml/badge.svg)](https://github.com/deveel/deveel.repository/actions/workflows/ci.yml) [![codecov](https://codecov.io/gh/deveel/deveel.repository/graph/badge.svg?token=5US7L3C7ES)](https://codecov.io/gh/deveel/deveel.repository)

# Deveel Repository

This project wants to provide a _low-ambitions_ / _low-expectations_ implementation of the (_infamous_) _[Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)_ for .NET to support the development of applications that need to access different data sources, using a common interface, respecting the principles of the _[Domain-Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design)_ and the _[SOLID](https://en.wikipedia.org/wiki/SOLID)_ principles.

## Drivers and Motivation

The repository pattern is a well-known pattern in the domain-driven design, that allows to abstract the data access layer from the domain model, providing a clean separation of concerns.

The repository pattern is often used in conjunction with the _unit of work_ pattern, that allows to group a set of operations in a single transaction.

While implementing several projects for my own needs, and while creating some Open-Source projects requiring a certain degree of data persistence, I've found myself implementing the same pattern over and over again, with some minor differences, depending on the data source I was using.

I tried to look for existing solutions that could help me in this task, but I found that most of the existing libraries were either unreliable, either too opinionated, or simply not providing the features I was looking for.

Although this pattern is not applicable to all scenarios (for instance in the case of _event-driven_ applications), I found that it is still a good pattern to use in many cases, and I decided to create this library to provide a simple and reliable implementation of the pattern, that can be used in different scenarios.

### Why Not Just Use Entity Framework Core?

A great advantage from the usage _Entity Framework Core_ is that it provides a set of abstractions that allows to access different data sources, and to use the same LINQ syntax to query the data.

Anyway, design-wise the Entity Framework is closer to an ORM than to a repository pattern, and it doesn't provide a way to abstract the data access layer from the domain model.

Furthermore, the project was started to address the need to access different data sources, and not only relational databases (for example, MongoDB, or in-memory data sources).

## Installation

The framework is based on a _kernel_ package, that provides the basic interfaces and abstractions, and a set of _drivers_ that implement the interfaces to access different data sources.

| Package | NuGet |
| ------- | ----- |
| _Deveel.Repository.Core_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Core.svg)](https://www.nuget.org/packages/Deveel.Repository.Core/) |
| _Deveel.Repository.InMemory_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.InMemory.svg)](https://www.nuget.org/packages/Deveel.Repository.InMemory/) |
| _Deveel.Repository.MongoFramework_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.MongoFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.MongoFramework/) |
| _Deveel.Repository.EntityFramework_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.EntityFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.EntityFramework/) |
| _Deveel.Repository.DynamicLinq_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.DynamicLinq/) |
| _Deveel.Repository.Manager_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager/) |

### The Kernel Package

If you are interested developing a driver for a specific data source, you can use the _kernel_ package as a dependency, and implement the interfaces to access the data source: you will still receive many benefits by using the abstractions provided by the library, simplifying your development and usage.

To install the package, run the following command in the _Package Manager Console_:

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

## Instrumentation

The library provides a set of common extensions to leverage the _[Dependency Injection](https://en.wikipedia.org/wiki/Dependency_injection)_ pattern, and to simplify the registration of the services in the dependency injection container.

To register a repository in the dependency injection container, and be ready to use it in your application, you can use the `AddRepository<TRepository>` extension method of the `IServiceCollection` interface.

For example, if you want to register the default _in-memory_ repository, you can use the following code:

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddRepository<InMemoryRepository<MyEntity>>();
}
```

If you have implemented your own repository, deriving from the `IRepository<TEntity>` interface, or from one of the _drivers-specific_ repositories (eg. `MongoRepository<TEntity>`, `EntityRepository<TEntity>`) you can register it in the same way:

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
| `IQueryableRepository<MyEntity>` | The repository to access the data using the LINQ syntax (if the repository implements it). |
| `IPageableRepository<MyEntity>` | The repository to access the data using pagination (if the repository implements it). |
| `IFilterableRepository<MyEntity>` | The repository to access the data using filters (if the repository implements it). |

## The Repository

The `IRepository<TEntity>` interface is the main interface of the repository pattern, that defines the basic operations to query and manipulate the data.

The interface is defined as:

```csharp
public interface IRepository<TEntity> : where TEntity : class {
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(string id, CancellationToken cancellationToken = default);
}
```

### Querying the Repository

The foundational contract of the repository pattern provides one single method to query the repository, that is the `FindByIdAsync(string)` method: one of the core concepts of a domain-driven design is that entities are identified by a unique identifier, and the repository pattern provides a way to query the repository by the identifier.

Extensions of the repository pattern can provide additional methods to query the repository, using different criteria, and the library provides a set of interfaces that extend the basic repository interface to provide additional querying capabilities.

You can also implement your own methods to query the repository according to the business logic of your application, and the library will provide a set of extension methods to allow you to use the repository in a functional way.

#### The `IQueryFilter` interface

The `IQueryFilter` interface is a marker interface that defines a filter to apply to a query: it doesn't provide any method, and it's up to the repository implementation to define the actual filter.

The `IQueryableRepository<TEntity>` extension and the `PageQuery<TEntity>` class provide by a way to pass filters to the repository.

The library provides a set of predefined filter types that can be used to query the repository, and that can be used to implement your own filters.

| Filter | Description |
| ------ | ----------- |
| `ExpressionFilter<TEntity>` | A filter that is backed by a lambda expression of type `Expression<Func<TEntity, bool>>`. |
| `CombinedFilter` | A filter that combines two or more filters using a logical AND operator. |
| `QueryFilter.Empty` | A filter that doesn't apply any filter to the query. In fact, applying this filter to a query has no effect> the use of it is for combination purposes or for colascing. |

Implementations of the repository might provide additional types of query filters (eg. the `MongoDbRepository<TEntity>` provides a `MongoFilter` that is backed by a MongoDB filter expression).

### Extensions

To enrich the capabilities of operations that can be performed on the data source, the library provides a set of interfaces that extend the `IRepository<TEntity>` interface.

#### IQueryableRepository

The `IQueryableRepository<TEntity>` interface allows to query the repository using the LINQ syntax, as defined in the `System.Linq` namespace.

Such provisioning allows a _mutable_ repository (that implements functions for _Adding_, _Removing_ and _Updating_ entities) to be queried using the LINQ syntax, and to be used in a _functional_ way.

The interface is defined as:

```csharp
public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
    IQueryable<TEntity> AsQueryable();
}
```


#### IPageableRepository

The `IPageableRepository<TEntity>` interface extends the basic repository functions with a function to query the repository using pagination definition.

A page query is defined by a `PageQuery<TEntity>` object, that defines the page number, the page size, and an optional filter to apply to the query, and an optional list of sorting rules to apply to the result.

The interface is defined as:

```csharp
public interface IPageableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
	Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query, CancellationToken cancellationToken = default);
}
```

The `PageQuery<TEntity>` class encapsulates the definition of a page query, and is defined as follows:

```csharp
public class PageQuery<TEntity> where TEntity : class {
    public PageQuery(int page, int pageSize, Expression<Func<TEntity, bool>> filter = null) {
        Page = page;
        PageSize = pageSize;
	}

    public int Page { get; }
    
    public int PageSize { get; }

    public IQueryFilter Filter { get; set; }
    
    public IList<IResultSort> SortBy { get; set; }
}
```

The result of a paged query is an instance of `PageResult<TEntity>`, that is defined as:

```csharp
public class PageResult<TEntity> where TEntity : class {
    public PageResult(PageQuery<TEntity>, int total, IEnumerable<TEntity> items) {
        // ...
    }

    public PageQuery<TEntity> Query { get; }

    public int TotalItems { get; }

    public IReadOnly<TEntity> Items { get; }
}
```

#### IFilterableRepository

The `IFilterableRepository<TEntity>` interface provides some extensions that allows to query the repository using instances of the `IQueryFilter` contract.

It is up to the repository implementation support or not the concrete types of filters, either by throwing an exception, either by ignoring the filter.

The interface is defined as:

```csharp
public interface IFilterableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
    Task<TEntity?> FindAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    Task<IList<TEntity>> FindAllAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    Task<int> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
}
```

## Repository Drivers

The framework provides a set of default drivers to access different data sources, depending on the needs of your application.

| Driver | Package | Description |
| ------ | ------- | ----------- |
| In-Memory | `Deveel.Repository.InMemory` | An implementation of the repository pattern that stores the data in-memory. |
| MongoDB | `Deveel.Repository.MongoDb` | An implementation of the repository pattern that stores the data in a MongoDB database. |
| Entity Framework Core | `Deveel.Repository.EntityFramework` | An implementation of the repository pattern that stores the data in a relational database, using the [Entity Framework Core](https://github.com/dotnet/efcore). |

### In-Memory

The `InMemoryRepository<TEntity>` class is an implementation of the repository pattern that stores the data in-memory.

You can register an instance of the repository in the dependency injection container using the `AddInMemoryRepository<TEntity>` extension method of the `IServiceCollection` interface:

```csharp
public void ConfigureServices(IServiceCollection services) {
	services.AddInMemoryRepository<MyEntity>();
}
```

#### Filtering Data

The `InMemoryRepository<TEntity>` implements both the `IQueryableRepository<TEntity>` and the `IFilterableRepository<TEntity>` interfaces, and allows to query the data using the LINQ syntax, or using instances of the `IQueryFilter` interface.

The only supported filter type is the `ExpressionFilter<TEntity>` that is backed by a lambda expression of type `Expression<Func<TEntity, bool>>`.

### MongoDB

The `MongoDbRepository<TEntity>` class is an implementation of the repository pattern that stores the data in a MongoDB database, using the [MongoFramework](https://github.com/TurnerSoftware/MongoFramework)

MongoFramework is a lightweight .NET Standard library that allows to map .NET objects to MongoDB documents, and provides a set of APIs to query and manipulate the data, using a design that is very similar to the _Entity Framework Core_.

To start using instances of the `MongoRepository<TEntity>` class, you need first to register a `IMongoDbContext` instance in the dependency injection container, that will be used to access the database, using one of the extensions methods of the `IServiceCollection` interface.

The simplest use case for this is the following set of calls:

```csharp
services.AddMongoContext("mongodb://localhost:27017/my_database");
services.AddMongoRepository<MyEntity>();
```

The first call registers an instance of `IMongoDbContext` in the dependency injection container, that will be used by the repository to access the database.

**Note**: By default the _MongoFramework_ library doesn't provide any way to inject the database context in the service collection: this is an extension provided by this implementation of the repository pattern.

The following methods are available to register a MongoDB context in the dependency injection container:

| Method | Description |
| ------ | ----------- |
| `AddMongoContext<TContext>(string, ServiceLifetime)` | Registers a MongoDB context in the dependency injection container, using the connection string provided as argument. |
| `AddMongoContext<TContext>(Action<MongoConnectionBuilder>, ServiceLifetime)` | Registers a MongoDB context of the given type, using the connection configured. |
| `AddMongoContext<TContext>(Action<ITenantInfo, MongoConnectionBuilder>, ServiceLifetime)` | Registers a MongoDB context of the given type, using the connection configured for the given tenant. |


The call to register the repository in the dependency injection container is the same provided by the kernel library, and is the following:

```csharp
services.AddRepository<MongoRepository<MyEntity>>();
```

Additionally, the package provides a shortcut method to register the default implementation of the repository:

```csharp
services.AddMongoRepository<MyEntity>();
```

#### Multi-Tenant Support
**Note**: The multi-tenant support is provided by the [Finbuckle.MultiTenant](https://github.com/Finbuckle/Finbuckle.MultiTenant) framework, and you need to first register the `TenantInfo` class in the dependency injection container.

For example, using the following call:

```csharp
services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore()
    .WithRouteStrategy("tenant");
```

#### Filtering Data

The `MongoDbRepository<TEntity>` implements both the `IQueryableRepository<TEntity>` and the `IFilterableRepository<TEntity>` interfaces, and allows to query the data using the LINQ syntax, or using instances of the `IQueryFilter` interface.

The library provides a `MongoFilter` class that is backed by a MongoDB filter expression, and can be used to query the data, and that is convertible to a MongoDB filter expression.

It is also possible to filter by using lambda expressions of type `Expression<Func<TEntity, bool>>` or by using the `ExpressionFilter<TEntity>` class.


## Entity Framework Core

This framework provides an implementation of the repository pattern that uses the [Entity Framework Core](https://github.com/dotnet/efcore), and allows to access a wide range of relational databases.

The `EntityRepository<TEntity>` class is an implementation of the repository pattern that wraps around an instance of `DbContext` and provides the basic operations to query and manipulate the data.

To start using instances of the `EntityRepository<TEntity>` class, you need first to register a `DbContext` instance in the dependency injection container, that will be used to access the database, using one of the extensions methods of the `IServiceCollection` interface: you don't receive any special provisioning from the library, and you can use the standard methods provided by the Entity Framework Core itself.

The simplest use case for this is the following set of calls:

```csharp
services.AddDbContext<MyDbContext>(options => options.UseSqlServer("<connection_string>"));
services.AddRepository<EntityRepository<MyEntity>>();
```

The library provides a shortcut method to register the DbContext in multi-tenant applications, using the ITenantInfo interface provided by the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework.

For example:

```csharp
services.AddDbContextForTenant<MyDbContext, TenantInfo>((tenant, options) => options.UseSqlServer("<connection_string>"));
```

**Note**: Refer to the official documentation by Microsoft for more information on how to configure the DbContext in your application, and the documentation of the [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant) framework for more information on how to configure the multi-tenant support and its support for [Entity Framework Core](https://www.finbuckle.com/MultiTenant/EFCore).

The registration of the repository in the dependency injection container is the same provided by the kernel library, and is the following:

```csharp
services.AddRepository<EntityRepository<MyEntity>>();
services.AddRepository<MyEntityRepository>();
```

or using the shortcut method, that will register the default implementation of the repository:

```csharp
services.AddEntityRepository<MyEntity>();
```

#### Filtering Data

The `EntityRepository<TEntity>` implements both the `IQueryableRepository<TEntity>` and the `IFilterableRepository<TEntity>` interfaces, and allows to query the data only through the `ExpressionFilter<TEntity>` class or through lambda expressions of type `Expression<Func<TEntity, bool>>`.

## Entity Manager

The framework provides an extension that allows to control the operations performed on the repository, ensuring the consistency of the data (through validation).

The `EntityManager<TEntity>` class wraps around instances of `IRepository<TEntity>`, enriching the basic operations with validation logic, and providing a way to intercept the operations performed on the repository, and preventing exceptions to be thrown without a proper handling.

It is possible to derive from the `EntityManager<TEntity>` class to implement your own business and validation logic, and to intercept the operations performed on the repository.

This class is suited to be used in application contexts, where a higher level of control is required on the operations performed on the repository (such for example in the case of ASP.NET services).

### Instrumentation

To register an instance of `EntityManager<TEntity>` in the dependency injection container, you can use the `AddEntityManager<TManager>` extension method of the `IServiceCollection` interface.

```csharp
public void ConfigureServices(IServiceCollection services) {
	services.AddEntityManager<MyEntityManager>();
}
```

The method will register an instance of `MyEntityManager` and `EntityManager<TEntity>` in the dependency injection container, ready to be used.

### Entity Validation

It is possible to validate the entities before they are added or updated in the repository, by implementing the `IEntityValidator<TEntity>` interface, and registering an instance of the validator in the dependency injection container.

The `EntityManager<TEntity>` class will check for instances of `IEntityValidator<TEntity>` in the dependency injection container, and will use the first instance found to validate the entities before they are added or updated in the repository.

### Operation Cancellation

The `EntityManager<TEntity>` class provides a way to directly cancel the operations performed on the repository, by passing an argument of type `CancellationToken` to each asynchronous operation, and optionally verifies for instances of `IOperationCancellationSource` that are registered in the dependency injection container.

When the `CancellationToken` argument of an operation is `null`, the `EntityManager<TEntity>` class will check for instances of `IOperationCancellationSource` in the dependency injection container, and will use the first instance found to cancel the operation.

The value of this approach is to be able to attach the cancellation of the operation to a specific context (such as `HttpContext`), and to be able to cancel the operation from a different context (for instance when the HTTP request is cancelled).

## License

The project is licensed under the terms of the [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).

## Contributing

The project is open to contributions: if you want to contribute to the project, please read the [contributing guidelines](CONTRIBUTING.md) for more information.