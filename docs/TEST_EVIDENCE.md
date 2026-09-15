# Evidencia de pruebas

## Ejecutado

```powershell
dotnet restore PuntoDeVentaAtlas.Web.slnx -p:NuGetAudit=false --ignore-failed-sources
dotnet build PuntoDeVentaAtlas.Web.slnx --configuration Release --no-restore
dotnet test tests\PuntoDeVentaAtlas.Web.Tests\PuntoDeVentaAtlas.Web.Tests.csproj --configuration Release --no-restore
$env:ATLAS_MYSQL_ADMIN_CONNECTION='...'; dotnet test tests\Integration.MySql\Integration.MySql.csproj --configuration Release --no-restore
```

Resultado observado en esta sesión: restore correcto; Release verde (0 warnings, 0 errors); 12 tests unitarios correctos y 9 pruebas MySQL correctas.

## MySQL / runtime verificado

- MySQL80 activo en `127.0.0.1:3306`; conexión EF exitosa a `atlas_pos` con credencial suministrada externamente.
- `dotnet ef migrations list`: 8 migraciones aplicadas, hasta `20260813220000_AddPartialReturnLines`.
- App real sin migrar ni sembrar: `GET /health` → 200 `Healthy`.
- `GET /Auth/Login` → 200.
- Ruta protegida `GET /Pos/Devices` sin sesión → 302 al login.
- Integration.MySql sobre bases efímeras: fresh/migraciones, upgrade desde `20260813201012_SnapshotSyncMultiTerminal`, backup/restore a base vacía, idempotencia, venta concurrente de última unidad, devoluciones concurrentes, aislamiento de producto, rollback de compra cross-store y apertura concurrente de turno.
- Backup operativo Lab: `mysqldump --single-transaction --routines --triggers --hex-blob` correcto; SHA-256 observado `984ADC923B4DDE803A9578E73305EF842EA9B35D025C4EA35088EB72BEB5FDB5`; importación a otra base correcta (25 tablas, 5 productos). Ambas bases y el dump temporal fueron eliminados después de verificar.
- Browser Lab: login de `admin@atlas.local`, apertura de turno, venta de `TEST-IDEM` por efectivo, corte esperado/contado `$10.00` y cierre auditado. Consola sin errores/advertencias.

La contraseña no se guardó ni se imprimió.

El warning `NU1900` puede aparecer al ejecutar tests con auditoría de vulnerabilidades si NuGet no está accesible; no es un fallo de compilación ni se oculta en el proyecto.

## Cobertura disponible

- `SaleCalculator`: redondeo comercial, pesables, IVA y descuento acotado.
- `ScaleFrameParser`: gramos, kilogramos, libras, peso estable y tramas inválidas.

## Evidencia pendiente que bloquea READY

No se probó mutational testing formal, compras/devoluciones/fabricación desde browser, ni un ciclo operativo `mysqldump` → checksum → restore. Tampoco se instaló un paquete Production en una máquina limpia.
