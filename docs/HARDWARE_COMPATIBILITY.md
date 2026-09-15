# Compatibilidad de hardware

Los contratos están en `Integrations/IntegrationContracts.cs`. Impresora, báscula, cajón, terminal, firma y PAC se aíslan por terminal y usan timeout/cancellation; los estados de software incluyen `failed`, `driver_required` y `not_configured`.

La validación browser y los contratos demuestran el comportamiento del software y la configuración por caja. No demuestran que un modelo físico, SDK bancario o PAC específico esté certificado o conectado. La compatibilidad física queda **EXTERNAL / NOT PROVEN** hasta contar con proveedor, modelo, credenciales de prueba y evidencia reproducible.

Los secretos de periféricos se guardan protegidos, no se exponen en listados y no se escriben en logs. El Agent debe limitar su escucha a la red de la caja y rechazar solicitudes sin token válido.
