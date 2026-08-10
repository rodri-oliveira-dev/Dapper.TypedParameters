# Changelog

## Unreleased

- Adiciona parametros SQL Server numericos e booleanos explicitos para Dapper.
- Inclui `bit`, `tinyint`, `smallint`, `int`, `bigint`, `real`, `float`, `decimal`, `money` e `smallmoney`.
- Configura `Precision` e `Scale` declarados para `decimal`.
- Adiciona parametros `uniqueidentifier`, `binary`, `varbinary` e `varbinary(max)`.
- Preserva arrays binarios vazios e converte somente `null` para `DBNull.Value`.
- Adiciona parametros SQL Server temporais para `date`, `time`, `datetime`,
  `smalldatetime`, `datetime2` e `datetimeoffset`.
- Adiciona suporte fluente a `Output` e `InputOutput` para parametros escalares.
- Adiciona leitura de outputs por `OutputValue` e `GetValue<T>()`.
- Normaliza output `DBNull.Value` para `null` e rejeita casts incompativeis sem
  conversao silenciosa.
- Adiciona suporte a table-valued parameters com `SqlParam.TableValued`.
- Configura TVPs como `SqlDbType.Structured` com `TypeName` explicito e
  `DataTable` fornecido pelo chamador.
- Suporta `DataTable` vazio para TVPs e rejeita tabela nula.
- Mantem API equivalente para `net8.0` e `net10.0`.

## 0.1.0-preview.1

- Primeiro escopo preview do pacote planejado `Dapper.TypedParameters.SqlServer`.
- Documenta parametros string SQL Server explicitamente tipados para Dapper.
- Inclui `varchar`, `nvarchar`, `char`, `nchar`, `varchar(max)` e `nvarchar(max)`.
- Suporta assets para `net8.0` e `net10.0` com a mesma API publica.
- Usa `Dapper` `2.1.79` e `Microsoft.Data.SqlClient` `7.0.2`.
- Mantem suporte restrito a `Microsoft.Data.SqlClient`; `System.Data.SqlClient` nao e suportado.
