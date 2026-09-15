# Atlas POS en IIS con múltiples cajas

## Servidor

- Instalar IIS, el Hosting Bundle de .NET 8 y MySQL 8.
- Publicar Atlas en IIS con HTTPS y una identidad del Application Pool que pueda leer/escribir `.keys` y la carpeta de respaldos.
- Configurar `ConnectionStrings__AtlasMySql` y `Atlas__InitialAdminPassword` como variables protegidas del servidor.
- Abrir en Windows Firewall únicamente los puertos 80/443 desde la subred privada de las cajas. MySQL 3306 debe permanecer sólo local al servidor.

## Cada caja cliente

- Acceder a `https://SERVIDOR/Client/Setup` y descargar `AtlasPeripheralAgent.zip`.
- Ejecutar `Install-AtlasAgent.ps1` como administrador. Instala un servicio automático y abre TCP 17420 sólo en perfil de red privada.
- El instalador muestra URL y token. En Atlas, un administrador configura báscula e impresora con conexión **Atlas Agent**, URL `http://IP-DE-CAJA:17420` y ese token.
- Asignar IP fija o reserva DHCP a cada caja. En entornos estrictos, restringir la regla del agente a la IP del servidor con `Set-NetFirewallRule`/`Set-NetFirewallAddressFilter`.

## Seguridad y operación

- Cada navegador genera un identificador de terminal persistente. Turnos, ventas y periféricos quedan separados por terminal y usuario.
- El servidor IIS es la única autoridad para inventario, ventas, roles y auditoría. El agente nunca escribe en MySQL.
- La venta usa aislamiento serializable, identificador único y reintento limitado por deadlock; repetir una solicitud devuelve la venta original.
- Cajeros cobran y usan su turno. Sólo Administrator/Manager abre el cajón manualmente, siempre con motivo auditado. La apertura automática sólo ocurre al imprimir una venta en efectivo si la impresora tiene `drawer=auto`.
- El cajón se conecta físicamente al puerto RJ11/RJ12 de la impresora ESC/POS; no debe conectarse directamente a red.
