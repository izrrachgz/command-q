Project: Domain
Purpose: Domain models, entities, aggregates, business rules and domain-specific exceptions.
Framework: .NET 10 (class library).
Conventions: Favor immutability where applicable; place validations inside entities/value objects.
Testing: Test business rules here with unit tests from `Application.Test` or dedicated tests.
Notes: Avoid dependencies on infrastructure (persistence, web frameworks).