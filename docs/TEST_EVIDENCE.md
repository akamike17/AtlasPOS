# Evidencia de pruebas

## Comandos

```powershell
dotnet restore PuntoDeVentaAtlas.Web.slnx --ignore-failed-sources -p:NuGetAudit=false
dotnet build PuntoDeVentaAtlas.Web.slnx --configuration Release --no-restore -m:1
dotnet test tests\\PuntoDeVentaAtlas.Web.Tests\\PuntoDeVentaAtlas.Web.Tests.csproj --configuration Release --no-build
dotnet test tests\\Integration.MySql\\Integration.MySql.csproj --configuration Release --no-build
dotnet publish PuntoDeVentaAtlas.Web.csproj --configuration Release --no-restore --output <TEMP>
```

Resultado final: **Unit 38 PASS / 0 FAIL / 0 SKIP**; **MySQL 19 PASS / 0 FAIL / 0 SKIP**; Release **0 errores / 0 advertencias**.

## MySQL y seguridad

Las suites crean bases efímeras y las eliminan. Cubren migración fresh, upgrade desde `20260813201012_SnapshotSyncMultiTerminal`, backup/restore, corrupción/truncado/formato incompatible/destino no vacío, idempotencia, última unidad concurrente, cross-store, rollback, devoluciones, turnos, manufactura, onboarding, workstation spoof/disabled/unknown, sesiones y actor ID.

La frontera `SERVER` sólo se activa para una petición local del host con configuración interna explícita. Un cliente remoto sin header, con header/cookie inválido, literal `SERVER`, GUID desconocido o workstation de otra tienda no obtiene contexto servidor ni acceso POS.

## Browser E2E

En una base temporal se validaron health, login, dos contextos visibles con terminales explícitas, turnos separados, aperturas y ventas simultáneas, cierre independiente y continuidad de Caja B. Resultado: PASS; 0 respuestas 5xx durante el flujo.

## Production y límites

El publish temporal arrancó en `Production` con `ApplyMigrations=false` y `DemoMode=false`; pasó startup, login, health, static files y restart. `.keys` persistió dentro del publish temporal y no está versionado. El hardware físico, SDK bancario y PAC no se declaran probados.
