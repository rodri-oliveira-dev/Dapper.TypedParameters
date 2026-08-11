# D01 - Stable 1.0 Public Docs

## Status

Completed.

## Context

`TypedParameters.Dapper.SqlServer` 1.0.0 is the first stable NuGet release of
the SQL Server typed-parameters package for Dapper.

## Problem

The public documentation still contained release-preparation language,
preview/RC installation guidance, local package-source instructions, and text
that made the README read like release history rather than a technical landing
page.

## Audience

- .NET developers using Dapper with SQL Server.
- Backend developers maintaining Dapper data-access code.
- Library maintainers and architects evaluating package behavior.

## Documentation Goals

- Explain the project quickly.
- Show installation early.
- Show a minimal Dapper example early.
- Document supported SQL Server parameter families.
- Preserve technical accuracy around inference, implicit conversions, and
  performance.
- Keep English and Portuguese documentation semantically aligned.

## Documentation Hierarchy

README files are the public landing pages. Detailed docs under `docs/` provide
usage, motivation, examples, and behavioral contracts.

## English/Portuguese Parity

English remains canonical. Portuguese (Brazil) is maintained with the same
structure, commands, APIs, examples, cautions, and technical meaning.

## Versioning Strategy

Primary docs describe 1.0.0 as the stable release. Historical preview and RC
versions remain in historical locations such as `CHANGELOG.md` and SDD release
history.

## Installation Strategy

Primary installation uses:

```bash
dotnet add package TypedParameters.Dapper.SqlServer
```

Release-specific reproducibility may use:

```bash
dotnet add package TypedParameters.Dapper.SqlServer --version 1.0.0
```

## README Strategy

README starts with value proposition, installation, minimal example, motivation,
supported functionality, compatibility, documentation links, non-goals, quality,
contributing, license, and disclaimer.

## Getting Started Strategy

Getting Started assumes the stable package is publicly available and covers
prerequisites, installation, imports, connection creation, SELECT, INSERT,
UPDATE, DELETE, null values, multiple parameters, and links to specialized
examples.

## Examples Strategy

Examples are grouped by parameter family and describe behavior that is supported
by public API and tests.

## Cross-Linking Strategy

Every English document with a Portuguese counterpart links to it. Every
Portuguese document links back to its English counterpart.

## SEO/Discoverability Considerations

README naturally includes Dapper, SQL Server, .NET, Microsoft.Data.SqlClient,
typed parameters, explicit SQL parameter types, SqlDbType, and NuGet without
keyword stuffing.

## Technical Accuracy Constraints

- No universal performance guarantees.
- No claim that `varchar` is better than `nvarchar`.
- No claim that every string inference causes implicit conversion.
- No schema inspection.
- No extra SQL Server permissions or metadata queries.
- No API examples outside `PublicAPI.Shipped.txt`.

## Non-Goals

- Changing production code.
- Changing tests.
- Changing CI or release behavior.
- Publishing packages, tags, releases, pushes, or pull requests.
- Creating a heavy docs portal.

## Acceptance Criteria

- Public docs no longer present RC/preview as primary guidance.
- Stable 1.0.0 is documented after official NuGet verification.
- README functions as a technical landing page.
- Detailed docs provide deeper usage and behavior.
- English and Portuguese docs have semantic parity.
- Links and Markdown are validated.

## Validation Plan

- Verify the official NuGet package page.
- Compare examples with public API files and source.
- Search for stale release-preparation references.
- Validate relative Markdown links.
- Run `git diff --check`.
- Review `git status`, `git diff --stat`, and `git diff`.

## Expected Files

- `README.md`
- `README.pt-BR.md`
- `CHANGELOG.md`
- `docs/getting-started.md`
- `docs/getting-started.pt-BR.md`
- `docs/motivation.md`
- `docs/motivation.pt-BR.md`
- `docs/examples/*.md`
- `docs/sdd/documentation/*`

## Expected Commit

`docs: update documentation for 1.0 release`
