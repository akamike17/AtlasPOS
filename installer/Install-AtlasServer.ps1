param([switch]$WhatIfMode)
$ErrorActionPreference='Stop'
if(-not$WhatIfMode-and-not([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)){throw 'Ejecuta el instalador como administrador.'}
$logDirectory=if($WhatIfMode){Join-Path $env:TEMP 'AtlasPOS-InstallerLogs'}else{Join-Path $env:ProgramData 'AtlasPOS\InstallerLogs'};New-Item -ItemType Directory -Force -Path $logDirectory|Out-Null
$log=Join-Path $logDirectory ("server-{0:yyyyMMdd-HHmmss}.log"-f(Get-Date));Start-Transcript -Path $log|Out-Null
$createdSite=$false;$createdPool=$false;$installedMySql=$false
function Step([string]$message){Write-Host "`n== $message ==" -ForegroundColor Cyan}
function Run([scriptblock]$operation){if($WhatIfMode){Write-Host "SIMULACION: $operation";return}& $operation}
function Secret([string]$prompt){$secure=Read-Host $prompt -AsSecureString;$plain=[Net.NetworkCredential]::new('',$secure).Password;if($plain.Length-lt 12){throw 'La clave debe tener al menos 12 caracteres.'};return $plain}
function SqlSecret([string]$prompt){$plain=Secret $prompt;if($plain-notmatch'^[A-Za-z0-9!@#%_\-]{12,64}$'){throw 'Para MySQL usa de 12 a 64 caracteres: letras, numeros y ! @ # % _ -'};return $plain}
try{
    $marker=Join-Path $env:ProgramData 'AtlasPOS\server-installed.json';$existingAtlasInstall=(-not$WhatIfMode-and(Test-Path $marker))
    if(-not[Environment]::Is64BitOperatingSystem){throw 'Atlas Server requiere Windows de 64 bits.'}
    if([Environment]::OSVersion.Version.Major-lt10){throw 'Se requiere Windows 10, Windows 11 o Windows Server compatible.'}
    $systemDrive=Get-PSDrive -Name $env:SystemDrive.TrimEnd(':');if($systemDrive.Free-lt3GB){if($WhatIfMode){Write-Warning 'La PC de simulacion tiene menos de 3 GB libres.'}else{throw 'Se requieren al menos 3 GB libres.'}}
    $profile=Get-NetConnectionProfile -ErrorAction SilentlyContinue|Where-Object{$_.IPv4Connectivity-ne'NoTraffic'}|Select-Object -First 1;if(-not$WhatIfMode-and$profile-and$profile.NetworkCategory-eq'Public'){throw 'La red activa esta como Publica. Cambiala a Privada antes de instalar.'}
    if(-not$WhatIfMode-and-not$existingAtlasInstall-and(Get-NetTCPConnection -LocalPort 8080 -State Listen -ErrorAction SilentlyContinue)){throw 'El puerto 8080 ya esta ocupado por otra aplicacion.'}
    $source=Join-Path $PSScriptRoot 'app';if(-not(Test-Path (Join-Path $source 'PuntoDeVentaAtlas.Web.dll'))){throw 'El instalador no contiene la aplicacion publicada.'}
    $adminPassword=if($WhatIfMode){'Simulation-Admin-123!'}else{Secret 'Clave inicial de admin@atlas.local'}
    $databasePassword=if($WhatIfMode){'Simulation-Database-123!'}else{SqlSecret 'Clave nueva para el usuario atlas_app de MySQL'}
    $newRootPassword=$null
    Step 'Habilitando IIS'
    Run {Enable-WindowsOptionalFeature -Online -All -NoRestart -FeatureName IIS-WebServerRole,IIS-WebServer,IIS-CommonHttpFeatures,IIS-StaticContent,IIS-DefaultDocument,IIS-HttpErrors,IIS-ApplicationDevelopment,IIS-ISAPIExtensions,IIS-ISAPIFilter,IIS-NetFxExtensibility45,IIS-ASPNET45,IIS-ManagementConsole|Out-Null}
    Step 'Instalando ASP.NET Core Hosting Bundle 8'
    if(-not(Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")){if(-not(Get-Command winget -ErrorAction SilentlyContinue)){throw 'Falta winget/App Installer para descargar el Hosting Bundle.'};Run {winget install --id Microsoft.DotNet.HostingBundle.8 --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity;if($LASTEXITCODE-ne 0){throw "Hosting Bundle fallo: $LASTEXITCODE"}}}
    Step 'Preparando MySQL 8.4'
    $mysql=Get-ChildItem "$env:ProgramFiles\MySQL" -Filter mysql.exe -Recurse -ErrorAction SilentlyContinue|Select-Object -First 1 -ExpandProperty FullName
    if(-not $mysql){if(-not(Get-Command winget -ErrorAction SilentlyContinue)){throw 'Falta winget/App Installer para descargar MySQL.'};Run {winget install --id Oracle.MySQL --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity;if($LASTEXITCODE-ne 0){throw "MySQL fallo: $LASTEXITCODE"}};$installedMySql=$true;$mysql=Get-ChildItem "$env:ProgramFiles\MySQL" -Filter mysql.exe -Recurse|Select-Object -First 1 -ExpandProperty FullName;if(-not$mysql-and-not$WhatIfMode){throw 'MySQL se descargo, pero mysql.exe no fue localizado.'}}
    if($WhatIfMode-and-not$mysql){$mysql='C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe'}
    $service=Get-Service MySQL*,AtlasMySQL -ErrorAction SilentlyContinue|Where-Object{$_.Status-eq'Running'}|Select-Object -First 1
    $rootPassword=$null
    if(-not$service)
    {
        $newRootPassword=if($WhatIfMode){'Simulation-Root-123!'}else{SqlSecret 'Clave administrativa nueva para root de MySQL'}
        $mysqld=Join-Path (Split-Path $mysql) 'mysqld.exe';$mysqlRoot=Join-Path $env:ProgramData 'AtlasPOS\MySQL';$data=Join-Path $mysqlRoot 'data';$ini=Join-Path $mysqlRoot 'my.ini';Run {New-Item -ItemType Directory -Force -Path $data|Out-Null;@("[mysqld]","basedir=$((Split-Path (Split-Path $mysql)) -replace '\\','/')","datadir=$($data-replace '\\','/')","port=3306","bind-address=127.0.0.1","character-set-server=utf8mb4","collation-server=utf8mb4_unicode_ci")|Set-Content $ini -Encoding ascii;& $mysqld --defaults-file=$ini --initialize-insecure;if($LASTEXITCODE-ne 0){throw 'No se pudo inicializar MySQL.'};& $mysqld --install AtlasMySQL --defaults-file=$ini;if($LASTEXITCODE-ne 0){throw 'No se pudo registrar MySQL como servicio.'};Start-Service AtlasMySQL};$service=Get-Service AtlasMySQL -ErrorAction SilentlyContinue
    }
    else{$rootPassword=if($WhatIfMode){'Simulation-Root-123!'}else{Secret 'Clave root de la instalacion MySQL existente'}}
    Step 'Creando base y usuario restringido'
    $escaped=$databasePassword.Replace("'","''");$sql="CREATE DATABASE IF NOT EXISTS atlas_pos CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci; CREATE USER IF NOT EXISTS 'atlas_app'@'localhost' IDENTIFIED BY '$escaped'; ALTER USER 'atlas_app'@'localhost' IDENTIFIED BY '$escaped'; GRANT ALL PRIVILEGES ON atlas_pos.* TO 'atlas_app'@'localhost';";if($newRootPassword){$escapedRoot=$newRootPassword.Replace("'","''");$sql+=" ALTER USER 'root'@'localhost' IDENTIFIED BY '$escapedRoot';"};$sql+=' FLUSH PRIVILEGES;'
    Run {$old=$env:MYSQL_PWD;try{$env:MYSQL_PWD=$rootPassword;& $mysql -uroot --protocol=TCP --host=127.0.0.1 --execute=$sql;if($LASTEXITCODE-ne 0){throw 'No se pudo preparar atlas_pos. Revisa la clave root.'}}finally{$env:MYSQL_PWD=$old}}
    Step 'Publicando Atlas POS en IIS'
    $target=Join-Path $env:ProgramFiles 'Atlas POS\Server';Run {New-Item -ItemType Directory -Force -Path $target|Out-Null;Copy-Item (Join-Path $source '*') $target -Recurse -Force}
    $config=@{ConnectionStrings=@{AtlasMySql="Server=127.0.0.1;Port=3306;Database=atlas_pos;User=atlas_app;Password=$databasePassword;Allow User Variables=true;"};Atlas=@{InitialAdminPassword=$adminPassword;DemoMode=$false;ServerContext=$true;Currency='MXN';TimeZone='America/Mexico_City';RequireHttps=$false}}|ConvertTo-Json -Depth 5
    Run {$config|Set-Content (Join-Path $target 'appsettings.Production.json') -Encoding utf8;New-Item -ItemType Directory -Force -Path (Join-Path $target '.keys')|Out-Null;icacls $target /inheritance:r /grant:r 'Administrators:(OI)(CI)F' 'SYSTEM:(OI)(CI)F' 'IIS AppPool\AtlasPOS:(OI)(CI)M'|Out-Null}
    if(-not$WhatIfMode){Import-Module WebAdministration}
    Run {if(-not(Test-Path IIS:\AppPools\AtlasPOS)){New-WebAppPool AtlasPOS;$script:createdPool=$true};Set-ItemProperty IIS:\AppPools\AtlasPOS -Name managedRuntimeVersion -Value '';Set-ItemProperty IIS:\AppPools\AtlasPOS -Name processModel.identityType -Value ApplicationPoolIdentity;if(Get-Website AtlasPOS -ErrorAction SilentlyContinue){Remove-Website AtlasPOS};New-Website -Name AtlasPOS -PhysicalPath $target -ApplicationPool AtlasPOS -Port 8080;$script:createdSite=$true;New-NetFirewallRule -DisplayName 'Atlas POS Server (red privada)' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 8080 -Profile Private -ErrorAction SilentlyContinue|Out-Null;iisreset /restart|Out-Null}
    Step 'Verificando base, migraciones e IIS'
    if(-not$WhatIfMode){$ok=$false;for($i=0;$i-lt 45;$i++){Start-Sleep 1;try{if((Invoke-WebRequest http://127.0.0.1:8080/health -UseBasicParsing -TimeoutSec 3).StatusCode-eq 200){$ok=$true;break}}catch{}};if(-not$ok){throw 'Atlas no respondio /health despues de instalar.'};$setup=Invoke-WebRequest http://127.0.0.1:8080/Client/Setup -UseBasicParsing -TimeoutSec 5;if($setup.StatusCode-ne200-or-not$setup.Content.Contains('AtlasPOS-Cliente-Instalador.exe')){throw 'La pagina de instalacion de cajas no quedo disponible.'};$client=Invoke-WebRequest http://127.0.0.1:8080/downloads/AtlasPOS-Cliente-Instalador.exe -Method Head -UseBasicParsing -TimeoutSec 5;if($client.StatusCode-ne200-or[int64]$client.Headers['Content-Length']-lt1MB){throw 'El ejecutable para cajas no se publico correctamente.'}}
    $desktop=[Environment]::GetFolderPath('CommonDesktopDirectory');$shortcut=Join-Path $desktop 'Atlas POS.url';Run {@("[InternetShortcut]","URL=http://localhost:8080","IconFile=$env:SystemRoot\System32\SHELL32.dll","IconIndex=220")|Set-Content $shortcut -Encoding ascii}
    Run {@{InstalledAt=(Get-Date).ToString('o');Version='1.0';Url='http://localhost:8080'}|ConvertTo-Json|Set-Content $marker -Encoding utf8}
    Write-Host "`nAtlas POS Server instalado correctamente: http://localhost:8080" -ForegroundColor Green
}
catch{Write-Error $_;if(-not$WhatIfMode){if($createdSite){Remove-Website AtlasPOS -ErrorAction SilentlyContinue};if($createdPool){Remove-WebAppPool AtlasPOS -ErrorAction SilentlyContinue}};throw}
finally{Stop-Transcript|Out-Null}
