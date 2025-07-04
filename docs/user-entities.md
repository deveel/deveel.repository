# User Entities

The repository pattern library provides a way to define user entities. These are entities that are specific to the user of the application, and that are not shared between different users.

Within the scope of a tenant, there might be several users, and each user might have its own set of entities (eg. configurations, preferences, etc.).

## Defining User Entities

The data model of an entity is free to be defined by the developer, and it can be any class, but to be used as a user entity, it should implement the `IHaveOwner<TKey>` contract.

Anyway, entities that are intended to be user-specific should provide a field that identifies the user that owns the entity.

For instance, a user entity might look like this:

```csharp
public class UserConfiguration : IHaveOwner<string>
{
    public string Id { get; set; }

    public string UserId { get; set; }

    string IHaveOwner<string>.OwnerKey => UserId;

    public string ConfigurationKey { get; set; }

    public string ConfigurationValue { get; set; }
}
```

In this case, the `UserId` field is used to identify the user that owns the configuration, and the `ConfigurationKey` and `ConfigurationValue` fields are used to store the configuration data.

**Note**: _The above example uses the `IHaveOwner<TKey>` explicit implementation to provide the owner key of the entity. If you prefer, you can implement the `IHaveOwner<TKey>` interface directly on the entity class, and provide the `OwnerKey` property as a public property._

## Using User Entities Repository

The Deveel Repository framework provides a way to define a repository for user entities, that is specific to the user that is accessing the data.

The repository for user entities is defined by the `IUserRepository<TEntity, TKey, TOwnerKey>` interface, that extends the `IRepository<TEntity>` interface.

In this specific model, the `TEntity` is the type of the entity, `TKey` is the type of the key of the entity, and `TOwnerKey` is the type of the key of the owner of the entity.

With reference to the previous example, the repository for the `UserConfiguration` entity might look like this:

```csharp
public interface IUserConfigurationRepository : IUserRepository<UserConfiguration, string, string>
{
    Task<UserConfiguration> FindByUserAsync(string userId, string configurationKey, CancellationToken cancellationToken = default);
}
```

In this case, the `IUserConfigurationRepository` interface extends the `IUserRepository<UserConfiguration, string, string>` interface, and provides an additional method to find a configuration by the user and the configuration key.

### Accessing the Current User at Runtime

The `IUserRepository` interface requires the implementation of a `IUserAccessor` service, that provides the current user identifier at runtime.

The `IUserAccessor` service is defined as follows:
```csharp
public interface IUserAccessor<TKey>
{
    TKey? GetUserId();
}
```

The `GetUserId` method should return the identifier of the current user, or `null` if the user is not authenticated.

The `IUserAccessor` service should be registered in the dependency injection container of the application, and it should be provided to the repository implementation.

## Entity Framework Core User Entities Repository

The framework provides a specific implementation of the user entities repository for Entity Framework Core, that allows to store user-specific entities in a relational database.

The `EntityUserRepository<TEntity, TKey, TOwnerKey>` class is the base implementation of the user entities repository for Entity Framework Core, that implements the `IUserRepository<TEntity, TKey, TOwnerKey>` interface.

The logic behind the implementation of the user entities repository is to filter the entities by the owner key, that is provided by the `IUserAccessor` service.