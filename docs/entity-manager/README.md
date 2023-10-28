# The Entity Manager

The framework provides an extension that allows control of the operations performed on the repository, ensuring the consistency of the data (through validation).

The `EntityManager<TEntity>` class wraps around instances of `IRepository<TEntity>`, enriching the basic operations with validation logic, providing a way to intercept the operations performed on the repository, and preventing exceptions from being thrown without proper handling.

It is possible to derive from the `EntityManager<TEntity>` class to implement your own business and validation logic, and to intercept the operations performed on the repository.

This class is suited to be used in application contexts, where a higher level of control is required on the operations performed on the repository (such for example in the case of ASP.NET services).

#### Instrumentation

To register an instance of `EntityManager<TEntity>` in the dependency injection container, you can use the `AddEntityManager<TManager>` extension method of the `IServiceCollection` interface.

```csharp
public void ConfigureServices(IServiceCollection services) {
    services.AddEntityManager<MyEntityManager>();
}
```

The method will register an instance of `MyEntityManager` and `EntityManager<TEntity>` in the dependency injection container, ready to be used.

#### Operation Cancellation

The `EntityManager<TEntity>` class provides a way to directly cancel the operations performed on the repository, by passing an argument of type `CancellationToken` to each asynchronous operation, and optionally verifies instances of `IOperationCancellationSource` that are registered in the dependency injection container.

When the `CancellationToken` the argument of an operation is `null`, the `EntityManager<TEntity>` the class will check for instances of `IOperationCancellationSource` in the dependency injection container, and will use the first instance found to cancel the operation.

The value of this approach is to be able to attach the cancellation of the operation to a specific context (such as `HttpContext`), and to be able to cancel the operation from a different context (for instance when the HTTP request is canceled).
