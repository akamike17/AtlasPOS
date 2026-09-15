# Evidencia de pruebas

## Comandos ejecutados

```powershell
dotnet restore PuntoDeVentaAtlas.Web.slnx -p:NuGetAudit=false --ignore-failed-sources
dotnet build PuntoDeVentaAtlas.Web.slnx --configuration Release --no-restore
dotnet test PuntoDeVentaAtlas.Web.slnx --configuration Release --no-build
dotnet build tests\Integration.MySql\Integration.MySql.csproj --configuration Release --no-restore
dotnet test tests\Integration.MySql\Integration.MySql.csproj --configuration Release --no-build
```

Resultado: restore y Release correctos; build sin advertencias; pruebas unitarias verdes; MySQL integrado **12/12**.

## Cobertura MySQL

Las pruebas crean y eliminan bases efímeras con MySQL local, sin tocar `atlas_pos`. Cubren migración fresh, upgrade desde `20260813201012_SnapshotSyncMultiTerminal`, snapshot lógico backup/restore, idempotencia, venta concurrente de última unidad, devoluciones parciales y concurrentes, aislamiento cross-store, rollback de compra inválida, apertura concurrente de turno, clientes, inventario y fabricación atómica.

## Browser E2E

Se validó contra una base temporal separada: login de `admin@atlas.local`, inventario, cliente, compra, receta, orden de producción, apertura/cierre de turno, venta en efectivo, devolución y auditoría. La consola no reportó errores, no hubo `pageerror` y no hubo respuestas HTTP 4xx/5xx. También se verificó que una ruta protegida redirige al login sin sesión.

## Seguridad de la evidencia

La contraseña de laboratorio se suministró sólo por variables de proceso; no se guardó ni imprimió. No se ejecutaron operaciones destructivas sobre `atlas_pos`.

## Límites conocidos

No se ejecutó mutational testing formal, no se instaló un paquete Production en una máquina limpia y no se conectó hardware físico, SDK bancario o PAC real. Esas validaciones permanecen externas.
