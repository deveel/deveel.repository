# In-Memory

| Feature | Status | Notes
| --- | --- |--- |
| Base Repository | :white_check_mark: | |
| Filterable | :white_check_mark: |  |
| Queryable | :white_check_mark: | _Native .NET Queryable functions_ |
| Pageable | :white_check_mark: | |
| Multi-tenant | :x: | |

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
