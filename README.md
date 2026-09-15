# Atlas POS

Punto de venta universal en ASP.NET Core MVC sobre .NET 8, JavaScript nativo y esquema exclusivo para MySQL 8.

## Estado actual

La interfaz ejecutable incluye catálogo, código de barras, artículos pesables, carrito, impuestos, descuento, clientes, pagos, cambio, inventario, caja, reportes y centro de dispositivos. La persistencia usa Entity Framework Core 8, Pomelo y MySQL 8; ventas, pagos, inventario y auditoría se guardan transaccionalmente.

Las migraciones están en `Data/Migrations` y se aplican al iniciar. La cadena `AtlasMySql` se configura mediante Secret Manager o variables de entorno; nunca se guardan contraseñas reales en el repositorio.

## Ejecutar

```powershell
dotnet run
```

## Paquete para otra computadora

`deploy/Build-TestPackage.ps1` crea `release/AtlasPOS-TestPC.zip`, autocontenido para Windows x64. La computadora de prueba sólo necesita Docker Desktop y los controladores oficiales de sus periféricos; no necesita instalar .NET ni MySQL manualmente. Al ejecutar `Start-Atlas.ps1`, se crea MySQL 8 con contraseñas aleatorias, se aplican migraciones, se solicita la contraseña inicial del administrador y se abre Atlas en el navegador.

El acceso requiere sesión. La cuenta inicial es `admin@atlas.local`; su contraseña se configura en Secret Manager con `Atlas:InitialAdminPassword`. Los roles disponibles son `Administrator`, `Manager` y `Cashier`.

## Operación y verificación

- `GET /health` comprueba la conexión MySQL.
- Un administrador puede descargar un respaldo lógico comprimido desde Auditoría.
- Las migraciones se aplican automáticamente al iniciar.
- `dotnet test tests/PuntoDeVentaAtlas.Web.Tests` ejecuta las pruebas de cálculo fiscal.
- Cada cajero debe abrir su propio turno con fondo inicial antes de cobrar. Caja permite registrar entradas, retiros y corte; el efectivo esperado descuenta el cambio entregado porque sólo se aplica a la venta el importe efectivamente cobrado.
- Cada intento de venta lleva un identificador único. Los reintentos de red devuelven la venta original y no vuelven a descontar inventario.
- Tarjeta y transferencia requieren capturar una referencia o autorización. Hasta conectar una terminal, esa referencia representa la autorización obtenida en el dispositivo externo.
- Las devoluciones pueden ser parciales por renglón y cantidad. Sólo administradores o encargados pueden procesarlas; cada renglón permite decidir si la mercancía regresa a existencia y nunca puede devolverse más de lo vendido.
- Inventario permite conteo físico, entradas, salidas y mermas manuales. Cada ajuste exige motivo, genera movimiento y auditoría, y no puede producir existencia negativa.
- La prueba de una impresora instalada en Windows o conectada por TCP envía un ticket físico ESC/POS (o una etiqueta ZPL para Zebra). Una conexión abierta por sí sola ya no se reporta como impresión correcta.

Atajos: `F2` nueva venta, `F3` búsqueda/escáner y `F4` cobrar. Los lectores HID funcionan como teclado. Los contratos para terminal integrada, terminal semiintegrada, liga de pago, PAC CFDI, báscula, ESC/POS y firma están en `Integrations/IntegrationContracts.cs`; cada hardware requiere el SDK/protocolo y credenciales de su fabricante.

## Próximo hito para producción

Para activar integraciones externas falta elegir proveedor y entregar credenciales/SDK del PAC, terminal bancaria y hardware específico. También se recomienda programar el respaldo lógico en infraestructura y ensayar restauración antes de abrir una sucursal.

## Dashboard de clientes y periféricos

El módulo Clientes incluye métricas de completitud, búsqueda instantánea y exportación PDF. El arranque de desarrollo completa de forma idempotente un directorio demostrativo de 100 clientes; nunca duplica registros cuando ya existen 100 o más.

La sección `Peripherals` de `appsettings.json` conserva únicamente parámetros no secretos. Las credenciales deben suministrarse mediante Secret Manager o variables de entorno. Cada proveedor implementa el puerto correspondiente de `Integrations/IntegrationContracts.cs`: `ICardPaymentConnector`, `IInvoiceConnector`, `IScaleConnector`, `ITicketPrinter` o `ISignaturePad`.
