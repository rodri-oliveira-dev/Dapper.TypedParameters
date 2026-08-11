# Motivação

[English](motivation.md) | Português (Brasil)

[Voltar ao README](../README.pt-BR.md) | [Primeiros passos](getting-started.pt-BR.md)

`Dapper.TypedParameters.SqlServer` existe porque o tipo de um parâmetro SQL
Server faz parte do contrato entre o código da aplicação e o banco. Dapper
mantém o envio de parâmetros pequeno e conveniente, enquanto providers ADO.NET
ainda precisam materializar valores CLR como parâmetros SQL Server.

## Valores CLR e Metadados SQL

Um valor CLR como `string`, `decimal`, `DateOnly`, `TimeOnly`, `DateTime`,
`DateTimeOffset`, `Guid` ou `byte[]` não representa sozinho o contrato completo
do parâmetro SQL Server. O provider também envia metadados como `SqlDbType`,
tamanho, precisão, escala, direção e, para table-valued parameters, `TypeName`.

`SqlParameter` é o objeto do provider que leva esses metadados ao
`Microsoft.Data.SqlClient`.

## Inferência de Parâmetros

Dapper e o provider conseguem inferir metadados de parâmetros a partir de valores
comuns em objetos anônimos:

```csharp
new
{
    Document = "12345678901"
}
```

Essa inferência é útil e correta em muitas aplicações. O trade-off é que o tipo
SQL enviado ao SQL Server não fica explícito no código chamador.

## Metadados SQL Server Explícitos

Quando o contrato esperado do banco é conhecido, o chamador pode deixá-lo
explícito:

```csharp
new
{
    Document = SqlParam.VarChar(document, 11),
    Amount = SqlParam.Decimal(amount, precision: 18, scale: 2)
}
```

A biblioteca então materializa parâmetros do provider com `SqlDbType`, `Size`,
`Precision` e `Scale` declarados.

## varchar e nvarchar

Um exemplo comum é uma `string` .NET comparada com uma coluna SQL Server
declarada como `varchar(11)` ou `nvarchar(150)`.

```csharp
Document = SqlParam.VarChar(document, 11)
Name = SqlParam.NVarChar(name, 150)
```

A biblioteca não presume que `varchar` é melhor que `nvarchar`. A escolha correta
é a que corresponde ao schema ou ao contrato da stored procedure.

## Conversões Implícitas

SQL Server pode aplicar conversões implícitas quando expressões combinam tipos
SQL diferentes. Se uma conversão aparece, e se ela importa, depende da
precedência de tipos do SQL Server, da collation, dos predicados, dos índices, do
formato da query e do plano de execução final.

Metadados explícitos de parâmetros podem ajudar a evitar divergências não
intencionais quando o chamador conhece o tipo SQL Server esperado. Eles não
garantem queries mais rápidas e não removem toda conversão possível.

## Índices e Planos de Execução

Metadados de parâmetros podem influenciar como o SQL Server avalia predicados e
se um padrão de acesso por índice continua útil para uma query específica. O
resultado depende do plano.

Código sensível a performance deve ser medido com dados, estatísticas, índices e
planos de execução representativos.

## Trade-offs

- Mais conhecimento do schema aparece no código da aplicação.
- O chamador pode declarar o tipo SQL errado.
- Mudanças de schema podem exigir mudanças no código.
- As chamadas ficam mais explícitas que valores comuns em objetos anônimos.
- A biblioteca não inspeciona schema automaticamente.
