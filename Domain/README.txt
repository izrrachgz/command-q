Project: Domain
Purpose: Domain models, entities, aggregates, business rules and domain-specific exceptions.
Framework: .NET 10 (class library).
Conventions: Favor immutability where applicable; place validations inside entities/value objects.
Entity contracts: Use typed entity identity through `IEntity<TKey>` and lifecycle contracts through `IAuditableEntity` / `ISoftDeletableEntity`.
Testing: Test business rules here with unit tests from `Application.Test` or dedicated tests.
Notes: Avoid dependencies on infrastructure-oriented contracts where possible; repository and command runner interfaces live in `Persistence`.
