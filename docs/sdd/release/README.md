# Release SDD

Esta pasta guarda a fase SDD de preparacao de release do primeiro pacote
NuGet publico.

## Finalidade

A fase de release formaliza identidade publica, politica tecnica, consumo,
autenticacao de publicacao e checklist final antes de qualquer publicacao no
NuGet.org.

## Prompts

Os prompts desta fase devem rodar em chats independentes, sem depender de
memoria de conversas anteriores:

1. `013-package-release-policy`
2. `014-package-consumption`
3. `015-release-automation`
4. `016-final-release-readiness`

Cada prompt deve recuperar informacao persistente do repositorio, criar ou
atualizar sua especificacao antes da implementacao e terminar com um unico
commit semantico.

## Fontes de handoff

- `DECISIONS.md` e a fonte de decisoes aceitas da fase de release.
- `STATUS.md` e o handoff operacional entre prompts.
- `EXTERNAL-SETUP.md` lista acoes humanas fora do repositorio, como ambiente
  GitHub e Trusted Publishing no NuGet.org.

## Restricoes

Durante os prompts 13 a 16:

- nao publique pacote automaticamente;
- nao crie release;
- nao crie tag;
- nao faca push sem pedido explicito;
- nao marque configuracoes externas como concluidas sem verificacao humana.
