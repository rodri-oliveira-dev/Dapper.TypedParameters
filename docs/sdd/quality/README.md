# Quality SDD

Esta pasta guarda o contexto persistente da fase de qualidade do repositorio.

## Finalidade

A fase de qualidade documenta a integracao com SonarQube Cloud, a estrategia de
cobertura e o comportamento esperado do Quality Gate em pull requests e na
branch principal.

## SonarQube Cloud

SonarQube Cloud e a plataforma de analise estatica e Quality Gate do
repositorio. A cobertura de testes .NET faz parte do Quality Gate, com foco em
codigo novo e na estrategia Clean as You Code.

## Handoff entre chats

Cada prompt desta fase deve rodar em um chat independente, sem depender de
memoria de conversas anteriores. Os arquivos desta pasta sao o handoff
persistente entre execucoes.

## Segredos

Valores de secrets nunca devem ser persistidos. O token do SonarQube Cloud deve
ser referenciado somente como secret do GitHub Actions, e qualquer configuracao
externa deve ser registrada em `EXTERNAL-SETUP.md` apenas como metadado.

## Commits

Cada prompt deve terminar com um unico commit semantico quando houver
alteracoes. Nao faca push, nao abra pull request e nao altere configuracoes
remotas automaticamente sem pedido explicito.
