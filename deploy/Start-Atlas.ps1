$ErrorActionPreference = 'Stop'
$deployDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$appDirectory = Join-Path $deployDirectory 'app'
$envFile = Join-Path $deployDirectory '.env'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Instala e inicia Docker Desktop antes de arrancar Atlas POS.'
}

if (-not (Test-Path -LiteralPath $envFile)) {
    $databasePassword = -join ((48..57)+(65..90)+(97..122) | Get-Random -Count 30 | ForEach-Object {[char]$_})
    $rootPassword = -join ((48..57)+(65..90)+(97..122) | Get-Random -Count 34 | ForEach-Object {[char]$_})
    @("ATLAS_DB_PASSWORD=$databasePassword","ATLAS_DB_ROOT_PASSWORD=$rootPassword") | Set-Content -LiteralPath $envFile -Encoding utf8
}

$settings = @{}
Get-Content -LiteralPath $envFile | Where-Object {$_ -match '='} | ForEach-Object {
    $name,$value = $_ -split '=',2
    $settings[$name] = $value
}

Push-Location $deployDirectory
try { docker compose up -d --wait }
finally { Pop-Location }

$secureAdminPassword = Read-Host 'Contraseña inicial de admin@atlas.local (mínimo 10 caracteres; después del primer inicio ya no se modifica)' -AsSecureString
$adminPassword = [System.Net.NetworkCredential]::new('', $secureAdminPassword).Password
if ($adminPassword.Length -lt 10) { throw 'La contraseña inicial debe tener al menos 10 caracteres.' }

# Este paquete es un entorno Lab autocontenido. Production usa Upgrade-Atlas.ps1
# y arranca con Atlas:ApplyMigrations=false.
$env:ASPNETCORE_ENVIRONMENT = 'Lab'
$env:ASPNETCORE_URLS = 'http://127.0.0.1:5099'
$env:ConnectionStrings__AtlasMySql = "Server=127.0.0.1;Port=3306;Database=atlas_pos;User=atlas_app;Password=$($settings.ATLAS_DB_PASSWORD);Allow User Variables=true;"
$env:Atlas__InitialAdminPassword = $adminPassword

$executable = Join-Path $appDirectory 'PuntoDeVentaAtlas.Web.exe'
$assembly = Join-Path $appDirectory 'PuntoDeVentaAtlas.Web.dll'
if (Test-Path -LiteralPath $executable) { $process = Start-Process -FilePath $executable -WorkingDirectory $appDirectory -WindowStyle Hidden -PassThru }
elseif (Test-Path -LiteralPath $assembly) { $process = Start-Process -FilePath 'dotnet' -ArgumentList $assembly -WorkingDirectory $appDirectory -WindowStyle Hidden -PassThru }
else { throw "No se encontró la aplicación en $appDirectory. Ejecuta Build-TestPackage.ps1 desde el proyecto." }

for ($attempt=0; $attempt -lt 30; $attempt++) {
    Start-Sleep -Seconds 1
    try { if ((Invoke-WebRequest 'http://127.0.0.1:5099/health' -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { Start-Process 'http://127.0.0.1:5099'; Write-Host "Atlas POS listo. Proceso $($process.Id)."; exit 0 } } catch { }
    if ($process.HasExited) { throw "Atlas POS terminó durante el arranque (código $($process.ExitCode))." }
}
throw 'La aplicación arrancó, pero MySQL no estuvo listo dentro del tiempo esperado.'
