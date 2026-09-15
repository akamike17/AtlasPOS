# Modelo de seguridad AtlasPOS

## Límites

- Cada consulta/mutación comercial debe filtrar `StoreId` derivado de la sesión, nunca de un ID enviado por el cliente.
- `WorkstationId` proviene del header/cookie de terminal y se vuelve a comprobar contra el Store en middleware y servicios.
- Las rutas mutantes usan antiforgery; el navegador envía `RequestVerificationToken`.
- La autenticación usa cookies HttpOnly, SameSite Lax y Secure en Production. Las contraseñas usan `PasswordHasher<UserEntity>`.
- Las cuentas inactivas no autentican y los errores de login son genéricos.
- Secretos de periféricos se protegen con ASP.NET Data Protection y no se devuelven en endpoints de listado.

## Controles de dominio

El servidor valida cantidades, métodos de pago, referencias no-cash, descuento autorizado, cliente por Store, stock no negativo, proveedor/productos de compras, elegibilidad de devoluciones y permisos por rol.

## Production

Production no crea datos demo y no ejecuta migraciones al arrancar. El upgrade se hace con `deploy/Upgrade-Atlas.ps1`, respaldo previo y una identidad de base de datos con privilegios mínimos. `RequireHttps` está habilitado por defecto.

## Revisión pendiente

Debe ejecutarse una prueba negativa HTTP contra cada endpoint IDOR, una revisión de reverse proxy/forwarded headers, límites de request y una prueba real de restauración. No se deben registrar contraseñas, tokens, connection strings, biometría ni XML fiscal sensible.
