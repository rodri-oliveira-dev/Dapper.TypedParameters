---
name: coverage-analysis
description: Use esta skill para analisar cobertura de testes da biblioteca Dapper.TypedParameters.SqlServer, identificar gaps relevantes, hotspots de risco, classes/metodos perigosos de modificar e prioridades de teste. Nao use para inflar cobertura, alterar testes sem validar comportamento ou instalar ferramentas sem necessidade concreta.
license: MIT
---

# Objetivo

Orientar uma analise pragmatica de cobertura para encontrar risco real, nao apenas aumentar percentual.

Cobertura responde o que foi exercitado, mas nao garante qualidade. Use esta skill para priorizar testes em codigo com maior risco de mudanca, complexidade ou impacto na API publica.

# Quando usar

- O usuario mencionar cobertura, coverage, gate, gaps, hotspots, CRAP score ou risco de refatoracao.
- A cobertura estiver abaixo do limite esperado em CI ou em validacao local.
- For necessario decidir onde adicionar testes primeiro.
- Uma mudanca alterar criacao de `SqlParameter`, API publica, tamanho, precisao, escala, nulidade ou compatibilidade.
- Houver duvida se determinada area esta segura para refatorar.

# Quando nao usar

- Escrever testes novos sem analise de cobertura.
- Corrigir falha funcional de teste sem relacao com coverage.
- Rodar testes apenas para validar build.
- Inflar cobertura tocando codigo sem assert significativo.
- Instalar ferramenta global, pacote ou script sem necessidade comprovada.

# Regras obrigatorias

- Nao altere testes apenas para aumentar percentual.
- Nao aceite teste sem assert significativo como melhoria real de cobertura.
- Nao reduza threshold de cobertura para contornar falha sem instrucao explicita.
- Nao adicione pacote com `Version=`; use Central Package Management.
- Nao instale ferramentas globais se `coverlet.collector` e os scripts locais forem suficientes.
- Nao substitua analise de risco por ranking puramente numerico.
- Considere complexidade, comportamento publico, compatibilidade binaria/de origem e conteudo do pacote NuGet.

# Fontes e arquivos relevantes

Consulte quando existirem ou forem relevantes:

- `AGENTS.md`
- `Directory.Packages.props`
- `src/Dapper.TypedParameters.SqlServer/`
- `tests/Dapper.TypedParameters.SqlServer.Tests/`
- relatorios Cobertura, cobertura HTML ou output do pipeline fornecido pelo usuario

# Processo

1. Identifique se a tarefa pede diagnostico, priorizacao ou mudanca em testes.
2. Leia a estrategia de testes do repositorio antes de propor ferramenta nova.
3. Use o relatorio de cobertura existente quando fornecido.
4. Se for necessario gerar cobertura, prefira `dotnet test` com `coverlet.collector`.
5. Escreva saidas de analise em `tests/Dapper.TypedParameters.SqlServer.Tests/TestResults/coverage-analysis/`.
6. Use `scripts/Compute-CrapScores.ps1` para calcular CRAP score a partir de Cobertura XML quando disponivel.
7. Classifique gaps por risco:
   - alto: API publica, conversao de nulidade, metadados de `SqlParameter`, validacao de limites ou excecoes observaveis;
   - medio: comportamento interno que afeta empacotamento, multi-targeting ou interoperabilidade com Dapper;
   - baixo: glue code trivial, declaracao sem logica ou codigo gerado.
8. Priorize testes que aumentem confianca comportamental, nao apenas linhas executadas.
9. Quando sugerir novos testes, indique comportamento, cenario e motivo.
10. Quando encontrar cobertura superficial, aponte o problema explicitamente.
11. Valide com teste local ou comando proporcional quando a tarefa envolver alteracao.

# Comandos recomendados

```bash
dotnet test Dapper.TypedParameters.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory TestResults/coverage
```

Para calcular CRAP score no PowerShell:

```powershell
.agents/skills/coverage-analysis/scripts/Compute-CrapScores.ps1 -CoberturaPath <coverage.cobertura.xml>
```

# Saida esperada

- Diagnostico objetivo da cobertura.
- Lista priorizada de hotspots ou gaps relevantes.
- Separacao entre cobertura baixa aceitavel e cobertura baixa arriscada.
- Recomendacoes de testes por comportamento.
- Validacoes executadas ou motivo para nao executar.

# Criterio de qualidade

Um bom resultado nao e apenas aumentar percentual. Um bom resultado e reduzir risco real de regressao na API publica, na criacao de parametros SQL Server e no pacote NuGet.
