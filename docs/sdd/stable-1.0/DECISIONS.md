# Stable 1.0 decisions

## Accepted

1. StablePhaseBranch: `release/1.0-hardening` for Prompts 17 through 19.
2. StablePhaseFeatureFreeze: Active.
3. Public preview package:
   `TypedParameters.Dapper.SqlServer 0.1.0-preview.1`.
4. Public package consumption validation must use NuGet.org as the only source
   for `TypedParameters.Dapper.SqlServer`.
5. Local `.nupkg` files, project references, local package sources, `HintPath`
   references, and repository build outputs are forbidden as package sources for
   Prompt 017.
6. Public NuGet consumption: Validated.
7. TFMs validated for public NuGet consumption:
   - `net8.0`
   - `net10.0`
8. Feature freeze: Active.

## Pending

- Public API freeze review for Prompt 018.
- RC release rehearsal for Prompt 019.
- Stable release readiness for Prompt 020.
