# Stable 1.0 SDD

Esta pasta guarda a fase SDD de estabilizacao para o primeiro release estavel
do pacote `TypedParameters.Dapper.SqlServer`.

## Objetivo

Validar a cadeia publica de release, congelar a API publica, ensaiar o release
candidato e preparar a publicacao estavel `1.0.0` sem adicionar novas features.

## Prompts

Os prompts desta fase devem rodar em chats independentes, sem depender de
memoria de conversas anteriores:

1. `017-public-nuget-consumption`
2. `018-public-api-freeze`
3. `019-rc1-preparation`
4. `020-stable-preparation`

Os Prompts 17, 18 e 19 usam a branch:

```text
release/1.0-hardening
```

Cada prompt deve recuperar informacao persistente somente dos arquivos
versionados, atualizar os arquivos SDD como handoff e terminar com um unico
commit semantico.

## Significado de 1.0.0

`1.0.0` significa que a API publica existente e considerada estavel para o
escopo atual de parametros SQL Server explicitos para Dapper usando
`Microsoft.Data.SqlClient`.

## Feature freeze

The 1.0 stabilization phase is feature-frozen.

Novas familias SQL, novos providers, novas abstracoes e mudancas de contrato
publico nao fazem parte desta fase. Ajustes permitidos devem ser limitados a
validacao, documentacao, governanca de release, compatibilidade e correcao de
problemas encontrados durante a estabilizacao.

## Handoff

`README.md`, `DECISIONS.md`, `STATUS.md`, `EXTERNAL-RELEASE.md`, `specs/` e
`reports/` sao as fontes de handoff entre chats independentes.

## Restricoes

- Um commit semantico por prompt.
- Nenhuma publicacao automatica pelos prompts.
- Nenhuma tag, release, push ou PR sem pedido explicito.
- Nenhum pacote local deve representar pacote publicado em validacoes publicas.
