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

### Caching

The `EntityManager<TEntity>` class provides a way to cache the entities retrieved from the repository, and to use the cached entities instead of querying the repository, through the `IEntityCache<TEntity>` service optionally available in the dependency injection container.

To enable the caching feature in the application, you can register an instance of the entity cache specific provider.

For example, to use the [EasyCaching](https://easycaching.readthedocs.io/en/latest/) library, you can use the following code:

```csharp
public void ConfigureServices(IServiceCollection services) {
	services.AddEasyCaching(options => {
		options.UseInMemory("default");
	});
	
	services.AddEntityEasyCacheFor<MyEntity>();
}
```

The above code will register an instance of `IEntityCache<MyEntity>` in the dependency injection container, that will be used by the `EntityManager<TEntity>` class to cache the entities of type `MyEntity`.

The `EntityManager<TEntity>` class will interface the instance of `IEntityCache<TEntity>` with the following methods:

| Method | Cache Operation | Description |
| ------ | --------------- | ----------- |
| `FindByKeyAsync` | `GetOrSetAsync` | Retrieves an entity from the cache, using a key generated from the given primary key, and if not found it will invoke a the Repository method to retrieve the entity, and cache it (if found) |
| `AddAsync` | `SetAsync` | Adds an entity to the cache, using a set of keys generated from the entity added. |
| `AddRangeAsync` | `SetAsync` | Adds a set of entities to the cache, using a set of keys generated from the entities added. |
| `UpdateAsync` | `SetAsync` | Updates an entity in the cache, using a set of keys generated from the entity updated. |
| `RemoveAsync` | `RemoveAsync` | Removes an entity from the cache, using a set of keys generated from the entity removed. |
| `RemoveRangeAsync` | `RemoveAsync` | Removes a set of entities from the cache, using a set of keys generated from the entities removed. |

By default, the `EntityManager<TEntity>` class will use the primary key of the entity to generate the keys used to store the entity in the cache: this behavior can be overridden by implementing the `IEntityCacheKeyGenerator<TEntity>` interface, and registering an instance of the generator in the dependency injection container.

Implementations of the `EntityManager<TEntity>` have the option to override the default behavior for generating keys for the entities.

#### Cache Serialization

In some scenarios, the entities retrieved from the repository need to be converted to another version of the entity, that is capable of being serialized before being stored in the cache, and deserialized when retrieved from the cache.
