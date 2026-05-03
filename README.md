![GitHub release](https://img.shields.io/github/v/release/deveel/deveel.repository)
![GitHub license](https://img.shields.io/github/license/deveel/deveel.repository?color=blue)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/deveel/deveel.repository/ci.yml?logo=github)
[![codecov](https://codecov.io/gh/deveel/deveel.repository/graph/badge.svg?token=5US7L3C7ES)](https://codecov.io/gh/deveel/deveel.repository)
[![Documentation](https://img.shields.io/badge/gitbook-docs?logo=gitbook&label=docs&color=blue)](https://deveel.gitbook.io/repository/)

# Deveel Repository

**Deveel Repository** is a lightweight .NET framework that provides a principled implementation of the [_Repository Pattern_](https://martinfowler.com/eaaCatalog/repository.html), designed to help developers build applications grounded in [_Domain-Driven Design (DDD)_](https://en.wikipedia.org/wiki/Domain-driven_design) and [_SOLID_](https://en.wikipedia.org/wiki/SOLID) principles.

The framework abstracts data access behind a clean, stable interface — keeping your domain model independent of any specific persistence technology — while integrating seamlessly with popular data-access libraries as the underlying backing store.

---

## Why Deveel Repository?

At its core, **Deveel Repository** is about _keeping your domain clean_.

In Domain-Driven Design the repository is not merely a data-access helper: it is the **boundary between the domain model and the infrastructure layer**. It speaks the language of the domain (entities, aggregates, identities) while hiding every detail of how data is fetched or persisted.

This library was born from the need to have a consistent, framework-agnostic abstraction for this boundary, without forcing application developers to:

- couple their domain logic to a specific ORM or database driver, or
- re-implement the same boilerplate repository scaffolding in every project.

> **It was never the intention to build another ORM.** Object-Relational Mappers (and document-mapper equivalents) such as Entity Framework Core, Dapper, or MongoFramework are excellent tools for mapping objects to storage. Deveel Repository _uses_ them — it does not replace them.

### Deveel Repository vs. ORMs

The table below highlights the key differences and shows how both layers coexist:

| Concern | ORM (EF Core, Dapper, …) | Deveel Repository |
|---|---|---|
| **Responsibility** | Map objects ↔ database tables / documents | Provide a domain-oriented access interface |
| **Speaks the language of** | Database schema, SQL, drivers | Domain model (entities, aggregates) |
| **Knows about** | Tables, columns, change tracking, transactions | Collections of entities and their identities |
| **Lives in layer** | Infrastructure | Domain / Application boundary |
| **Used by** | Repositories and infrastructure code | Application services and domain services |

In practice, **you create a repository _on top of_ an ORM** — the ORM handles low-level persistence while Deveel Repository defines _what_ the application can ask for. For example, `Deveel.Repository.EntityFramework` wraps Entity Framework Core's `DbContext` behind the `IRepository<TEntity>` interface, giving the domain a stable contract that survives database migrations and EF Core upgrades. The same principle applies to `Deveel.Repository.MongoFramework` (backed by MongoFramework / MongoDB) and any custom driver you care to write.

This is not a limitation — it is by design. Decoupling ORMs from domain logic is one of the most impactful architectural decisions you can make for long-term maintainability.

---

## Libraries

The framework is organized into a _kernel_ package (providing interfaces and abstractions) and a set of _driver_ packages that wire those abstractions to concrete data sources.

- **Stable releases** are published to [**NuGet.org**](https://www.nuget.org/profiles/deveel).
- **Pre-release / unstable builds** are available from the [**GitHub Packages**](https://github.com/deveel/deveel.repository/packages) feed (`https://nuget.pkg.github.com/deveel/index.json`).

| Package                                | Description                                                                                                   | NuGet (stable) |                                                                        Downloads (NuGet)                                                                         |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------| :------------: |:----------------------------------------------------------------------------------------------------------------------------------------------------------------:|
| `Deveel.Repository.Core`               | Kernel abstractions: interfaces, base types, and DI extensions                                                | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Core.svg)](https://www.nuget.org/packages/Deveel.Repository.Core/) |                [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.Core.svg)](https://www.nuget.org/packages/Deveel.Repository.Core/)                |
| `Deveel.Repository.InMemory`           | Volatile, in-process repository — ideal for testing and prototyping                                           | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.InMemory.svg)](https://www.nuget.org/packages/Deveel.Repository.InMemory/) |            [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.InMemory.svg)](https://www.nuget.org/packages/Deveel.Repository.InMemory/)            |
| `Deveel.Repository.EntityFramework`    | Repository driver backed by [Entity Framework Core](https://github.com/dotnet/efcore)                         | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.EntityFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.EntityFramework/) |     [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.EntityFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.EntityFramework/)     |
| `Deveel.Repository.MongoFramework`     | Repository driver backed by [MongoFramework](https://github.com/turnersoftware/mongoframework) / MongoDB      | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.MongoFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.MongoFramework/) |      [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.MongoFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.MongoFramework/)      |
| `Deveel.Repository.DynamicLinq`        | Filter / query support via [System.Linq.Dynamic.Core](https://github.com/zzzprojects/System.Linq.Dynamic.Core) | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.DynamicLinq/) |         [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.DynamicLinq/)         |
| `Deveel.Repository.Manager`            | Business layer (_EntityManager_) with validation, normalization, event sourcing, and logging                  | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager/) |             [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.Manager.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager/)             |
| `Deveel.Repository.Manager.DynamicLinq` | Dynamic LINQ query extensions for the Entity Manager                                                          | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.DynamicLinq/) | [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.Manager.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.DynamicLinq/) |
| `Deveel.Repository.Manager.EasyCaching` | Second-level caching for the Entity Manager via [EasyCaching](https://github.com/dotnetcore/EasyCaching)      | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.EasyCaching.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.EasyCaching/) | [![Downloads](https://img.shields.io/nuget/dt/Deveel.Repository.Manager.EasyCaching.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.EasyCaching/) |

---

## Quick Start

### 1. Install a driver package

Pick the driver that matches your data source. The `Core` kernel package is pulled in automatically as a transitive dependency:

```bash
# Entity Framework Core
dotnet add package Deveel.Repository.EntityFramework

# MongoDB
dotnet add package Deveel.Repository.MongoFramework

# In-Memory (testing / prototyping)
dotnet add package Deveel.Repository.InMemory
```

To consume an unstable pre-release build, add the GitHub Packages feed first:

```bash
dotnet nuget add source https://nuget.pkg.github.com/deveel/index.json \
  --name deveel-github --username <your-github-username> --password <your-pat>
```

### 2. Register the repository

Use the `AddRepository<T>` extension on `IServiceCollection`:

```csharp
// Program.cs / Startup.cs
builder.Services.AddRepository<InMemoryRepository<Order>>();
```

After registration the following services are resolvable from the DI container (availability depends on the concrete repository's capabilities):

| Interface | Description |
|---|---|
| `IRepository<TEntity>` | Core CRUD and single-entity look-ups |
| `IQueryableRepository<TEntity>` | LINQ-based queries |
| `IPageableRepository<TEntity>` | Paginated result sets |
| `IFilterableRepository<TEntity>` | Filter-expression–based queries |

### 3. Consume the repository in your services

```csharp
public class OrderService(IRepository<Order> orders)
{
    public Task<Order?> GetAsync(string id, CancellationToken ct)
        => orders.FindByIdAsync(id, ct);

    public Task PlaceAsync(Order order, CancellationToken ct)
        => orders.AddAsync(order, ct);
}
```

For driver-specific configuration, multi-tenancy, and guidance on writing a custom repository, refer to the [full documentation](docs/index.md) or browse it online at [GitBook](https://deveel.gitbook.io/repository/).

---

## Documentation and Guides

| Topic | Description |
|---|---|
| [Getting Started](docs/index.md) | Installation, requirements, and first steps |
| [Entity Framework Core driver](docs/repository-implementations/ef-core.md) | Storing entities via EF Core |
| [MongoDB driver](docs/repository-implementations/mongodb.md) | Storing entities in MongoDB |
| [In-Memory driver](docs/repository-implementations/in-memory.md) | In-process volatile storage |
| [Entity Manager](docs/entity-manager/) | Business layer with validation, caching, and events |
| [Custom repositories](docs/custom-repository.md) | Write your own driver |
| [Multi-Tenancy](docs/multi-tenancy.md) | Tenant-isolated repositories |

Full documentation is also available on [GitBook](https://deveel.gitbook.io/repository/).

---

## License

The project is licensed under the terms of the [Apache Public License v2](LICENSE), which allows unrestricted use in any project — open-source or commercial — without restriction.

## Contributing

The project is open to contributions. Please read the [contributing guidelines](CONTRIBUTING.md) before opening a pull request.

## Contributors

A huge thank-you to everyone who has contributed code, documentation, bug reports, and ideas:

[![Contributors](https://contrib.rocks/image?repo=deveel/deveel.repository)](https://github.com/deveel/deveel.repository/graphs/contributors)

_Contributions of all kinds are welcome — see [CONTRIBUTING.md](CONTRIBUTING.md) to get started._
