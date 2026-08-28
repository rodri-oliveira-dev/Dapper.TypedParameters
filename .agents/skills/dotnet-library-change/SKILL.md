---
name: dotnet-library-change
description: Use esta skill ao alterar codigo, API publica, compatibilidade, testes ou empacotamento dos providers .NET em Dapper.TypedParameters. Nao use para tarefas puramente documentais, CI puro ou publicacao real de pacote.
---

# Objetivo

Orientar alteracoes pequenas e seguras nos providers `Dapper.TypedParameters.SqlServer` e `Dapper.TypedParameters.PostgreSql`, preservando API publica, compatibilidade e conteudo dos pacotes NuGet.

# Principios

- Prefira a menor alteracao que entregue o comportamento pedido.
- Preserve compatibilidade binaria e de origem sempre que possivel.
- Trate mudanca de assinatura publica, namespace, tipo exposto, excecao observavel ou comportamento de parametro provider-specific como potencial breaking change.
- Nao exponha tipos internos apenas para facilitar teste.
- Nao use reflection, introspeccao de schema ou consultas adicionais ao banco sem requisito explicito.
- Use `Microsoft.Data.SqlClient` somente no provider SQL Server.
- Use `Npgsql` somente no provider PostgreSQL.
- Nao crie referencias cruzadas entre providers.
- Nao extraia abstracao compartilhada apenas para reduzir duplicacao.
- Mantenha `PackageReference` sem `Version=` e altere versoes somente em `Directory.Packages.props`.

# Processo

1. Leia `AGENTS.md`, `README.md`, ADRs relevantes em `docs/decisions/`, o `.csproj` do provider afetado e os testes proximos.
2. Identifique se a mudanca afeta API publica, compatibilidade, multi-targeting, empacotamento ou apenas implementacao interna.
3. Para API publica, preserve nomes, nulidade, tipos de retorno e excecoes salvo pedido explicito.
4. Para multi-targeting, considere `net8.0` como menor TFM suportado futuro e `net10.0` como TFM adicional futuro.
5. Nao use API exclusiva do .NET 10 em codigo compartilhado sem condicional e justificativa.
6. Avalie dependencias publicas e privadas:
   - dependencia publica afeta consumidores e deve ser justificada;
   - dependencia privada deve usar `PrivateAssets` quando apropriado;
   - dependencias transitivas devem ser avaliadas pelo impacto no pacote.
7. Gere ou mantenha XML documentation para toda API publica.
8. Para o provider SQL Server, valide `SqlParameter`, `SqlDbType`, `Value`, `DBNull.Value`, `Size`, `Precision`, `Scale`, direcao e reutilizacao de parametro quando aplicavel.
9. Para o provider PostgreSQL, valide `NpgsqlParameter`, `NpgsqlDbType`, `Value`, `DBNull.Value`, metadados aplicaveis e reutilizacao de parametro quando aplicavel.
10. Prefira testes unitarios para contratos puros e testes de integracao somente quando o comportamento exigir banco real.
11. Nao altere testes apenas para faze-los passar.
12. Ao alterar contrato ou decisao arquitetural, atualize `README.md` ou ADR.
13. Antes de finalizar, valide restore, build, test e pack proporcionais aos providers afetados.

# Checklist de compatibilidade

- A assinatura publica mudou?
- A nulidade publica mudou?
- A excecao observavel mudou?
- O comportamento para `null` mudou?
- Algum pacote passou a expor dependencia nova?
- O conteudo do `.nupkg` ainda contem DLL, XML documentation e README esperados?
- A mudanca funciona no menor TFM suportado futuro, `net8.0`?
- A mudanca evita APIs exclusivas de `net10.0` sem condicional?
- A mudanca manteve dependencias e tipos especificos dentro do provider correto?

# Comandos recomendados

```bash
dotnet restore Dapper.TypedParameters.sln
dotnet build Dapper.TypedParameters.sln --configuration Release --no-restore
dotnet test Dapper.TypedParameters.sln --configuration Release --no-build
dotnet pack src/Dapper.TypedParameters.SqlServer/Dapper.TypedParameters.SqlServer.csproj --configuration Release --no-build --output artifacts/packages
dotnet pack src/Dapper.TypedParameters.PostgreSql/Dapper.TypedParameters.PostgreSql.csproj --configuration Release --no-build --output artifacts/packages
```

# Saida esperada

- Alteracao pequena e revisavel.
- Explicacao objetiva do impacto em API, testes e pacote.
- Validacoes executadas.
- Registro claro de qualquer breaking change explicitamente solicitado.
