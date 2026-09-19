# Demo.VatCalculator

A web portal for calculating **Net**, **VAT**, and **Gross** amounts for purchases in
Austria. The user provides exactly one of the three amounts and a VAT rate; the
application returns all three.

- Backend: **.NET 10 LTS**, C# 14, ASP.NET Core (minimal API), versioned REST API, Swagger UI.
- Frontend: **Angular 22** with Angular Material, standalone components, typed forms, signals.
- Domain rules live in a dependency-free core library; API and UI stay thin.

## Architecture

Pragmatic vertical-slice structure with clear boundaries.

| Project | Responsibility |
|---|---|
| `Demo.VatCalculator.Core` | Pure domain + use case. VAT formula, money rounding, validation. No ASP.NET dependencies. |
| `Demo.VatCalculator.Api` | Minimal API endpoint, JSON contract enforcement, uniform HTTPS error contract, OpenAPI/Swagger. No business logic. |
| `Demo.VatCalculator.Ui` | Angular 22 SPA. Lightweight form validation mirror; server is authoritative. |
| `*.Tests` / UI specs | Core unit tests (xUnit), API integration tests (`WebApplicationFactory`), UI tests (Vitest). |

The core folder `VatCalculation/` is a vertical slice:
`VatAmountCalculator` (pure math) → `VatCalculationValidator` (business rules) →
`VatCalculationService` (use case) → contracts. The API endpoint only maps results
to HTTP responses; business rules are independently testable and never duplicated
in other layers.

## Calculation and rounding

Money is handled as `decimal` (128-bit, exact base-10). A **single rounding pass**
happens per calculation; the third amount is always derived as a remainder so that

```
net + vat == gross
```

holds exactly for every response. Let `r = rate / 100` and
`round(x) = decimal.Round(x, 2, MidpointRounding.AwayFromZero)`.

| Given | Computed |
|---|---|
| net | `vat = round(net · r)`, `gross = net + vat` |
| gross | `net = round(gross / (1 + r))`, `vat = gross − net` |
| vat | `net = round(vat / r)`, `gross = net + vat` |

**Why `AwayFromZero` (round half up)?** It is the convention used for euro currency
rounding (Reg. EC 1103/97) and matches typical accounting/consumer expectations
(`0.5` rounds up). The alternative, banker's rounding (`ToEven`), is bias-neutral but
less intuitive for money. The rule is a single constant in `Money.Round`.

**Worked examples**

| Input | Net | VAT | Gross |
|---|---|---|---|
| net 100 @ 20% | 100.00 | 20.00 | 120.00 |
| net 33.33 @ 20% | 33.33 | 6.67 | 40.00 |
| gross 39.99 @ 20% | 33.33 | 6.66 | 39.99 |
| vat 6.67 @ 20% | 33.35 | 6.67 | 40.02 |

**Non-reconstruction boundary.** Because there is only one rounding pass, calculating
"backwards" does not always recover the original input. Example: `net 33.33 @ 20%`
gives `vat 6.67`, but the inverse `vat 6.67 @ 20%` yields `net 33.35`, not `33.33`.
This is inherent to 2-decimal money rounding and is stated here explicitly; the
examples above are covered by automated tests.

## Validation

Field-level, machine-readable errors. Codes are stable and prefixed `vat.`.

| Rule | Error code | Field |
|---|---|---|
| Rate must be one of `10`, `13`, `20` | `vat.rateNotSupported` | `rate` |
| Exactly one amount must be provided | `vat.noAmount` | `amount` |
| More than one amount provided | `vat.multipleInputs` | each provided amount field |
| Amount must be greater than zero | `vat.amountNotPositive` | amount field |
| Amount has more than 2 decimal places | `vat.excessivePrecision` | amount field |
| Amount exceeds `1,000,000,000.00` | `vat.amountTooLarge` | amount field |
| Request JSON cannot be parsed / number given as string | `vat.malformedBody` | `request` |
| Undocumented JSON property present | `vat.unknownField` | the unknown property |

Input amounts are strictly JSON numbers: a string like `"100"` or `"1,23"` is rejected
(`vat.malformedBody`). Locale-style separators are normalized **client-side** in the UI
(`1,23` / `1.23` both accepted and sent as `1.23`). The API itself accepts numbers only.

## API contract (v1)

Versioned by URL path (`/api/v1/...`); kept deliberately simple instead of adopting an
API-versioning library (KISS — a second version is not imminent).

```
POST /api/v1/vat/calculate
Content-Type: application/json

{ "rate": 20, "net": 100 }
```

Success `200 OK` (`application/json`):

```json
{ "rate": 20, "net": 100, "vat": 20, "gross": 120 }
```

Validation failure `400 Bad Request` (`application/problem+json`):

```json
{
  "type": "urn:demo-vat:validation-failed",
  "title": "Validation failed.",
  "status": 400,
  "errors": {
    "net": [{ "code": "vat.multipleInputs", "message": "Only one amount may be provided; more than one amount is ambiguous." }],
    "gross": [{ "code": "vat.multipleInputs", "message": "Only one amount may be provided; more than one amount is ambiguous." }]
  }
}
```

Error contract is uniform across error kinds:

| Situation | `type` | `errors` |
|---|---|---|
| Business rule violation | `urn:demo-vat:validation-failed` | field → `[{code, message}]` |
| Undocumented field (e.g. `tax`) | `urn:demo-vat:unknown-field` | property → `vat.unknownField` |
| Malformed/unparseable body | `urn:demo-vat:malformed-body` | `request` → `vat.malformedBody` |

Behavior stated and tested: a request with **multiple amounts** is rejected (contradictory
input), and one with **unknown fields** is rejected (strict contract) rather than silently
ignoring the extra data.

## Development

Prerequisites: .NET 10 SDK, Node 20.19+ (npm), Angular 22 CLI.

```bash
# backend
dotnet run --project src/Demo.VatCalculator.Api   # http://localhost:5180
# swagger UI:  http://localhost:5180/swagger
# openapi doc: http://localhost:5180/openapi/v1.json

# tests
dotnet test Demo.VatCalculator.slnx
cd src/Demo.VatCalculator.Ui && npm test && npm run build

# frontend dev server (proxies /api to :5180)
cd src/Demo.VatCalculator.Ui && ng serve
```

## Deliberately not built

- **Authentication/authorization** — no user context needed for a stateless calculator.
- **Persistence / audit log** — calculations are deterministic; no store required.
- **Multi-currency** — Austria/EUR only; rates are the three Austrian VAT rates.
- **Mobile-optimized layout** — desktop-first; responsive only at standard desktop widths.
- **API versioning library, generic repository, DI abstractions** — the single version,
  single calculator, and tiny dependency graph do not warrant them (YAGNI).