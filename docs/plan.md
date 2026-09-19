# Demo.VatCalculator — Implementation Plan

## Goal
Web portal VAT calculator for Austria: compute Net, VAT, Gross from exactly one
input amount, for VAT rates 10%, 13%, 20%. .NET 10 Web API (minimal API) +
Angular 22 (Material) SPA. Business rules live in a dependency-free Core layer;
UI and API stay thin.

## Key decisions
- Money rounding: `decimal.Round(x, 2, MidpointRounding.AwayFromZero)` (EU
  currency convention). Exactly one rounding pass; the derived amount is a
  remainder so `net + vat == gross` always holds.
- Rates: 10, 13, 20 (integer percent; enum `VatPercent`).
- Max amount: `1_000_000_000.00`; precision capped at 2 decimals (too-precise => error).
- Exactly one non-null, positive amount among `net`, `gross`, `vat`.
- Unknown JSON fields => rejected (HTTP 400). Contradictory inputs
  (multiple amounts) => 400 with errors on each provided amount field.
- API versioning: path-based `/api/v1/...`. No `Asp.Versioning` package (KISS).
- Locale separators: API accepts JSON numbers only (string `"1,23"` => 400
  malformed body). UI accepts `.` and `,` and normalizes to `.` before sending.
- Swagger: .NET 10 built-in `AddOpenApi()` (`/openapi/v1.json`) +
  `Swashbuckle.AspNetCore.SwaggerUi` for UI at `/swagger`.
- Frontend test runner: Vitest. Frontend validation is a lightweight UX mirror
  only; the server is authoritative (no business-logic duplication).

## Calculation model
`r = rate / 100m`, `round(x) = MidpointRounding.AwayFromZero, 2 dp`:
- input net   -> vat = round(net*r);           gross = net + vat
- input gross -> net = round(gross/(1+r));     vat   = gross - net
- input vat   -> net = round(vat/r);           gross = net + vat

Round-trip boundaries (documented + tested): e.g. net 33.33 @20% -> vat 6.67,
gross 40.00; a later vat-only input 6.67 -> net 33.35. Unavoidable; stated.

## Validation rules (field-level, codes)
`vat.noAmount`, `vat.multipleInputs`, `vat.amountNotPositive`,
`vat.excessivePrecision`, `vat.amountTooLarge`, `vat.rateNotSupported`,
`vat.malformedBody`, `vat.unknownField`.

## API contract
`POST /api/v1/vat/calculate`
Request: `{ "rate": 20, "net"?: 100.00, "gross"?: ..., "vat"?: ... }`
200: `{ rate, net, vat, gross, rounding: { digits: 2, rule: "MidpointRounding.AwayFromZero" } }`
400 (RFC 7807 ProblemDetails):
`{ type, title, status, errors: { <field>: [{ code, message }] } }`

## Architecture (vertical slice)
- `Demo.VatCalculator.Core` — pure domain: `VatPercent`, `VatCalculator`,
  `VatCalculationValidator`, request/response records, `MoneyConstants`. No
  ASP.NET dependencies; fully unit-testable.
- `Demo.VatCalculator.Api` — minimal API endpoint + uniform error mapping
  (ProblemDetails). No business logic.
- `Demo.VatCalculator.Ui` — Angular app (`app/vat-calculator/` feature slice:
  standalone component, typed forms, signals, Material, service, Vitest specs).
  Dev via `proxy.conf.json` `/api` -> API http port. Desktop responsive
  (>=1280px); mobile out of scope.

## Repo layout
```
README.md
Demo.VatCalculator.slnx
src/Demo.VatCalculator.Core/          Demo.VatCalculator.Core.Tests/
src/Demo.VatCalculator.Api/           Demo.VatCalculator.Api.Tests/
src/Demo.VatCalculator.Ui/
docs/plan.md
docs/work-items.md
```

## Test map
- Core.Tests (xUnit): formula tables incl. half-up boundaries (0.005, 33.325),
  all validation rules, max boundary, invariant scan `net+vat==gross`.
- Api.Tests (xUnit + WebApplicationFactory): 200 per rate x input type; 400 for
  no/multiple amount, zero, negative, too precise, too large, unknown field,
  string-typed amount, `"1,23"`; ProblemDetails shape + content-type checks.
- Ui (Vitest): component form validation; service maps response + errors.

## Non-goals (deliberately not built)
Auth/authorization, persistence/audit, multi-currency, invoice generation,
mobile optimization, CI/CD, containerization, API versioning package.