# Upgrade de esquema

Las migraciones históricas no se editan. Se agregan migraciones correctivas y se valida el snapshot de EF.

## Fresh

Base vacía → `deploy/Upgrade-Atlas.ps1` con respaldo de base vacía → migraciones → bootstrap explícito → health → login/setup.

## Upgrade

1. Restaurar un dump representativo en una base Lab.
2. Crear respaldo previo.
3. Ejecutar `deploy/Upgrade-Atlas.ps1` con credenciales de upgrade.
4. Revisar `__EFMigrationsHistory`, claves foráneas, índices compuestos por Store, tipos decimal y datos conservados.
5. Arrancar la aplicación con `Atlas:ApplyMigrations=false`.

Production nunca ejecuta `MigrateAsync` de forma ciega en cada arranque. El script de upgrade falla antes de modificar si el respaldo no se pudo crear.

La prueba integrada crea una base vacía, migra hasta `20260813201012_SnapshotSyncMultiTerminal`, inserta un producto, aplica el resto de migraciones y confirma que el producto conserva existencia y datos.
