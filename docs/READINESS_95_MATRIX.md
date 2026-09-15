# Matriz de readiness

La puntuación sólo usa `PROVEN` con evidencia reproducible. Las pruebas MySQL usan bases efímeras; `atlas_pos` no se reseteó ni recibió mutaciones.

| Área | Peso | Estado | Puntos |
|---|---:|---|---:|
| Build/start/config | 5 | PARTIAL | 3 |
| Sale atomicity/idempotency/concurrency | 15 | PARTIAL | 12 |
| Cash shifts/money | 10 | PARTIAL | 7 |
| Inventory/purchases | 10 | PARTIAL | 7 |
| Returns | 8 | PARTIAL | 6 |
| Store isolation/security | 10 | PARTIAL | 6 |
| Auth/roles/workstations | 8 | PARTIAL | 6 |
| MySQL migrations/upgrade | 7 | PROVEN | 7 |
| Backup/recovery | 5 | PROVEN | 5 |
| UI/browser E2E | 8 | PARTIAL | 6 |
| Automated tests | 6 | PARTIAL | 5 |
| Peripheral software behavior | 3 | PARTIAL | 2 |
| Manufacturing | 2 | PARTIAL | 1 |
| Docs/installability/observability | 3 | PARTIAL | 2 |
| **TOTAL** | **100** | **NO READY** | **76** |

Blockers abiertos: no hay mutational testing formal ni pruebas browser de compras/devoluciones/fabricación; falta prueba de producción instalada y validación completa de periféricos/manufactura. Por eso no se declara `ATLASPOS >=95%`.
