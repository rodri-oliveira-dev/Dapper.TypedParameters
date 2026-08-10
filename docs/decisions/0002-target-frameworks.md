# 0002 - Target frameworks

## Status

Accepted.

## Context

The package must remain a single NuGet package while exposing assets for the supported modern .NET target frameworks.

## Decision

- `Dapper.TypedParameters.SqlServer` targets `net8.0` and `net10.0`.
- `netstandard2.0` is removed.
- The public API must remain identical across both TFMs.
- Package references remain unconditioned and centrally versioned.

## Consequences

Consumers use the same package identity with framework-specific assets for `net8.0` and `net10.0`. The implementation should avoid TFM-specific code unless a real incompatibility is proven.
