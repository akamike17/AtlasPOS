# Probar Atlas POS en otra computadora

Requisitos: Windows 10/11 de 64 bits, Docker Desktop iniciado y los controladores oficiales de la báscula/impresora. La aplicación y MySQL quedan sólo en la computadora local; no se exponen a Internet.

1. Descomprime `AtlasPOS-TestPC.zip` en una carpeta permanente.
2. Abre PowerShell en esa carpeta y ejecuta `powershell -ExecutionPolicy Bypass -File .\Start-Atlas.ps1`. La primera vez Docker descargará MySQL y puede tardar varios minutos.
3. Escribe una contraseña de al menos 10 caracteres e inicia sesión con `admin@atlas.local`.
4. Abre **Dispositivos > Detectar hardware**.
5. Para una impresora Windows, pulsa **Usar dispositivo**, guarda y usa **Probar conexión / imprimir**. Debe salir un ticket físico.
6. Para báscula, colócala con peso, configura **Báscula** y pulsa **Detectar báscula automáticamente**. Atlas guarda COM, velocidad y modo detectado; después ejecuta la prueba.
7. El lector de códigos USB debe estar en modo HID/teclado. En Nueva venta, enfoca la búsqueda y escanea un código.

`Stop-Atlas.ps1` detiene la aplicación y MySQL sin borrar datos. La base permanece en el volumen `atlas_mysql_data`. No ejecutes `docker compose down -v`, porque elimina la base de prueba.

Terminal bancaria y CFDI sólo se habilitan con credenciales sandbox/producción y el contrato del proveedor. Detectar un USB nunca se interpreta como cobro aprobado.
