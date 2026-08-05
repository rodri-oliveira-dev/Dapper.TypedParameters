# Guidelines

- Nao modifique codigo de producao durante a analise inicial de cobertura.
- Nao modifique testes apenas para aumentar percentual.
- Use relatorios Cobertura existentes quando fornecidos.
- Quando gerar cobertura localmente, use `coverlet.collector` ja referenciado pelo projeto de testes.
- Escreva saidas em `tests/Dapper.TypedParameters.SqlServer.Tests/TestResults/coverage-analysis/`.
- Continue a analise se alguns testes falharem mas houver XML Cobertura parcial; registre a falha.
- Mostre hotspots de risco mesmo quando os percentuais gerais forem bons.
- Compute CRAP score quando houver dados de complexidade e cobertura por metodo.
- Priorize gaps em API publica, validacao de entrada, conversao de `null` para `DBNull.Value`, tipo SQL, tamanho, precisao, escala e compatibilidade com Dapper.
- Depriorize codigo trivial, declarativo ou gerado.
