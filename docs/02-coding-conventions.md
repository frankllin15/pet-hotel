# 02 — Convenções de Código

## Result pattern (falhas esperadas)

Falhas previsíveis (validação, regra de negócio, não encontrado, conflito) usam `Result<T>`. Exceções ficam para o **inesperado**, capturadas em middleware que devolve ProblemDetails.

```csharp
public readonly record struct Error(string Code, string Message, ErrorType Type);

public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Unexpected }

// uso no handler
public async Task<Result<ReservationId>> Handle(CreateReservation cmd, ...)
{
    var pet = await _pets.FindAsync(cmd.PetId);
    if (pet is null)
        return Error.NotFound("pet.not_found", "Pet não encontrado");
    // ...
}
```

- Não usar exceção para fluxo de negócio.
- Mapear `ErrorType` → status HTTP no adaptador da API (Validation→400, NotFound→404, Conflict→409, Forbidden→403, Unexpected→500).

## Validação

- **FluentValidation** valida o **input** (Command/Query) na borda da Application.
- **Invariantes de negócio** vivem no **domínio** (no agregado), não no validator.
- Regra prática: validator garante "o comando é bem formado"; o agregado garante "a operação é válida no estado atual".

## Casos de uso

- Um caso de uso = um **Command** (escrita) ou **Query** (leitura), cada um com seu handler Wolverine.
- Command/Query são `record`s imutáveis.
- Handlers são finos: orquestram, não contêm regra de domínio.
- **Escrita** carrega o agregado completo via repositório e respeita invariantes.
- **Leitura** projeta direto para DTO (não passa pelo agregado) — ver `docs/04`.

## Mapeamento objeto-objeto

- **Mapeamento manual** ou **Mapperly** (source-generated, MIT). **Não usar AutoMapper** (comercial desde 2025).
- Mapear na borda (Application → DTO de resposta). Domínio nunca conhece DTO.

## Endpoints (API)

- **Wolverine.HTTP** ou Minimal API, agrupados por módulo.
- Versionamento por rota (`/v1/...`).
- Erros padronizados via **ProblemDetails** (RFC 9457).
- Endpoint não contém regra: recebe request → envia Command/Query via `IMessageBus` → mapeia `Result` para resposta HTTP.

## Injeção de dependência

- Cada módulo expõe um método de registro próprio: `services.AddBookingModule(config)`.
- Portas registradas no módulo que as implementa.
- Nada de `IServiceProvider` injetado em domínio/application (service locator é proibido).

## Naming

- Código e identificadores em **inglês**; comentários/doc em **pt-BR**.
- Agregados no singular (`Reservation`, `Pet`).
- Commands no imperativo (`CreateReservation`, `ConfirmCheckIn`).
- Eventos de domínio no passado (`ReservationConfirmed`, `VaccineExpired`).
- Portas com prefixo `I` e sufixo por papel (`IReservationRepository`, `IHealthClearanceGateway`).

## Assíncrono

- `async`/`await` ponta a ponta. Proibido `.Result` / `.Wait()` / `.GetAwaiter().GetResult()`.
- `CancellationToken` propagado em toda chamada de I/O.

## Imutabilidade e nulabilidade

- `nullable reference types` habilitado em toda a solução.
- Value Objects e Commands imutáveis (`record` / `readonly`).
- Coleções expostas pelo agregado são somente-leitura (`IReadOnlyCollection<>`).
