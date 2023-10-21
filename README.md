![GitHub release](https://img.shields.io/github/v/release/deveel/deveel.repository)
![GitHub license](https://img.shields.io/github/license/deveel/deveel.repository?color=blue)
 ![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/deveel/deveel.repository/ci.yml?logo=github)
 [![codecov](https://codecov.io/gh/deveel/deveel.repository/graph/badge.svg?token=5US7L3C7ES)](https://codecov.io/gh/deveel/deveel.repository) ![Code Climate maintainability](https://img.shields.io/codeclimate/maintainability/deveel/deveel.repository) [![Documentation](https://img.shields.io/badge/gitbook-docs?logo=gitbook&label=docs&color=blue)](https://deveel.gitbook.io/repository/)



# Deveel Repository

This project wants to provide a _low-ambitions_ / _low-expectations_ implementation of the (_infamous_) _[Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)_ for .NET to support the development of applications that need to access different data sources, using a common interface, respecting the principles of the _[Domain-Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design)_ and the _[SOLID](https://en.wikipedia.org/wiki/SOLID)_ principles.

## Libraries

The framework is based on a _kernel_ package, that provides the basic interfaces and abstractions, and a set of _drivers_ that implement the interfaces to access different data sources.

| Package | NuGet |
| ------- | ----- |
| _Deveel.Repository.Core_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Core.svg)](https://www.nuget.org/packages/Deveel.Repository.Core/) |
| _Deveel.Repository.InMemory_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.InMemory.svg)](https://www.nuget.org/packages/Deveel.Repository.InMemory/) |
| _Deveel.Repository.MongoFramework_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.MongoFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.MongoFramework/) |
| _Deveel.Repository.EntityFramework_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.EntityFramework.svg)](https://www.nuget.org/packages/Deveel.Repository.EntityFramework/) |
| _Deveel.Repository.DynamicLinq_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.DynamicLinq/) |
| _Deveel.Repository.Manager_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager/) |
| _Deveel.Repository.Manager.DynamicLinq_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.DynamicLinq.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.DynamicLinq/) |
| _Deveel.Repository.Manager.EasyCaching_ | [![NuGet](https://img.shields.io/nuget/v/Deveel.Repository.Manager.EasyCaching.svg)](https://www.nuget.org/packages/Deveel.Repository.Manager.EasyCaching/) |


## Why Deveel Repository?

The _Repository Pattern_ is a well-known pattern in the software development, that is used to abstract the access to a data source, and to provide a common interface to query and manipulate the data.

You can read the [motivations and drivers](docs/motivations.md) that have led to the development of this framework, and decide if it is suited for your needs.

## Documentation and Guides

A brief gruide on how to use the framework is available in the [documentation](docs/index.md) section of the repository, that will be updated regularly with any major release of the framework.

[Read Here](docs/index.md) or at [GitBook](https://deveel.gitbook.io/repository/).

## License

The project is licensed under the terms of the [Apache Public License v2](LICENSE), that allows the use of the code in any project, open-source or commercial, without any restriction.

## Contributing

The project is open to contributions: if you want to contribute to the project, please read the [contributing guidelines](CONTRIBUTING.md) for more information.
