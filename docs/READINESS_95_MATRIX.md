# Matriz de readiness

La matriz usa evidencia reproducible de esta iteración; las pruebas MySQL y publish usan entornos temporales.

| Área | Estado | Evidencia |
|---|---|---|
| Build/start/config | PASS | Release build: 0 errores / 0 advertencias |
| Checkout, idempotencia y concurrencia | PASS | MySQL: retry, última unidad, rollback |
| Cash shifts multicaixa | PASS | apertura/cierre independiente y carrera |
| Inventario/compras | PASS | centralizado, cross-store y rollback |
| Devoluciones | PASS | parcial, límite y concurrencia |
| Store/workstation isolation | PASS | SERVER spoof, unknown, disabled, cross-store |
| Roles y sesiones | PASS | metadata de endpoints + revalidación de claims |
| Migraciones fresh/upgrade | PASS | bases efímeras |
| Backup/restore | PASS | SHA/restore y corrupto, truncado, formato incompatible, destino no vacío |
| Browser E2E multicaixa | PASS | SERVER + Caja A/B simultáneas |
| Manufactura | PASS | atomicidad y última materia prima concurrente |
| Periféricos software | PASS | contratos, estados, parser y aislamiento por workstation |
| Production publish/observabilidad | PASS | Production sin demo/migrations, health mínimo, static, restart |
| Hardware físico/proveedores | EXTERNAL | requiere dispositivo, SDK, PAC o credenciales reales |

Conclusión: el software interno queda listo para el alcance demostrado; las dependencias físicas permanecen explícitamente externas.
