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
9. Public API contract: Frozen for 1.0 candidate.
10. Breaking public API changes after this point: Not allowed without restarting
    the RC cycle.
11. Additive future APIs: May be considered for 1.x using SemVer compatibility
    rules.
12. Scalar provider-parameter reuse: declared scalar metadata is applied and
    undeclared scalar metadata is reset to provider default `0`.
13. First release candidate version: `1.0.0-rc.1`.
14. SDK package validation baseline: `0.1.0-preview.1` is used as the hard
    binary and public API compatibility baseline for the RC. The behavioral
    scalar metadata reset from Prompt 018 is documented as an intentional
    pre-1.0 stabilization adjustment, not hidden by a suppression.
15. 1.0 public API: Frozen.
16. RC compatibility: Passed.
17. Feature freeze: Maintained.
18. Stable candidate: `1.0.0`.
19. Breaking changes before stable: None after RC.
20. Stable release recommendation: READY FOR 1.0.0.

## Pending

- Stable merge, tag, rehearsal, publication, indexing, and public consumption
  validation.

## Public API Freeze

Public API contract:
Frozen for 1.0 candidate

Feature freeze:
Active

Breaking public API changes after this point:
Not allowed without restarting the RC cycle

Additive future APIs:
May be considered for 1.x using SemVer compatibility rules

## Stable 1.0 Readiness

1.0 public API:
Frozen

RC compatibility:
Passed

Feature freeze:
Maintained

Stable candidate:
1.0.0

Breaking changes before stable:
None after RC

Stable release recommendation:
READY FOR 1.0.0
