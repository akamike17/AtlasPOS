# Backup y restore

La aplicación ofrece un snapshot lógico comprimido para operación y una restauración explícita sólo sobre una base vacía. Para Production, el respaldo previo al upgrade debe hacerse con `mysqldump` o la herramienta administrada por infraestructura, incluyendo triggers y rutinas cuando apliquen.

Flujo Lab reproducible:

1. Crear una base MySQL Lab vacía.
2. Arrancar con `ASPNETCORE_ENVIRONMENT=Lab` y `Atlas__InitialAdminPassword` externo.
3. Ejecutar venta, compra, devolución y una mutación de inventario.
4. Crear respaldo y registrar checksum/fecha fuera de la base.
5. Mutar datos sólo en Lab, restaurar en una base Lab nueva y verificar ventas, líneas, pagos, movimientos, existencias, devoluciones, turnos y auditoría.
6. Reiniciar aplicación y repetir un retry con el mismo `ClientOperationId`; debe devolver la venta existente sin duplicar.

La prueba MySQL integrada ejecutó creación de snapshot y restore en una base efímera nueva, verificando sucursal y producto. También se comprobó un ciclo `mysqldump --single-transaction --routines --triggers --hex-blob`, checksum SHA-256 e importación a otra base Lab (25 tablas y 5 productos). Nunca debe apuntar a una base real de negocio.
