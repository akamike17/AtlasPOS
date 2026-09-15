# Compatibilidad de hardware

## Software demostrado

La configuración de impresora, báscula, cajón, scanner, firma, terminal e invoice se filtra por `StoreId + WorkstationId`; Caja A no puede leer ni accionar la configuración privada de Caja B. Los secretos de periféricos se protegen con DataProtection y no se devuelven en listados.

Los contratos soportan resultados `connected`, `configuration_ready`, `not_configured`, `driver_required`, `read_failed`, `agent_failed` y `failed`. La báscula valida unidad, rango, peso negativo, ruido y timeout; una trama inválida nunca se convierte en cantidad de venta.

Los adaptadores ESC/POS, ZPL, cajón TCP y Atlas Peripheral Agent tienen límites y cancellation/timeout. Una impresión fallida ocurre después de confirmar la venta y no duplica Sale, Payment ni InventoryMovement. La apertura manual exige autorización, motivo y auditoría.

## EXTERNAL / NOT PHYSICALLY PROVEN

No se conectó un modelo físico, SDK bancario, terminal de pago ni PAC real. La certificación de Epson/Star/Zebra/Bixolon, báscula Torrey/Dibal/Mettler/CAS, terminal bancaria, firma y CFDI requiere hardware, controlador, proveedor y credenciales externas. Reconocer un puerto o responder a un simulador no equivale a certificar compatibilidad física.
