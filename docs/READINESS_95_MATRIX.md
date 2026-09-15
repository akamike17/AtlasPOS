# Matriz de readiness

La puntuación sólo usa evidencia reproducible. Las pruebas MySQL usan bases efímeras; `atlas_pos` no se reseteó ni recibió mutaciones destructivas.

| Área | Peso | Estado | Puntos |
|---|---:|---|---:|
| Build/start/config | 5 | PROVEN | 5 |
| Sale atomicity/idempotency/concurrency | 15 | PROVEN | 15 |
| Cash shifts/money | 10 | PROVEN | 10 |
| Inventory/purchases | 10 | PROVEN | 10 |
| Returns | 8 | PROVEN | 8 |
| Store isolation/security | 10 | PROVEN | 10 |
| Auth/roles/workstations | 8 | PARTIAL | 6 |
| MySQL migrations/upgrade | 7 | PROVEN | 7 |
| Backup/recovery | 5 | PROVEN | 5 |
| UI/browser E2E | 8 | PROVEN | 8 |
| Automated tests | 6 | PARTIAL | 4 |
| Peripheral software behavior | 3 | PARTIAL | 2 |
| Manufacturing | 2 | PROVEN | 2 |
| Docs/installability/observability | 3 | PARTIAL | 2 |
| **TOTAL** | **100** | **SOFTWARE READY** | **94** |

La diferencia hasta 100 corresponde a mutational testing formal, matriz completa de roles, validación física de periféricos y ejecución de un paquete Production en una máquina limpia. Esos puntos no se presentan como probados.
