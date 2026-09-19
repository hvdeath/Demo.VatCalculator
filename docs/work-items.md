# Demo.VatCalculator — Work Items

Each work item is verified and reviewed by the owner before anything is
committed. Work items are executed in order; the next one starts only after the
current one is approved.

| # | Work item | Status |
|---|-----------|--------|
| 1 | Scaffold `Demo.VatCalculator.slnx` + projects (Core, Core.Tests, Api, Api.Tests) | done (awaiting review) |
| 2 | Core domain: `VatPercent`, request/response contracts, `VatCalculator`, `VatCalculationValidator`, `MoneyConstants` | done (awaiting review) |
| 3 | Core unit tests: formula tables, rounding boundaries, validation rules, max boundary, `net+vat==gross` scan | pending |
| 4 | API: minimal endpoint `POST /api/v1/vat/calculate`, error contract (ProblemDetails), OpenAPI + Swagger UI, http profile | pending |
| 5 | API integration tests (`WebApplicationFactory`): happy paths + full 400 matrix + error shape | pending |
| 6 | README.md rewrite: rounding, validation boundaries, API contract, architecture, non-goals | pending |
| 7 | Scaffold Angular app in `src/Demo.VatCalculator.Ui` (standalone, SCSS, strict, Vitest) + proxy | pending |
| 8 | UI feature slice: `vat-calculator` component (signals, typed forms, Material), service, a11y, responsive layout | pending |
| 9 | UI tests (Vitest): component + service specs; `ng build` clean | pending |
| 10 | E2E smoke: run API + `ng serve`, exercise via UI and HTTP | pending |

## Notes
- API is minimal API only; Swagger via `AddOpenApi()` + `Swashbuckle.AspNetCore.SwaggerUi`.
- Business rules live only in Core; validator/calculator carry no ASP.NET dependencies.
- Rounding: `MidpointRounding.AwayFromZero`, 2 dp, single rounding pass, remainder-derived third amount.
- Max amount: `1_000_000_000.00`; unknown JSON fields rejected.