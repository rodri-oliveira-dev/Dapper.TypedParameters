# AGENTS.md

## Objetivo do repositorio

Este repositorio contem uma biblioteca publica .NET para criacao explicita de parametros SQL Server no Dapper.

- Provider inicial exclusivo: `Microsoft.Data.SqlClient`.
- Pacote NuGet: `TypedParameters.Dapper.SqlServer`.
- A API publica deve permanecer pequena, explicita e previsivel.
- A biblioteca deve considerar compatibilidade futura com `net8.0` e `net10.0`.

O trabalho do Codex deve ser pequeno, correto, reproduzivel e coerente com uma biblioteca NuGet publica. Responda em portugues, salvo pedido explicito em outro idioma.

## Fontes de verdade

Consulte somente os arquivos aplicaveis ao escopo da tarefa atual:

1. `README.md`
2. `docs/decisions/`
3. `Directory.Build.props`
4. `Directory.Build.targets`, se existir
5. `Directory.Packages.props`
6. `.editorconfig`
7. `global.json`
8. `Dapper.TypedParameters.sln`
9. Projetos em `src/`
10. Projetos em `tests/`
11. Workflows em `.github/workflows/`

## Regras obrigatorias

- Faca a menor mudanca possivel para resolver o pedido.
- Preserve compatibilidade da API publica.
- Nao introduza breaking changes sem pedido explicito e documentacao.
- Nao exponha tipos internos desnecessariamente.
- Nao use reflection ou introspeccao de schema sem requisito explicito.
- Nao faca consultas adicionais ao banco.
- Nao dependa de permissoes adicionais no SQL Server.
- Use somente `Microsoft.Data.SqlClient`; nao adicione `System.Data.SqlClient`.
- Preserve Central Package Management. Nao adicione `Version=` em `PackageReference`.
- Nao altere testes apenas para faze-los passar.
- Nao publique pacote, release, tag ou push sem pedido explicito.
- Gere documentacao XML para a API publica.
- Atualize `README.md` e ADR quando houver mudanca de contrato ou decisao arquitetural.
- Considere comportamento e compatibilidade nos dois TFMs futuros, `net8.0` e `net10.0`.
- Nao use APIs exclusivas do .NET 10 em codigo compartilhado sem condicional e justificativa.
- Mantenha a implementacao adequada ao menor TFM suportado, `net8.0`.

## Validacao

Fluxo basico:

```bash
dotnet --info
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
```

Enquanto o projeto ainda nao estiver multi-target para .NET 8 e .NET 10, nao exija comandos por TFM.

## Git

- Continue na branch de trabalho atual.
- Use Conventional Commits.
- Nao faca push nem abra PR sem solicitacao.
- Revise `git status` e `git diff` antes do commit.
- Nao commite com build ou testes falhando sem registrar claramente a causa.

## Skills disponiveis

- `repository-governance-sdd`: governanca do repositorio, `AGENTS.md`, ADRs, prompts e skills. Caminho: `.agents/skills/repository-governance-sdd/SKILL.md`.
- `ci-release-governance`: GitHub Actions, validacoes de CI, release governance e empacotamento sem publicacao. Caminho: `.agents/skills/ci-release-governance/SKILL.md`.
- `coverage-analysis`: analise de cobertura, hotspots e gaps de risco em testes .NET. Caminho: `.agents/skills/coverage-analysis/SKILL.md`.
- `dotnet-library-change`: alteracoes de codigo, API publica, compatibilidade, testes e empacotamento da biblioteca. Caminho: `.agents/skills/dotnet-library-change/SKILL.md`.

Antes de executar uma tarefa especializada, selecione apenas as skills cujo `description` corresponda ao pedido. As regras deste arquivo prevalecem em caso de conflito.
