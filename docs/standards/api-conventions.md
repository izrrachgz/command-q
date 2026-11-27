# CommandQ package contracts

The package README is part of the published contract. Identify the affected package, existing source and binary callers on both targets, and observable behavior before changing a public surface.

| Surface | Compatibility evidence |
| --- | --- |
| .NET APIs and DI | Public types, signatures, overload resolution, option defaults, service scopes, registration order and missing or duplicate registration behavior |
| Async | Cancellation propagation, exception versus ordinary result, ownership of cleanup, and behavior if an operation has already started |
| Data and time | JSON, UTC precision, existing persisted values, query semantics, transaction ownership and provider differences |
| Delivery and localization | Acknowledgement and dispatch semantics, public envelope shape, extension points, resource precedence and fallback |
| Packaging | Package IDs, dependencies, both target frameworks, public README content and downstream build using the intended package version |

Use package validation and affected consumer tests together. A stable method signature can still hide changes to fallback, cancellation, registration order or precision. Record the version decision and a consumer upgrade path when compatibility changes. Follow the [release policy](../package-release.md) for immutable published versions.
