# 004 - README usage and compatibility

## Status

Completed.

## Publico-alvo

- Consumidores .NET que usam Dapper com SQL Server.
- Mantenedores que precisam validar o contrato do primeiro preview.
- Usuarios que precisam declarar explicitamente metadados de parametros string para reduzir ambiguidades de provider.

## Problema resolvido

O README atual ainda esta em estado bootstrap e nao descreve o uso real do pacote preview. Ele precisa explicar quando usar parametros SQL Server explicitamente tipados, quais APIs existem, quais limites sao aplicados e quais comportamentos nao sao prometidos.

## Estrutura planejada do README

1. Visao geral.
2. Motivacao.
3. Compatibilidade.
4. Instalacao.
5. API atual.
6. Exemplos completos.
7. Limites.
8. Tratamento de null.
9. O que o pacote nao faz.
10. Testes e build.
11. Roadmap.
12. Afiliacao.
13. Licenca.

## Exemplos necessarios

- `SELECT` com parametro tipado.
- `INSERT` com mais de um parametro.
- `UPDATE`.
- `DELETE`.
- Valor `null`.
- `varchar(max)`.
- `nvarchar(max)`.
- Chamada assincrona com Dapper.
- Todos os exemplos devem usar `Dapper`, `Dapper.TypedParameters.SqlServer` e `Microsoft.Data.SqlClient`.

## Tabela de compatibilidade

| Item | Versao ou suporte |
| --- | --- |
| .NET | `net8.0`; `net10.0` |
| Dapper | `2.1.79` |
| Microsoft.Data.SqlClient | `7.0.2` |
| Provider ADO.NET | Somente `Microsoft.Data.SqlClient` |
| SQL Server | Suportado via tipos SQL Server declarados |
| System.Data.SqlClient | Nao suportado |

Um unico pacote deve conter assets para `net8.0` e `net10.0`, com a mesma API publica nos dois TFMs. Os limites SQL Server documentados nao mudam conforme o TFM.

## Limitacoes

- Nao consultar schema.
- Nao analisar plano de execucao.
- Nao detectar automaticamente `CONVERT_IMPLICIT`.
- Nao alterar SQL.
- Nao tratar listas `IN`.
- Nao oferecer output parameters nesta versao.
- Nao oferecer TVPs nesta versao.
- Nao oferecer outros providers nesta versao.
- Nao validar se o conteudo cabe em bytes no tamanho declarado.

## Criterios de aceite

- README documenta a visao geral, motivacao, compatibilidade, instalacao, API atual, exemplos, limites, null, nao objetivos, build/testes, roadmap, afiliacao e licenca.
- README documenta somente APIs publicas existentes.
- Exemplos sao coerentes com as assinaturas reais de `SqlParam`.
- `CHANGELOG.md` existe com secao `0.1.0-preview.1` sem data de publicacao.
- `docs/sdd/STATUS.md` e atualizado no inicio e no handoff final.
- Exatamente um commit semantico e criado com a mensagem `docs: document usage and compatibility`.

## Validacoes

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build
dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages
git diff --check
git status --short
```

## Riscos de documentacao divergente

- Exemplos podem mencionar factories ainda nao implementadas.
- Comandos de instalacao podem sugerir que o pacote ja esta publicado publicamente.
- Compatibilidade pode divergir de `Directory.Packages.props` ou dos TFMs reais do projeto.
- O texto de motivacao pode prometer eliminacao completa de conversoes implicitas, o que nao e garantido.
- Limites de `varchar` podem ser descritos incorretamente como quantidade universal de caracteres em vez de bytes declarados.

## Discovery

- Branch inicial: `feat/string-parameters`.
- Working tree inicial: limpa.
- Historico recente inicial:
  - `7358639 test: add Dapper SQL Server integration coverage`
  - `e4c7f83 ci: validate net8.0 and net10.0`
  - `6cfaf44 build: target net8.0 and net10.0`
  - `50abd3f chore: configure repository development baseline`
  - `308c68e Merge pull request #1 from rodri-oliveira-dev/feat/string-parameters`
- `docs/sdd/STATUS.md` indicava `Last completed prompt: 003`.
- API publica real inspecionada em `src/Dapper.TypedParameters.SqlServer/SqlParam.cs`:
  - `SqlParam.VarChar(string? value, int size)`
  - `SqlParam.NVarChar(string? value, int size)`
  - `SqlParam.Char(string? value, int size)`
  - `SqlParam.NChar(string? value, int size)`
  - `SqlParam.VarCharMax(string? value)`
  - `SqlParam.NVarCharMax(string? value)`
- `TypedSqlParameter` implementa `SqlMapper.ICustomQueryParameter` e converte `null` para `DBNull.Value` ao aplicar o parametro ao `SqlCommand`.
- `Directory.Packages.props` confirma `Dapper` `2.1.79` e `Microsoft.Data.SqlClient` `7.0.2`.
- O projeto da biblioteca confirma `PackageId` planejado `Dapper.TypedParameters.SqlServer` e TFMs `net8.0;net10.0`.
- Nenhuma nova decisao duradoura exigiu atualizacao de `docs/sdd/DECISIONS.md`.

## Changed Files

- `README.md`
- `CHANGELOG.md`
- `docs/sdd/STATUS.md`
- `docs/sdd/specs/004-readme-compatibility.md`

## Validation Results

- `dotnet restore Dapper.TypedParameters.sln`: passed; todos os projetos estavam atualizados para restauracao.
- `dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore`: passed; 0 warnings, 0 errors.
- `dotnet test Dapper.TypedParameters.sln --framework net8.0 --configuration Release --no-build`: passed; 29 testes unitarios e 8 testes de integracao passaram.
- `dotnet test Dapper.TypedParameters.sln --framework net10.0 --configuration Release --no-build`: passed; 29 testes unitarios e 8 testes de integracao passaram.
- `dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output ./artifacts/packages`: passed; criou `artifacts/packages/Dapper.TypedParameters.SqlServer.0.1.0-preview.1.nupkg`.
- Comparacao dos exemplos do README com a API real: passed; o README menciona somente as seis factories existentes de `SqlParam`.
- `git diff --check`: passed.
- `git status --short`: mostrou somente arquivos pertencentes a esta tarefa antes do commit.

## Limitations

- O pacote nao foi publicado.
- Nenhum push, tag, release ou PR foi criado.
- A disponibilidade publica do Package ID no NuGet nao foi validada nesta tarefa.

## Handoff

```text
Last completed prompt: 004
Last expected commit: docs: document usage and compatibility
Current status: Completed
Next prompt: 005-preview-package-readiness
```

## Commit Planned

```text
docs: document usage and compatibility
```
