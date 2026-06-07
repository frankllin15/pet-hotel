# 06 — Testes

Quatro níveis, cada um com um alvo claro. Rápidos embaixo, lentos no topo.

## Unit — Domínio

Alvo: invariantes dos agregados e value objects. Sem infraestrutura, sem mocks de I/O.

- Testar transições de estado e regras (`Reservation.Confirm` rejeita vacina vencida, rejeita estado inválido).
- Value objects validam na criação (`DateRange` rejeita fim antes do início).
- Verificar que o evento de domínio correto é levantado.

## Unit — Application

Alvo: handlers de Command/Query com portas *fakeadas*.

- Repositórios e gateways substituídos por fakes/stubs.
- Verifica orquestração e mapeamento de `Result` (NotFound, Conflict, Success).
- Sem banco real.

## Integração

Alvo: EF Core real, mapeamento, query filters de tenant, migrations, Outbox.

- **Testcontainers** sobe um PostgreSQL efêmero por execução.
- Aplicar migrations reais no container.
- **Casos obrigatórios de multi-tenancy:**
  - leitura de um tenant nunca retorna dado de outro (query filter);
  - inserção carimba `TenantId` automaticamente (interceptor);
  - operação sem tenant no contexto falha de forma controlada.
- **Concorrência:** duas confirmações concorrentes na mesma acomodação → uma falha com conflito (xmin).
- **Outbox:** mensagem é persistida na mesma transação do agregado; rollback do agregado descarta a mensagem.
- Endpoints: testar via host real (Wolverine/`Alba` ou `WebApplicationFactory`).

## Arquitetura

Alvo: travar a regra de dependência — falha de build, não revisão manual.

Ferramenta: **NetArchTest** (ou ArchUnitNET). Regras mínimas:

- `Domain` não referencia `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore`, `Wolverine`, nem `Infrastructure`.
- `Application` não referencia `Infrastructure`.
- Um módulo não referencia o `Domain`/`Infrastructure` de outro módulo (só `Contracts`).
- `Infrastructure` não é referenciada por `Domain`.

```csharp
[Fact]
public void Domain_nao_depende_de_infraestrutura()
{
    var result = Types.InAssembly(typeof(Reservation).Assembly)
        .ShouldNot()
        .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Wolverine", "Microsoft.AspNetCore")
        .GetResult();

    Assert.True(result.IsSuccessful, string.Join("\n", result.FailingTypeNames ?? []));
}
```

## Convenções

- Um projeto de unit test por módulo (`PetHotel.<Module>.UnitTests`).
- Integração e arquitetura centralizados (`PetHotel.IntegrationTests`, `PetHotel.ArchitectureTests`).
- xUnit. Nomes de teste descritivos em pt-BR são aceitáveis para legibilidade do negócio.
- Sem dependência de ordem entre testes; cada um cria seu próprio estado.
