# Compatibilidad de hardware POS

El centro de dispositivos detecta impresoras instaladas en Windows, puertos COM y dispositivos USB. La coincidencia por fabricante sugiere un perfil, pero el operador debe confirmar modelo, conexión y protocolo antes de habilitarlo.

## Impresoras

- Epson TM: ESC/POS por red/Bluetooth y OPOS/ePOS mediante controladores oficiales. Windows documenta soporte POS para TM-T88V, TM-T70, TM-T20, TM-U220 y móviles P20/P60/P80. https://learn.microsoft.com/windows/uwp/devices-sensors/pos-device-support
- Star Micronics: StarPRNT/StarIO para Windows. El SDK descubre red, USB, Bluetooth y BLE, consulta estado y errores. https://starmicronics.com/support/developers/windows-sdks/
- Zebra: Link-OS para series ZD, ZQ, ZT y ZE; ofrece descubrimiento y SDK .NET/Java. https://techdocs.zebra.com/link-os/
- Bixolon: sus modelos SRP publican Windows POS SDK/OPOS; el paquete exacto depende del modelo. https://www.bixolon.com/
- Atlas incluye adaptadores sin dependencia para ESC/POS y ZPL por TCP 9100. No se envían trabajos hasta que un administrador guarde y pruebe el host.

## Básculas

### Conexión recomendada

1. Instalar el controlador USB/serial del fabricante y cerrar cualquier programa como Scale o Bridge Printer que mantenga abierto el COM.
2. En Dispositivos > Báscula, pulsar **Detectar báscula automáticamente** con un objeto sobre el plato.
3. Atlas prueba los puertos disponibles a 9600, 4800, 2400 y 19200 baud, primero escuchando transmisión continua y después enviando `P`.
4. Guardar el puerto y ejecutar **Probar conexión**. Atlas sólo muestra “configurada correctamente” después de recibir y analizar una trama real.

La conexión permanece abierta durante el pesaje, se serializan las lecturas para impedir que dos solicitudes compitan por el COM y se reinicia automáticamente ante desconexión. Se considera estable cuando varias lecturas recientes difieren como máximo 2 gramos. El diagnóstico muestra puerto ocupado, ausencia de respuesta o una muestra sanitizada de la trama no reconocida.

- Torrey y Dibal: se detecta el puerto serie/USB y se prepara el perfil, pero hay que seleccionar el modelo y obtener del fabricante su trama, baud rate, paridad y comando de lectura.
- Mettler Toledo: preferir su Service Object OPOS oficial; la documentación de Mettler describe la interfaz UPOS Scale.
- CAS: usar el protocolo/driver correspondiente al modelo. No se deben probar comandos de otra familia porque una respuesta numérica no garantiza unidad ni estabilidad.

## Terminales

- Mercado Pago Point Smart 1/2 cuenta con integración oficial mediante Orders API. Permite listar terminales, activar modo PDV, crear/consultar/cancelar órdenes y reembolsar. Requiere cuenta, aplicación y Access Token guardado como secreto. https://www.mercadopago.com.mx/developers/es/docs/mp-point/overview
- Clip, NetPay y terminales bancarias se mantienen como perfil semiintegrado. Sus contratos, SDK, certificación y llaves se obtienen durante el alta comercial. Atlas no considera un dispositivo conectado ni un cobro aprobado sólo por reconocer USB/serie.

## Seguridad

Reconocer hardware significa identificar un controlador, nombre o puerto. No equivale a validar el modelo, certificar una terminal ni aprobar un pago. Los secretos permanecen cifrados, la comunicación de pago debe usar HTTPS/OAuth y la confirmación final siempre proviene del proveedor.
