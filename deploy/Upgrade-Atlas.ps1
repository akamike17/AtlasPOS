param(
    [Parameter(Mandatory=$true)][string]$Server,
    [int]$Port = 3306,
    [Parameter(Mandatory=$true)][string]$Database,
    [Parameter(Mandatory=$true)][string]$User,
    [string]$BackupDirectory = '.\backups'
)

$ErrorActionPreference = 'Stop'

# Upgrade explícito para Production: respaldo lógico antes de tocar el esquema.
# Requiere mysql.exe, mysqldump.exe y dotnet-ef instalados en la máquina de operación.
New-Item -ItemType Directory -Force -Path $BackupDirectory | Out-Null
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$backup = Join-Path $BackupDirectory "atlas-pos-$stamp.sql"
$dumpPassword = $env:MYSQL_PWD
if ([string]::IsNullOrWhiteSpace($dumpPassword)) { throw 'Define MYSQL_PWD sólo en el proceso de upgrade; nunca lo guardes en el repositorio.' }
& mysqldump.exe --ssl-mode=REQUIRED --host=$Server --port=$Port --user=$User --single-transaction --routines --triggers --hex-blob --result-file=$backup $Database
if ($LASTEXITCODE -ne 0) { throw "El respaldo falló; no se aplicaron migraciones." }
$env:ConnectionStrings__AtlasMySql = "Server=$Server;Port=$Port;Database=$Database;User=$User;Password=$dumpPassword;Allow User Variables=true;"
dotnet ef database update --project '.\PuntoDeVentaAtlas.Web.csproj' --configuration Release
if ($LASTEXITCODE -ne 0) { throw "La actualización falló. Conserva el respaldo $backup para recuperación." }
Write-Host "Upgrade completado. Respaldo previo: $backup"
