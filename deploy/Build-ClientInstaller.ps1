$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path);& (Join-Path $root 'deploy\Build-AgentPackage.ps1');if($LASTEXITCODE-ne 0){throw 'Falló el paquete del agente.'}
$payload=Join-Path $root 'wwwroot\downloads\AtlasPeripheralAgent.zip';$source=Join-Path $root 'installer\InstallerBootstrap.cs';$clientSource=Join-Path $root 'release\ClientInstallerBootstrap.cs';New-Item -ItemType Directory -Force (Split-Path $clientSource)|Out-Null
$content=(Get-Content $source -Raw).Replace('string script=Environment.GetEnvironmentVariable("ATLAS_INSTALL_SCRIPT")??"Install-AtlasServer.ps1";','string script="Install-AtlasAgent.ps1";');$content|Set-Content $clientSource -Encoding utf8
$compiler="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe";$output=Join-Path $root 'wwwroot\downloads\AtlasPOS-Cliente-Instalador.exe'
& $compiler /nologo /target:winexe /platform:x64 /out:$output /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Windows.Forms.dll /resource:$payload,payload.zip $clientSource
if($LASTEXITCODE-ne 0){throw 'No se pudo construir el ejecutable del cliente.'};Write-Host "Instalador listo: $output"
