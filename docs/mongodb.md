# MongoDB

| Feature | Status | Notes
| --- | --- |--- |
| Base Repository | :white_check_mark: | |
| Filterable | :white_check_mark: |  |
| Queryable | :white_check_mark: | _Native EF Core queryable extensions_ |
| Pageable | :white_check_mark: | |
| Multi-tenant | :white_check_mark: | _Uses Finbuckle Multi-Tenant_ |


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