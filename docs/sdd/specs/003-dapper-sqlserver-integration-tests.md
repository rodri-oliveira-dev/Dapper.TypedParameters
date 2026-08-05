# 003 - Dapper SQL Server integration tests

## Status

Implemented.

## Problem

Os testes existentes comprovam o contrato dos factories e de `TypedSqlParameter.AddParameter` em memoria, mas ainda nao comprovam que Dapper, `Microsoft.Data.SqlClient` e SQL Server real preservam os metadados tipados ao executar comandos.

## Goals

- Criar testes de integracao reais com Dapper e SQL Server em container.
- Executar os testes de integracao em `net8.0` e `net10.0`.
- Manter os testes que exigem Docker em projeto separado.
- Usar somente `Microsoft.Data.SqlClient` como provider ADO.NET.
- Usar uma unica biblioteca para gerenciar containers.
- Centralizar novas versoes em `Directory.Packages.props`.
- Atualizar a CI para tornar os testes de integracao uma etapa explicita e obrigatoria.

## Non-goals

- Nao alterar APIs publicas.
- Nao implementar novos tipos SQL.
- Nao misturar testes de integracao no projeto de testes unitarios.
- Nao publicar pacote, criar tag, fazer push ou abrir PR.
- Nao introduzir imagens SQL Server de terceiros.
- Nao registrar senha em arquivos versionados ou logs.

## Discovery

- Branch inicial: `feat/string-parameters`.
- Working tree inicial: limpa.
- Historico recente inicial:
  - `e4c7f83 ci: validate net8.0 and net10.0`
  - `6cfaf44 build: target net8.0 and net10.0`
  - `50abd3f chore: configure repository development baseline`
  - `308c68e Merge pull request #1 from rodri-oliveira-dev/feat/string-parameters`
  - `cae1e20 test: cover SQL Server string parameters`
- `docs/sdd/STATUS.md` indicava `Last completed prompt: 002`.
- `docs/sdd/STATUS.md` indicava `Next prompt: 003-dapper-sqlserver-integration-tests`.
- A solucao contem a biblioteca e o projeto unitario `tests/Dapper.TypedParameters.SqlServer.Tests/`.
- Os testes unitarios usam xUnit, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio` e `coverlet.collector`.
- Os testes unitarios estao organizados por contrato: `SqlParamTests` valida factories e limites; `TypedSqlParameterTests` valida criacao/reuso de `SqlParameter`, `DBNull.Value`, tipo, tamanho e rejeicao de comandos nao SQL Server.
- Dapper esta referenciado pela biblioteca em `src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj`.
- `Microsoft.Data.SqlClient` esta referenciado pela biblioteca; `System.Data.SqlClient` nao e usado.
- `Directory.Packages.props` centraliza as versoes de Dapper, SqlClient e dependencias de teste.
- Biblioteca e testes unitarios ja usam `TargetFrameworks` com `net8.0;net10.0`.
- A CI valida os dois TFMs com matriz explicita, mas executa `dotnet test` na solucao, sem separar unitarios e integracao.
- Docker local esta disponivel via Rancher Desktop/WSL2.
- `docker info` retornou servidor Linux `29.5.3`, arquitetura `x86_64`, 8 CPUs e 15.32 GiB de memoria.
- `dotnet --info` retornou SDK ativo `10.0.302`, com SDKs `8.0.423`, `10.0.110`, `10.0.204` e `10.0.302` instalados.

## Container Approach

Usar `Testcontainers.MsSql` como unica biblioteca de container. O modulo encapsula criacao, inicializacao, connection string, aceite de EULA exigido pela imagem e limpeza do container ao final do fixture.

Essa escolha evita scripts Docker paralelos, portas fixas e estado externo compartilhado. A readiness sera reforcada por uma consulta `SELECT 1` via `Microsoft.Data.SqlClient` apos `StartAsync`.

## SQL Server Image

Imagem oficial escolhida:

```text
mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04
```

A tag foi escolhida por ser estavel e identificavel, evitando `latest`. `docker manifest inspect` confirmou a existencia do manifesto da imagem antes da implementacao.

## Readiness Strategy

1. O fixture inicia o container com `MsSqlContainer.StartAsync`.
2. Apos o start, o fixture abre uma conexao real com `SqlConnection`.
3. O fixture executa `SELECT 1` com retry e timeout.
4. Se o SQL Server nao ficar pronto, os testes falham em vez de serem ignorados silenciosamente.

## Isolation Between Tests

Cada teste abrira sua propria `SqlConnection` e usara objetos locais da sessao, como tabelas temporarias `#...`, evitando colisao entre execucoes. Os testes de integracao ficarao em uma collection xUnit com paralelismo desabilitado para impedir compartilhamento mutavel concorrente do container.

## Cleanup Strategy

Os testes que criarem tabelas temporarias farao `DROP TABLE IF EXISTS` em `finally`. A conexao tambem limita o ciclo de vida dos objetos temporarios. O fixture chamara `DisposeAsync` no container para encerrar e remover recursos ao final da suite.

