# Changelog

## Unreleased

- Adds SonarQube Cloud analysis to CI.
- Imports OpenCover coverage reports for SonarQube Cloud.
- Enforces the pull request Quality Gate through the SonarQube Cloud job.

## 1.0.0

- Prepares the first stable release of `TypedParameters.Dapper.SqlServer`.
- Stabilizes the public API contract validated by `1.0.0-rc.1`.
- Supports explicit SQL Server scalar parameters for strings, numeric values,
  binary values, identifiers, and temporal values.
- Supports `net8.0` and `net10.0` with equivalent public API and package
  assets.
- Supports `Microsoft.Data.SqlClient` as the only ADO.NET provider.
- Maintains the SQL Server compatibility policy declared for SQL Server 2016
  through SQL Server 2025, Azure SQL Database, Azure SQL Managed Instance, and
  Azure Synapse Analytics through driver compatibility.
- Supports scalar output and input/output parameters through `AsOutput()`,
  `AsInputOutput()`, `OutputValue`, and `GetValue<T>()`.
- Supports table-valued parameters through `SqlParam.TableValued(...)` with
  explicit `TypeName` and caller-provided `DataTable`.
- Uses SDK package validation with `1.0.0-rc.1` as the compatibility baseline
  for the stable package.
- Validates public package consumption from NuGet.org for `1.0.0-rc.1`.
- Keeps the Trusted Publishing workflow for NuGet.org publication without a
  long-lived NuGet API key.

This section prepares the stable release. The package is not published by this
prompt.

## 1.0.0-rc.1

- Prepares the first release candidate for the frozen 1.0 public API contract.
- Keeps the RC feature-complete without adding new public APIs after the freeze.
- Supports explicit SQL Server scalar parameters for strings, numeric values,
  binary values, identifiers, and temporal values.
- Supports scalar output and input/output parameters through `AsOutput()`,
  `AsInputOutput()`, `OutputValue`, and `GetValue<T>()`.
- Supports table-valued parameters through `SqlParam.TableValued(...)` with
  explicit `TypeName` and caller-provided `DataTable`.
- Provides equivalent package assets and public API for `net8.0` and `net10.0`.
- Keeps package identity `TypedParameters.Dapper.SqlServer` separate from
  assembly and namespace identity `Dapper.TypedParameters.SqlServer`.
- Maintains package quality checks for contents, symbols, SourceLink, public
  API baselines, SDK package validation, local package consumption, public
  preview consumption, and vulnerability auditing.
- Uses `0.1.0-preview.1` as the SDK package validation baseline for binary and
  public API compatibility.
- Stabilizes scalar provider-parameter reuse before the RC by resetting
  undeclared `Size`, `Precision`, and `Scale` metadata to provider defaults
  when reusing an existing `SqlParameter`.

This is a release candidate and is not the stable `1.0.0` release.

## 0.1.0-preview.1

- Primeiro preview publico de `TypedParameters.Dapper.SqlServer`.
- Reescreve o README principal em ingles como documentacao canonica.
- Adiciona `README.pt-BR.md` como traducao mantida em portugues brasileiro.
- Adiciona documentacao conceitual e exemplos por familia de parametros em
  `docs/`.
- Adiciona versoes em portugues brasileiro para os guias e exemplos linkados
  pelo README.
- Adiciona parametros SQL Server numericos e booleanos explicitos para Dapper.
- Inclui `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`,
  `decimal`, `money` e `smallmoney`.
- Configura `Precision` e `Scale` declarados para `decimal`.
- Adiciona parametros `uniqueidentifier`, `binary`, `varbinary` e
  `varbinary(max)`.
- Preserva arrays binarios vazios e converte somente `null` para
  `DBNull.Value`.
- Adiciona parametros SQL Server temporais para `date`, `time`, `datetime`,
  `smalldatetime`, `datetime2` e `datetimeoffset`.
- Adiciona suporte fluente a `Output` e `InputOutput` para parametros
  escalares.
- Adiciona leitura de outputs por `OutputValue` e `GetValue<T>()`.
- Normaliza output `DBNull.Value` para `null` e rejeita casts incompativeis sem
  conversao silenciosa.
- Adiciona suporte a table-valued parameters com `SqlParam.TableValued`.
- Configura TVPs como `SqlDbType.Structured` com `TypeName` explicito e
  `DataTable` fornecido pelo chamador.
- Suporta `DataTable` vazio para TVPs e rejeita tabela nula.
- Mantem API equivalente para `net8.0` e `net10.0`.
- Configura SourceLink para GitHub e geracao de simbolos `.snupkg`.
- Adiciona analise de API publica e package validation para evitar mudancas
  acidentais de contrato.
- Adiciona cobertura Cobertura na CI, inspecao de conteudo do pacote,
  dependency review em PR e benchmarks manuais com BenchmarkDotNet.
- Define o Package ID NuGet planejado como `TypedParameters.Dapper.SqlServer`,
  preservando assembly e namespace `Dapper.TypedParameters.SqlServer`.
- Ajusta `Microsoft.Data.SqlClient` para `6.1.6`.
- Documenta politica de compatibilidade do driver para SQL Server 2016 a 2025,
  Azure SQL Database, Azure SQL Managed Instance e Azure Synapse Analytics.
