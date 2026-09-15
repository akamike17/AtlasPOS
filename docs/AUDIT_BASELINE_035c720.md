# AtlasPOS — baseline de auditoría

Fecha: 2026-09-15  
Baseline solicitado: `035c720c0b1c12e9dde6a616cdd7cec099e864ff`  
Repositorio local: `PuntoDeVentaAtlas.Web`

## Evidencia inicial

- `dotnet --info`: SDK 10.0.401, runtimes .NET 8.0.31 y ASP.NET Core 8.0.31 disponibles.
- `dotnet restore`: correcto.
- `dotnet build --configuration Release`: correcto, 0 advertencias y 0 errores.
- Tests explícitos del proyecto: 12 correctos.
- La solución original no incluía el proyecto de tests; se corrigió `PuntoDeVentaAtlas.Web.slnx`.

## Riesgos P0 observados y atendidos

1. El arranque ejecutaba `MigrateAsync` y sembraba demo sin distinguir Production. Ahora la migración es explícita, DemoMode está prohibido en Production y existe un perfil Lab.
2. El checkout comprobaba stock por línea, por lo que dos líneas del mismo producto podían superar la existencia. Ahora agrupa por producto y calcula precios/impuestos en servidor.
3. El `CustomerId` recibido no se validaba por Store. Ahora un cliente de otra sucursal es rechazado.
4. Stock inicial/cambiado desde catálogo no generaba movimiento. Ahora toda diferencia genera `InventoryMovement` y auditoría dentro de una transacción.
5. Movimientos y cierre de caja no tenían transacción explícita. Ahora ambos usan Serializable y commit explícito.

## No probado en este entorno

MySQL real, concurrencia entre conexiones, fresh/upgrade con datos representativos, backup/restore, browser E2E y hardware físico requieren un entorno Lab externo. No se declara evidencia ficticia.
