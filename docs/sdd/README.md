# SDD workflow

Esta pasta guarda o contexto persistente do fluxo SDD para evoluir o repositorio de forma pequena, rastreavel e reproduzivel.

## Ordem dos prompts

Os prompts devem ser executados em ordem numerica. Cada prompt roda em um chat separado, sem presumir acesso a conversas anteriores.

Antes de iniciar qualquer implementacao, leia sempre:

- `docs/sdd/DECISIONS.md`
- `docs/sdd/STATUS.md`

Cada prompt deve criar uma especificacao em `docs/sdd/specs/` antes da implementacao. Ao concluir, atualize `STATUS.md` com o estado final e o proximo prompt esperado.

Cada prompt deve terminar com exatamente um commit semantico quando houver alteracoes. Nao faca push, nao publique pacote e nao abra pull request automaticamente.
