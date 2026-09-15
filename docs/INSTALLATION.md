# Instalación y operación

## Requisitos

- .NET 8 Runtime para ejecución framework-dependent, o Windows x64 para el paquete self-contained.
- MySQL 8 compatible con Pomelo Entity Framework Core.
- Cuenta de aplicación MySQL con permisos de lectura/escritura de tablas y sin privilegios de administración de esquema en Production.
- HTTPS terminado en Kestrel o reverse proxy para Production.

## Configuración

No se guardan secretos en `appsettings.json`. Configura `ConnectionStrings__AtlasMySql` y, sólo durante bootstrap explícito, `Atlas__InitialAdminPassword` mediante Secret Manager, entorno seguro o vault.

Development no siembra automáticamente. Lab usa `ASPNETCORE_ENVIRONMENT=Lab` y el perfil `appsettings.Lab.json`, que habilita migración y demo sólo para pruebas.

## Primer administrador

El bootstrap requiere `Atlas:AllowInitialAdminBootstrap=true` y una contraseña inicial externa de al menos 10 caracteres. Production no crea una cuenta conocida por sí mismo. Tras bootstrap, deshabilita el flag y elimina la variable secreta del proceso.

## Paquete Lab

`deploy/Build-TestPackage.ps1` construye el paquete; `Start-Atlas.ps1` lo ejecuta como Lab con MySQL local de Docker. Los instaladores deliberadamente distribuidos se regeneran con los scripts de `deploy/` e `installer/`; no se reconstruyen desde binarios de QA rastreados.

## Troubleshooting

- `/health` confirma conectividad MySQL.
- Si el login falla, comprueba que el usuario está activo y que la contraseña fue configurada durante bootstrap.
- Si una terminal está deshabilitada, un administrador debe habilitarla desde Workstations.
- Si un periférico falla, la UI conserva estado `failed`/`not_tested` y no lo presenta como hardware probado.
