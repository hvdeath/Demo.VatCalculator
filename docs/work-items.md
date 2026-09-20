# Demo.VatCalculator — Work Items

Each work item is verified and reviewed by the owner before anything is
committed. Work items are executed in order; the next one starts only after the
current one is approved.

| # | Work item | Status |
|---|-----------|--------|
| 1 | Scaffold `Demo.VatCalculator.slnx` + projects (Core, Core.Tests, Api, Api.Tests) | committed `c50726a` |
| 2 | Core domain: `VatPercent`, request/response contracts, `VatAmountCalculator`, `VatCalculationValidator`, `MoneyConstants` | committed `1b897f0` |
| 3 | Core unit tests: formula tables, rounding boundaries, validation rules, max boundary, `net+vat==gross` scan | committed `4a7e633` |
| 4 | API: minimal endpoint `POST /api/v1/vat/calculate`, error contract (ProblemDetails), OpenAPI + Swagger UI, http profile | committed `3c9662f` |
| 5 | API integration tests (`WebApplicationFactory`): happy paths + full 400 matrix + error shape | committed `29f0f56` |
| 6 | README.md rewrite: rounding, validation boundaries, API contract, architecture, non-goals | committed `953c588` |
| 7 | Scaffold Angular app in `src/Demo.VatCalculator.Ui` (standalone, SCSS, strict, Vitest) + proxy | committed `a0f154c` |
| 8 | UI feature slice: `vat-calculator` component (signals, typed forms, Material), service, a11y, responsive layout | committed `a5564b9` |
| 9 | UI tests (Vitest): component + service specs; `ng build` clean | committed `79c0011` |
| 10 | E2E smoke: run API + `ng serve`, exercise via UI and HTTP | done (awaiting review) |

## Notes
- API is minimal API only; Swagger via `AddOpenApi()` + `Swashbuckle.AspNetCore.SwaggerUi`.
- Business rules live only in Core; validator/calculator carry no ASP.NET dependencies.
- Rounding: `MidpointRounding.AwayFromZero`, 2 dp, single rounding pass, remainder-derived third amount.
- Max amount: `1_000_000_000.00`; unknown JSON fields rejected.