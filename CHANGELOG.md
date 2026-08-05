# Changelog

## Unreleased

- Adiciona parametros SQL Server numericos e booleanos explicitos para Dapper.
- Inclui `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money` e `smallmoney`.
- Configura `Precision` e `Scale` declarados para `decimal`.
- Mantem API equivalente para `net8.0` e `net10.0`.

## 0.1.0-preview.1

- Primeiro escopo preview do pacote planejado `Dapper.TypedParameters.SqlServer`.
- Documenta parametros string SQL Server explicitamente tipados para Dapper.
- Inclui `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)` e `nvarchar(max)`.
- Suporta assets para `net8.0` e `net10.0` com a mesma API publica.
- Usa `Dapper` `2.1.79` e `Microsoft.Data.SqlClient` `7.0.2`.
- Mantem suporte restrito a `Microsoft.Data.SqlClient`; `System.Data.SqlClient` nao e suportado.
