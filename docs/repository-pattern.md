# The Repository Pattern

The `IRepository<TEntity>` interface is the main interface of the repository pattern, that defines the basic operations to query and manipulate the data.

The interface is defined as:

```csharp
public interface IRepository<TEntity> : where TEntity : class {
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(object key, CancellationToken cancellationToken = default);
}
```

## Querying the Repository

The foundational contract of the repository pattern provides one single method to query the repository, that is the `FindByIdAsync(object)` method: one of the core concepts of a _domain-driven design_ is that entities are associated to a unique identifier, and the repository pattern provides a way to query the repository by the identifier.

Extensions of the repository pattern can provide additional methods to query the repository, using different criteria, and the library provides a set of interfaces that extend the basic repository interface to provide additional querying capabilities.

You can also implement your own methods to query the repository according to the business logic of your application, and the library will provide a set of extension methods to allow you to use the repository in a functional way. See the [documentation](custom-repository.md) for more details on this specific matter.

### The `IQuery` Interface

The library provides an abstraction to define a query to the repository, to filter the items to be returned and sort the result of the query.

You can use a simple query object like the `Query` structure, or you can implement your own query object, as long as it implements the `IQuery` interface.

These are the predefined query objects provided by the library:

| Query | Description |
| ----- | ----------- |
| `Query` | A simple query object that defines a filter and a sorting rule. |
| `PageQuery<TEntity>` | A query object that defines a page query, with a page number, a page size, a filter and a sorting rule. |
| `QueryBuilder<TEntity>` | An object that allows to build a query using a fluent syntax for a specific entity type. |

### The `IQueryFilter` Interface

The `IQueryFilter` interface is a marker interface that defines a filter to apply to a query: it doesn't provide any method, and it's up to the repository implementation to define the actual filter.

The library provides a set of predefined filter types that can be used to query the repository, and that can be used to implement your own filters.

| Filter | Description |
| ------ | ----------- |
| `ExpressionFilter<TEntity>` | A filter that is backed by a lambda expression of type `Expression<Func<TEntity, bool>>`. |
| `CombinedFilter` | A filter that combines two or more filters using a logical AND operator. |
| `QueryFilter.Empty` | A filter that doesn't apply any filter to the query. In fact, applying this filter to a query has no effect> the use of it is for combination purposes or for colascing. |

Implementations of the repository might provide additional types of query filters: for example, the `MongoDbRepository<TEntity>` provides a `MongoFilter` that is backed by a MongoDB filter expression, and a `MongoGeoDistanceFilter`: see the [MongoDB](mongodb-repository.md) for more information on the specifics).

### The `ISort` Interface

The `ISort` interface is a marker interface that defines a sorting rule to apply to a query: it doesn't provide any method, and it's up to the repository implementation to define the actual sorting rule.

The library provides a set of predefined sorting rules that can be used to query the repository, and that can be used to implement your own sorting rules.

| Sort | Description |
| ---- | ----------- |
| `ExpressionSort<TEntity>` | A sorting rule that is backed by a lambda expression of type `Expression<Func<TEntity, object>>`, used to select the member of `TEntity` for sorting by. |
| `CombinedSort` | A sorting rule that combines two or more sorting rules. |
| `FieldSort` | A  rule that sorts by a field name (_Note_: for IQueryable implementation of the Repository, it requires a mapping function to resolve the member). |

### Extensions

To enrich the capabilities of operations that can be performed on the data source, the library provides a set of interfaces that extend the `IRepository<TEntity>` interface.

#### IFilterableRepository

The `IFilterableRepository<TEntity>` interface provides some extensions that allows to query the repository using instances of the `IQueryFilter` contract.

It is up to the repository implementation support or not the concrete types of filters, either by throwing an exception, either by ignoring the filter.

The interface is defined as:

```csharp
public interface IFilterableRepository<TEntity> : IRepository<TEntity> where TEntity : class {
    Task<TEntity?> FindAsync(IQuery query, CancellationToken cancellationToken = default);
    Task<IList<TEntity>> FindAllAsync(IQuery query, CancellationToken cancellationToken = default);
    Task<long> CountAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(IQueryFilter filter, CancellationToken cancellationToken = default);
}
```


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
