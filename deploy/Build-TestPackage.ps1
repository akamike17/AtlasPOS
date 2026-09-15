$ErrorActionPreference = 'Stop'
$projectDirectory = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$packageDirectory = Join-Path $projectDirectory 'release\AtlasPOS-TestPC'
$appDirectory = Join-Path $packageDirectory 'app'
New-Item -ItemType Directory -Force -Path $appDirectory | Out-Null
dotnet publish (Join-Path $projectDirectory 'PuntoDeVentaAtlas.Web.csproj') -c Release -r win-x64 --self-contained true -o $appDirectory
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falló con código $LASTEXITCODE." }
Copy-Item (Join-Path $projectDirectory 'deploy\docker-compose.yml') $packageDirectory -Force
Copy-Item (Join-Path $projectDirectory 'deploy\Start-Atlas.ps1') $packageDirectory -Force
Copy-Item (Join-Path $projectDirectory 'deploy\Stop-Atlas.ps1') $packageDirectory -Force
Copy-Item (Join-Path $projectDirectory 'deploy\README-TEST-PC.md') $packageDirectory -Force
$zip = Join-Path $projectDirectory 'release\AtlasPOS-TestPC.zip'
Compress-Archive -Path (Join-Path $packageDirectory '*') -DestinationPath $zip -Force
Write-Host "Paquete creado: $zip"
