$ErrorActionPreference='Stop'
$projectDirectory=Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$packageDirectory=Join-Path $projectDirectory 'wwwroot\downloads\AtlasPeripheralAgent'
$appDirectory=Join-Path $packageDirectory 'app'
if(Test-Path -LiteralPath $packageDirectory){$resolved=(Resolve-Path -LiteralPath $packageDirectory).Path;if(-not $resolved.StartsWith((Join-Path $projectDirectory 'wwwroot\downloads'),[StringComparison]::OrdinalIgnoreCase)){throw 'Ruta de paquete fuera del directorio permitido.'};Remove-Item -LiteralPath $packageDirectory -Recurse -Force}
New-Item -ItemType Directory -Force -Path $appDirectory|Out-Null
dotnet publish (Join-Path $projectDirectory 'Atlas.PeripheralAgent\Atlas.PeripheralAgent.csproj') -c Release -r win-x64 --self-contained true -o $appDirectory
if($LASTEXITCODE-ne 0){throw "Falló la publicación del agente: $LASTEXITCODE"}
Copy-Item (Join-Path $projectDirectory 'Atlas.PeripheralAgent\Install-AtlasAgent.ps1') $packageDirectory -Force
$zip=Join-Path $projectDirectory 'wwwroot\downloads\AtlasPeripheralAgent.zip'
Compress-Archive -Path (Join-Path $packageDirectory '*') -DestinationPath $zip -Force
Write-Host "Cliente creado: $zip"
