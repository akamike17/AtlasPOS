# Compatibilidad de hardware

Los contratos están en `Integrations/IntegrationContracts.cs`. Impresora, báscula, cajón, terminal, firma y PAC se aíslan por terminal y usan timeout/cancellation; un fallo se refleja como `failed`, `driver_required` o `not_configured`.

Los simuladores y pruebas de protocolo sólo demuestran el software del adapter. No demuestran que un modelo físico, SDK bancario o PAC específico esté certificado o conectado. La validación física queda `EXTERNAL / NOT PROVEN` hasta contar con proveedor, modelo, credenciales de prueba y evidencia reproducible.

Los tokens se guardan protegidos, no se exponen en listados y no se escriben en logs. El Agent debe limitar su escucha a la red de la caja y rechazará solicitudes sin token válido.
