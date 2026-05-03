# In-Memory Repository

| Feature | Status | Notes |
| ------- | :----: | ----- |
| Base Repository | ✅ | |
| Filterable | ✅ | |
| Queryable | ✅ | Native .NET `IQueryable` |
| Pageable | ✅ | |
| Tracking | ❌ | |
| Multi-tenant | ❌ | |

The `InMemoryRepository<TEntity>` class stores entities in the memory of the running process. It is intended for **testing, prototyping, and scenarios where data persistence is not required**.

## Installation

```bash
dotnet add package Deveel.Repository.InMemory
```

## Registration

Register the repository in the DI container using the generic `AddRepository<T>` method from the kernel package:

```csharp
// Program.cs
builder.Services.AddRepository<InMemoryRepository<MyEntity>>();
```

The library does not provide a type-specific shortcut method for the in-memory driver. Use the generic `AddRepository<T>` form shown above, or register your own concrete sub-class if you have one.

## Pre-seeding Data

The `InMemoryRepository<TEntity>` constructor accepts an optional initial list of entities:

```csharp
var seeded = new List<MyEntity> { /* ... */ };
builder.Services.AddSingleton(new InMemoryRepository<MyEntity>(seeded));
```

## Querying

`InMemoryRepository<TEntity>` implements both `IQueryableRepository<TEntity>` and `IFilterableRepository<TEntity>`.

**With LINQ (`IQueryableRepository`):**

```csharp
var items = repository.AsQueryable()
    .Where(x => x.IsActive)
    .OrderBy(x => x.Name)
    .ToList();
```

**With filter types (`IFilterableRepository`):**

The only supported filter type is `ExpressionQueryFilter<TEntity>` (backed by a lambda expression). You can pass it directly or use the convenience `Query.Where<TEntity>` factory:

```csharp
// Using ExpressionQueryFilter
var query = new Query(new ExpressionQueryFilter<MyEntity>(x => x.Name == "John"));
var items = await repository.FindAllAsync(query);

// Using lambda shorthand (extension method)
var items = await repository.FindAllAsync(x => x.Name == "John");
```

> Passing an unsupported filter type (e.g., a MongoDB-specific filter) will throw `NotSupportedException`.
