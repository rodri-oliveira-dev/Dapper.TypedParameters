# SDD decisions

## Accepted

1. O produto e um unico pacote chamado atualmente `Dapper.TypedParameters.SqlServer`.
2. O pacote deve conter assets para `net8.0` e `net10.0`.
3. `netstandard2.0` sera removido.
4. A API publica deve ser a mesma nos dois TFMs.
5. O pacote suporta apenas `Microsoft.Data.SqlClient`.
6. `System.Data.SqlClient` nao e suportado.
7. Dapper e `Microsoft.Data.SqlClient` devem usar uma unica versao centralizada para os dois TFMs.
8. Nao devem existir condicionais por TFM sem necessidade comprovada.
9. Os limites de tipos SQL Server sao independentes do TFM:
   - `varchar` e `char`: tamanho declarado de 1 a 8.000;
   - `nvarchar` e `nchar`: tamanho declarado de 1 a 4.000;
   - tipos `max`: `Size = -1`.
10. Nenhum pacote provider-neutral `Core` ou `Abstractions` sera criado neste momento.
11. A CI valida `net8.0` e `net10.0` em matriz explicita antes do empacotamento.
12. O pacote NuGet gerado pela CI e retido apenas como artefato `.nupkg`; nao ha publicacao automatica.
