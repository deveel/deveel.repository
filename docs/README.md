# Why Another Repository Pattern Library?

## Drivers and Motivation

The repository pattern is a well-known pattern in the domain-driven design, that allows to abstract of the data access layer from the domain model, providing a clean separation of concerns.

The repository pattern is often used in conjunction with the _unit of work_ pattern, which allows to grouping of a set of operations in a single transaction.

While implementing several projects for my own needs, and while creating some Open-Source projects requiring a certain degree of data persistence, I've found myself implementing the same pattern over and over again, with some minor differences, depending on the data source I was using.

I tried to look for existing solutions that could help me in this task, but I found that most of the existing libraries were either unreliable, either too opinionated or simply not providing the features I was looking for.

Although this pattern is not applicable to all scenarios (for instance in the case of _event-driven_ applications), I found that it is still a good pattern to use in many cases, and I decided to create this library to provide a simple and reliable implementation of the pattern, that can be used in different scenarios.

### Why Not Just Use Entity Framework Core?

A great advantage of the usage of _Entity Framework Core_ is that it provides a set of abstractions that allows one to access different data sources and use the same LINQ syntax to query the data.

Anyway, design-wise the Entity Framework is closer to an ORM than to a repository pattern, and it doesn't provide a way to abstract the data access layer from the domain model.

Furthermore, the project was started to address the need to access different data sources, and not only relational databases (for example, MongoDB, or in-memory data sources).
