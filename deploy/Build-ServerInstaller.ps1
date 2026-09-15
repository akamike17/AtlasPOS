$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path);$stage=Join-Path $root 'release\AtlasServerInstaller';$app=Join-Path $stage 'app'
if(Test-Path $stage){Remove-Item $stage -Recurse -Force};New-Item -ItemType Directory -Force $app|Out-Null
dotnet publish (Join-Path $root 'PuntoDeVentaAtlas.Web.csproj') -c Release -o $app --no-self-contained
if($LASTEXITCODE-ne 0){throw 'Falló la publicación del servidor.'}
Copy-Item (Join-Path $root 'installer\Install-AtlasServer.ps1') $stage -Force
$payload=Join-Path $root 'release\AtlasServerPayload.zip';Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $payload -Force
$compiler="$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe";$output=Join-Path $root 'release\AtlasPOS-Servidor-Instalador.exe'
& $compiler /nologo /target:winexe /platform:x64 /out:$output /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:System.Windows.Forms.dll /resource:$payload,payload.zip (Join-Path $root 'installer\InstallerBootstrap.cs')
if($LASTEXITCODE-ne 0){throw 'No se pudo construir el ejecutable del servidor.'}
Write-Host "Instalador listo: $output"