## TFM Matrix

- `net8.0`
- `net10.0`

O projeto de integracao usara os mesmos TFMs da biblioteca e sera executado explicitamente por framework localmente e na CI.

## Test Cases

- `SELECT` com `varchar`.
- `SELECT` com `nvarchar`.
- `INSERT` em coluna `varchar`.
- `INSERT` em coluna `nvarchar`.
- `UPDATE` usando parametro tipado.
- `DELETE` usando parametro tipado.
- Parametro com valor `null`.
- `varchar(max)`.
- `nvarchar(max)`.
- Execucao assincrona com Dapper.
- Mais de um parametro tipado no mesmo comando.
- Validacao do tipo-base recebido pelo SQL Server para `varchar` e `nvarchar` nao `max` usando `SQL_VARIANT_PROPERTY`.

## CI Impact

A CI sera ajustada para:

- restaurar e buildar a solucao por TFM;
- executar testes unitarios por TFM como etapa separada;
- executar testes de integracao por TFM como etapa separada;
- depender de Docker disponivel no runner Linux;
- falhar quando integracao falhar;
- aplicar timeout no job;
- nao usar `continue-on-error`;
- preservar empacotamento somente apos validacao.

## Risks

- Pull inicial da imagem SQL Server pode tornar a primeira execucao lenta.
- O SQL Server exige recursos suficientes no Docker local e no runner.
- A tag de imagem pode ser removida no futuro pela Microsoft, embora seja mais reprodutivel que `latest`.
- Validacao local depende de Docker operacional.

## Acceptance Criteria

- Projeto `tests/Dapper.TypedParameters.SqlServer.IntegrationTests/` existe.
- Projeto de integracao usa `net8.0;net10.0`.
- Projeto de integracao referencia a biblioteca por `ProjectReference`.
- Projeto de integracao referencia Dapper, `Microsoft.Data.SqlClient` e dependencias de teste necessarias.
- `Testcontainers.MsSql` e a unica dependencia nova para containers.
- Projeto esta incluido na solucao.
- Todos os casos minimos de integracao estao cobertos.
- CI executa unitarios e integracao explicitamente em `net8.0` e `net10.0`.
- Nao ha senha versionada.
- Exatamente um commit semantico e criado com a mensagem planejada.

## Validation Commands

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build
docker version
docker info
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build
dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build
git diff --check
git status --short
```

## Commit Planned

```text
test: add Dapper SQL Server integration coverage
```

## Changed Files

- `.github/workflows/ci.yml`
- `Dapper.TypedParameters.sln`
- `Directory.Packages.props`
- `docs/sdd/DECISIONS.md`
- `docs/sdd/STATUS.md`
- `docs/sdd/specs/003-dapper-sqlserver-integration-tests.md`
- `tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj`
- `tests/Dapper.TypedParameters.SqlServer.IntegrationTests/DapperSqlServerParameterTests.cs`
- `tests/Dapper.TypedParameters.SqlServer.IntegrationTests/SqlServerContainerFixture.cs`

## Validation Results

- `dotnet package search Testcontainers.MsSql --take 5`: passed; `Testcontainers.MsSql` latest version observed as `4.13.0`.
- `docker manifest inspect mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04`: passed; manifest exists for the selected official image.
- First `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: failed because `MsSqlBuilder.MsSqlBuilder()` is obsolete in `Testcontainers.MsSql 4.13.0` and warnings are errors.
- Implementation adjusted to use `new MsSqlBuilder(SqlServerImage)`.
- `dotnet restore Dapper.TypedParameters.sln`: passed.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net8.0 --configuration Release --no-build`: passed; 29 tests passed, 0 failed, 0 skipped.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.Tests/Dapper.TypedParameters.SqlServer.Tests.csproj --framework net10.0 --configuration Release --no-build`: passed; 29 tests passed, 0 failed, 0 skipped.
- `docker version`: passed; client `29.6.2-rd`, server engine `29.5.3`, Linux containers.
- `docker info`: passed; Rancher Desktop WSL Distribution, `linux/x86_64`, 8 CPUs, 15.32 GiB memory.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net8.0 --configuration Release --no-build`: passed; 8 tests passed, 0 failed, 0 skipped.
- `dotnet test tests/Dapper.TypedParameters.SqlServer.IntegrationTests/Dapper.TypedParameters.SqlServer.IntegrationTests.csproj --framework net10.0 --configuration Release --no-build`: passed; 8 tests passed, 0 failed, 0 skipped.
- `git diff --check`: passed; emitted line-ending notices that `.github/workflows/ci.yml` and `Directory.Packages.props` will be normalized from CRLF to LF the next time Git touches them.
- `git status --short`: showed only files belonging to this task before commit.

## Limitations

- GitHub Actions remote execution was not run or observed in this task.
- No package was published.
- No push or pull request was created.
