# Motivação

[English](motivation.md) | Português (Brasil)

[Voltar ao README](../README.pt-BR.md) | [Primeiros passos](getting-started.pt-BR.md)

`Dapper.TypedParameters.SqlServer` existe porque o tipo de um parâmetro SQL
Server faz parte do contrato entre o código da aplicação e o banco. Dapper mantém
o modelo de chamada pequeno, enquanto providers ADO.NET ainda precisam
transformar valores CLR em parâmetros do provider.

## Inferência de parâmetros

Quando uma chamada Dapper recebe um objeto anônimo, Dapper e o provider montam
os parâmetros que serão enviados com o comando. Um valor como uma `string`,
`decimal`, `DateTime`, `Guid` ou `byte[]` .NET precisa de metadados SQL Server
antes que o SQL Server execute a query.

Inferência é conveniente e muitas vezes é exatamente o que uma aplicação precisa.
O custo é que os metadados SQL inferidos não ficam explícitos no código chamador.

## Metadados de tipo SQL

A biblioteca expõe os metadados aplicáveis à API atual:

- `SqlDbType`: o tipo SQL Server, como `VarChar`, `Decimal` ou `Structured`.
- `Size`: tamanho de strings e binários limitados, com `-1` representando tipos
  SQL Server `max`.
- `Precision`: declarado apenas para `decimal`.
- `Scale`: declarado para `decimal`, `time`, `datetime2` e `datetimeoffset`.
- `Direction`: `Input`, `Output` ou `InputOutput` para parâmetros escalares.
- `TypeName`: nome do user-defined table type para table-valued parameters.

## Divergência de tipo

Uma divergência de tipo acontece quando o parâmetro enviado pelo cliente não é o
mesmo tipo SQL Server esperado pelo schema ou pelo contrato da stored procedure.
Por exemplo, o código pode enviar um parâmetro string enquanto a coluna comparada
é `varchar(11)`, ou pode omitir a precisão e a escala pretendidas para
`decimal(18, 2)`.

Esta biblioteca torna o tipo do banco parte da intenção do código chamador:

```csharp
Amount = SqlParam.Decimal(amount, precision: 18, scale: 2)
```

## Conversão implícita

SQL Server pode aplicar conversões implícitas ao comparar ou atribuir valores de
tipos SQL diferentes. Se isso importa depende dos tipos exatos, da precedência de
tipos, da collation, dos predicados, dos índices e do plano de execução.

A presença de uma divergência não significa que uma query é automaticamente
lenta, e um parâmetro tipado não garante um plano melhor. Metadados explícitos
apenas dão ao chamador um modo de enviar o tipo SQL pretendido.

## Planos de execução

Diferenças de tipo de parâmetro podem influenciar como o SQL Server avalia
expressões. Em alguns workloads, evitar uma divergência não intencional pode
ajudar a preservar o formato de plano para o qual o schema foi desenhado. Em
outros workloads, pode não haver diferença mensurável.

Código sensível a performance deve ser verificado com dados, estatísticas,
índices e planos de execução representativos.

## Intenção explícita

O principal benefício é intenção:

```text
the database type becomes part of the calling code's intent
```

O chamador escolhe `SqlParam.VarChar`, `SqlParam.NVarChar`,
`SqlParam.Decimal`, `SqlParam.DateTime2` ou outra factory porque o contrato do
banco é conhecido naquele ponto de chamada.

## Trade-offs

- Mais conhecimento do schema aparece no código da aplicação.
- O chamador pode declarar o tipo SQL errado.
- Mudanças de schema podem exigir mudanças no código.
- As chamadas ficam mais verbosas que valores comuns em objetos anônimos.
- A biblioteca não introspecta schema automaticamente.
