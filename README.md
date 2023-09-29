[![Repository](https://github.com/deveel/deveel.data.repository/actions/workflows/ci.yml/badge.svg)](https://github.com/deveel/deveel.data.repository/actions/workflows/ci.yml)

# Deveel Repository

Implementations of the (infamous) _Repository Pattern_ for .NET to support the domain-driven modeling concerns.

## Installation

The library is available as a [NuGet package](https://www.nuget.org/packages/Deveel.Repository/).

To install the package, run the following command in the _Package Manager Console_:

```powershell
Install-Package Deveel.Repository
```

or using the _.NET CLI_:

```bash
dotnet add package Deveel.Repository
```


## Overview

The repository pattern is a well-known pattern in the domain-driven design, that allows to abstract the data access layer from the domain model, providing a clean separation of concerns.

The pattern is based on the concept of a _repository_, that is an object that encapsulates the logic to access the data source, and provides a set of methods to query and manipulate the data.

The repository pattern is often used in conjunction with the _unit of work_ pattern, that allows to group a set of operations in a single transaction.


## Repository

The `IRepository<TEntity>` interface is the main interface of the repository pattern, that defines the basic operations to query and manipulate the data.

The interface is defined as:

```csharp
public interface IRepository<TEntity> : where TEntity : class {
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken));

    Task<TEntity> FindByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken));
}
```

### Extensions

#### IQueryableRepository

The `IRepository<TEntity>` interface is extended by the `IQueryableRepository<TEntity>` interface, that allows to query the repository using the LINQ syntax.

The interface is defined as:

```csharp
public interface IQueryableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
    IQueryable<TEntity> AsQueryable();
}
```

#### IPageableRepository

The `IRepository<TEntity>` interface is extended by the `IPageableRepository<TEntity>` interface, that allows to query the repository to paginate the results.

The interface is defined as:

```csharp
public interface IPageableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
	Task<PageResult<TEntity>> GetPageAsync(PageQuery<TEntity> query, CancellationToken cancellationToken = default(CancellationToken));
}
```

The `PageQuery<TEntity>` class is defined as:

```csharp
public class PageQuery<TEntity> where TEntity : class {
    public PageQuery(int page, int pageSize, Expression<Func<TEntity, bool>> filter = null) {
        Page = page;
        PageSize = pageSize;
	}

    public int Page { get; }
    
    public int PageSize { get; }

    public Expression<Func<TEntity, bool>> Filter { get; set; }
    
    public IList<IResultSort> SortBy { get; set; }
}
```

The `PageResult<TEntity>` class is defined as:

```csharp
public class PageResult<TEntity> where TEntity : class {
    public PageResult(PageQuery<TEntity>, int total, IEnumerable<TEntity> items) {
        Query = query;
		Total = total;
		Items = items;
	}

	public PageQuery<TEntity> Query { get; }

    public int TotalItems { get; }

    public IEnumerable<TEntity> Items { get; }
}
```

#### IFilterableRepository

The `IRepository<TEntity>` interface is extended by the `IFilterableRepository<TEntity>` interface, that allows to query the repository using  asynchronous LINQ filters.

The interface is defined as:

```csharp
public interface IFilterableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
    Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));

    Task<IList<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));

    Task<int> CountAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken));
}
```

## Providers

The library provides a set of implementations of the repository pattern, that can be used to access different data sources.

### In-Memory

The `InMemoryRepository<TEntity>` class is an implementation of the repository pattern that stores the data in-memory.

### MongoDB

The `MongoDbRepository<TEntity>` class is an implementation of the repository pattern that stores the data in a MongoDB database, using the [MongoFramework](https://github.com/TurnerSoftware/MongoFramework)

MongoFramework is a lightweight .NET Standard library that allows to map .NET objects to MongoDB documents, and provides a set of APIs to query and manipulate the data, using a design that is very similar to the Entity Framework Core.

To start using instances of the `MongoRepository<TEntity>` class, you need first to register a `IMongoDbConnection` instance in the dependency injection container, that will be used to access the database, using one of the extensions methods of the `IServiceCollection` interface:

```csharp
services.AddMongoDbConnection("mongodb://localhost:27017", "my-database");
services.AddTenantDbConnection<TenantInfo>();
```

**Note**: The multi-tenant support is provided by the [Finbuckle.MultiTenant](https://github.com/Finbuckle/Finbuckle.MultiTenant) framework, and you need to first register the `TenantInfo` class in the dependency injection container:

```csharp
services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore()
    .WithRouteStrategy("tenant");
```