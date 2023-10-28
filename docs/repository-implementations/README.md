# Repository Implementations

The _Deveel Repository_ framework comes with some implementations of the Repository pattern.

<table data-full-width="true"><thead><tr><th>Data Source</th><th>Library</th></tr></thead><tbody><tr><td><a href="in-memory.md">In-Memory</a></td><td><img src="https://img.shields.io/nuget/v/Deveel.Repository.InMemory?logo=nuget&#x26;label=Deveel.Repository.InMemory" alt="Nuget" data-size="original"></td></tr><tr><td><a href="ef-core.md">Entity Framework</a></td><td><img src="https://img.shields.io/nuget/v/Deveel.Repository.EntityFramework?logo=nuget&#x26;label=Deveel.Repository.EntityFramework" alt="Nuget" data-size="original"></td></tr><tr><td><a href="mongodb.md">MongoDB</a></td><td><img src="https://img.shields.io/nuget/v/Deveel.Repository.MongoFramework?logo=nuget&#x26;label=Deveel.Repository.MongoFramework" alt="Nuget" data-size="original"></td></tr></tbody></table>



### Abstraction Patterns

One of the benefits of using a Repository pattern is the abstraction of the data access mechanism, which allows the implementation of a single paradigm for the management of the data, porting it to the various data storage sources, following the Liskov substitution principle.

For example. the [`EntityManager<TEntity>`](../entity-manager/) class uses this approach, implementing a superset of functions on top of the `IRepository<TEntity>` abstraction.

This allows to switch between implementations of `IRepository<TEntity>`, without affecting the behavior of the consuming class.

#### Business Data Logic

A design pattern that I recommend is the separation of the overall data logic from the concrete implementation, especially in scenarios of reuse (eg. NuGet libraries).

For example:

`Foo.Service.dll`

```csharp
using System;
using Deveel.Data;

namespace Foo {
    public interface IData {
        string? Id { get; }
        byte[] Content { get; }
        string ContentType { get; }
    }
    
    public interface IDataRepository<TData> : IRepository<TData> where TData : class, IData {
        Task<string> GetContentTypeAsync(TData data, CancellationToken cancellationToken = default);
        Task<byte[]> GetContentAsync(TData data, CancellationToken cancellationToken = default);
        Task SetContentAsync(TData data, string contentType, byte[] content, CancellationTyoken cancellationToken = default);
    }
    
    public class DataManager<TData> : EntityManager<TData> where TData : class, IData {
        public DataManager(IDataRepository<TData> repository, IEntityValidator<TData>? validator = null, IServiceProvider? services = null. ILoggerFactory? loggerFactory = null)
            : base(repository, validator, null, null, services, loggerFactory) {
        }
        
        protected IDataRepository<TData> DataRepository => (IDataRepository<TData>)base.Repository;
        
        public Task<OperationResult> SetContentAsync(TData data, string contentType, byte[] content, CancellationToken? cancellationToken = null) {
            ThrowIfDisposed();
            
            try {
                var existingContentType = await DataRepository.GetContentTypeAsync(data, GetCancellationToken(cancellationToken));
                var existingContent = await DataRepository.GetContentAsync(data, GetCancellationToken(cancellationToken));
                if ((existingContentType != null && existingContentType == contentType) &&
                    (existingContent != null && existingContent == content)) {
                    return OperationResult.NotModified;
                }
                
                await DataRepository.SetContentAsync(data, contentType, content, GetCancellationToken(cancellationToken);
                
                // this will invoke the validation before invoking the 
                // Update method of the repository
                return await UpdateAsync(data);
            } catch (Exception ex) {
                Logger.LogError(ex, "Could not set the content");
                return Fail("DATA_ERROR");
            }
        }
    }
}
```

While in the `Foo.Service.MongoDb.dll` you can implement a MongoDB-specific access logic

```csharp
using System;
using Deveel.Data;

namespace Foo {
    public class MongoData : IData {
        public ObjectId? Id { get; }
        string? IData.Id => Id?.ToString();
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
    
    public class MongoDataRepository : MongoRepository<MongoData>, IDataRepository<MongoData> {
        public MongoDataRepository(IMongoDbContext context)
            : base(context) {
        }
        
        public Task SetContenAsync(MongoData data, string contentType, byte[] content, CancellationToken cancellationToken = default) {
            data.ContentType = contentType;
            data.Content = content;
            return Task.CompletedTask;
        }
        
        public Task<byte[]> GetContentAsync(MongoData data, CancellationToken cancellationToken = default) {
            return Task.FromResult(data.Content);
        }
        
         public Task<string> GetContentTypeAsync(MongoData data, CancellationToken cancellationToken = default) {
            return Task.FromResult(data.ContentType);
        }
    }
}
```

... and in Foo.Service.EF implement the same logic using EntityFramework Core

```csharp
using System;
using Deveel.Data;

namespace Foo {
    public class DbData : IData {
        public Guid? Id { get; }
        string? IData.Id => Id?.ToString();
        public string ContentType { get; set; }
        public string Content { get; set; }
        byte[] IData.Content => Convert.FromBase64String(Content);
    }
    
    public class EntityDataRepository : EntityRepository<MongoData>, IDataRepository<MongoData> {
        public EntityDataRepository(DataDbContext context)
            : base(context) {
        }
        
        public Task SetContenAsync(DbData data, string contentType, byte[] content, CancellationToken cancellationToken = default) {
            data.ContentType = contentType;
            data.Content = Convert.ToBase64(Encoding.UTF8.GetString(content));
            return Task.CompletedTask;
        }
        
        public Task<byte[]> GetContentAsync(DbData data, CancellationToken cancellationToken = default) {
            return Task.FromResult(Convert.FromBase64(Encoding.UTF8.GetBytes(data.Content));
        }
        
         public Task<string> GetContentTypeAsync(MongoData data, CancellationToken cancellationToken = default) {
            return Task.FromResult(data.ContentType);
        }
    }
}
```

