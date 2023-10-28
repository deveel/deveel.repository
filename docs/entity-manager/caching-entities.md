# Caching Entities

The `EntityManager<TEntity>` class provides a way to cache the entities retrieved from the repository, and to use the cached entities instead of querying the repository, through the `IEntityCache<TEntity>` service optionally available in the dependency injection container.

To enable the caching feature in the application, you can register an instance of the entity cache-specific provider.

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

| Method             | Cache Operation | Description                                                                                                                                                                                   |
| ------------------ | --------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `FindByKeyAsync`   | `GetOrSetAsync` | Retrieves an entity from the cache, using a key generated from the given primary key, and if not found it will invoke a the Repository method to retrieve the entity, and cache it (if found) |
| `AddAsync`         | `SetAsync`      | Adds an entity to the cache, using a set of keys generated from the entity added.                                                                                                             |
| `AddRangeAsync`    | `SetAsync`      | Adds a set of entities to the cache, using a set of keys generated from the entities added.                                                                                                   |
| `UpdateAsync`      | `SetAsync`      | Updates an entity in the cache, using a set of keys generated from the entity updated.                                                                                                        |
| `RemoveAsync`      | `RemoveAsync`   | Removes an entity from the cache, using a set of keys generated from the entity removed.                                                                                                      |
| `RemoveRangeAsync` | `RemoveAsync`   | Removes a set of entities from the cache, using a set of keys generated from the entities removed.                                                                                            |

By default, the `EntityManager<TEntity>` class will use the primary key of the entity to generate the keys used to store the entity in the cache: this behavior can be overridden by implementing the `IEntityCacheKeyGenerator<TEntity>` interface and registering an instance of the generator in the dependency injection container.

Implementations of the `EntityManager<TEntity>` have the option to override the default behavior for generating keys for the entities.

**Cache Serialization**

In some scenarios, the entities retrieved from the repository need to be converted to another version of the entity, that is capable of being serialized before being stored in the cache, and deserialized when retrieved from the cache.
