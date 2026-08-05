---
name: ci-release-governance
description: Use esta skill para revisar ou ajustar GitHub Actions, validacoes de CI, empacotamento NuGet local, releases, tags e automacoes seguras deste repositorio. Nao use para publicacao real de pacote, push ou mudancas em codigo de producao sem impacto no pipeline.
---

# Objetivo

Orientar mudancas em CI, versionamento, empacotamento e automacoes do repositorio com seguranca e rastreabilidade.
Esta skill concentra regras de GitHub Actions, validacoes locais, artifacts e pacote NuGet sem poluir `AGENTS.md`.

# Quando usar

- Alterar ou revisar workflows em `.github/workflows/`.
- Ajustar validacoes de restore, build, test ou pack.
- Revisar artifacts, cobertura ou automacoes locais relacionadas a CI.
- Avaliar permissao, trigger, retencao, publicacao ou seguranca de workflows.
- Atualizar documentacao de desenvolvimento relacionada a CI, release ou empacotamento.

# Quando nao usar

- Alteracoes funcionais na biblioteca sem mudanca em pipeline.
- Testes unitarios ou de integracao sem mudanca em pipeline/automacao.
- Commits locais simples quando as convencoes ja estiverem claras.
- Publish real em NuGet, release, tag, push ou PR sem pedido explicito.

# Entradas esperadas

- Workflow, arquivo de versionamento ou automacao alvo.
- Problema observado, objetivo de governanca ou criterio de aceite.
- Restricoes de seguranca, permissao, trigger, artifact ou compatibilidade.
- Documentacao ou ADR relacionada quando houver.

# Saidas esperadas

- Ajuste minimo em pipeline, versionamento, automacao ou documentacao.
- Explicacao do impacto em restore, build, test, pack, release ou seguranca.
- Validacao local possivel sem executar publish.
- Commit semantico quando solicitado.

# Passos

1. Identifique se a mudanca afeta CI, release, versionamento, coverage, empacotamento ou seguranca de automacao.
2. Consulte `AGENTS.md`, `README.md`, `docs/decisions/`, `Directory.Build.props`, `Directory.Packages.props` e workflows afetados.
3. Compare o comportamento documentado com o comportamento configurado.
4. Preserve comandos oficiais e Central Package Management.
5. Mantenha permissoes de workflow no minimo necessario.
6. Evite triggers amplos, jobs caros ou artifacts persistentes sem necessidade comprovada.
7. Nunca transforme `dotnet pack` em publicacao de pacote sem pedido explicito.
8. Atualize documentacao quando mudar fluxo oficial, requisito local, release, validacao ou estrategia de empacotamento.
9. Valide sintaxe e consistencia dos arquivos alterados.
10. Revise diff e confirme que nao houve alteracao de API publica, codigo de producao ou testes fora do escopo.
11. Relate impacto, validacoes e riscos.

# Comandos recomendados

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
```

# Restricoes

- Nao executar publish, deploy, release real, tag ou push.
- Nao criar branch sem pedido explicito.
- Nao ampliar permissoes de workflow sem justificativa clara.
- Nao remover validacoes para contornar falha.
- Nao introduzir segredos em workflows, scripts ou documentacao.
