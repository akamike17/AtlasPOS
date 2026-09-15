#Requires -RunAsAdministrator
param([string]$ServerIp)
$ErrorActionPreference='Stop'
if([string]::IsNullOrWhiteSpace($ServerIp)){$ServerIp=Read-Host 'Direccion IPv4 del servidor Atlas'}
if($ServerIp-notmatch'^(?:\d{1,3}\.){3}\d{1,3}$'){throw 'La direccion IPv4 del servidor no es valida.'}
$installDirectory=Join-Path $env:ProgramData 'AtlasPOS\PeripheralAgent'
$sourceDirectory=Join-Path $PSScriptRoot 'app'
$serviceName='AtlasPeripheralAgent';$firewallName='Atlas POS Peripheral Agent (servidor)';$created=$false
if(-not(Test-Path $sourceDirectory)){throw 'No se encontro la carpeta app del agente.'}
try
{
    $oldSettings=Join-Path $installDirectory 'atlasagentsettings.json';$token=$null
    if(Test-Path $oldSettings){try{$token=(Get-Content $oldSettings -Raw|ConvertFrom-Json).Agent.Token}catch{}}
    $existing=Get-Service $serviceName -ErrorAction SilentlyContinue
    if($existing){if($existing.Status-ne'Stopped'){Stop-Service $serviceName -Force};sc.exe delete $serviceName|Out-Null;if($LASTEXITCODE-ne 0){throw 'No se pudo reemplazar el servicio Atlas Agent.'};Start-Sleep -Seconds 1}
    New-Item -ItemType Directory -Force -Path $installDirectory|Out-Null
    Copy-Item (Join-Path $sourceDirectory '*') $installDirectory -Recurse -Force
    if([string]::IsNullOrWhiteSpace($token)-or$token.StartsWith('CONFIGURAR_')){$token=-join((48..57)+(65..90)+(97..122)|Get-Random -Count 48|ForEach-Object{[char]$_})}
    $settings=@{Agent=@{Urls='http://0.0.0.0:17420';Token=$token};Logging=@{LogLevel=@{Default='Information';'Microsoft.AspNetCore'='Warning'}}}|ConvertTo-Json -Depth 5
    $settings|Set-Content $oldSettings -Encoding utf8
    $exe=Join-Path $installDirectory 'Atlas.PeripheralAgent.exe'
    sc.exe create $serviceName binPath= "`"$exe`" --contentRoot `"$installDirectory`"" start= auto DisplayName= "Atlas POS Peripheral Agent"|Out-Null
    if($LASTEXITCODE-ne 0){throw 'Windows no pudo crear el servicio Atlas Agent.'};$created=$true
    sc.exe description $serviceName "Conecta la bascula, impresora y cajon de esta caja con Atlas POS."|Out-Null
    sc.exe failure $serviceName reset= 86400 actions= restart/5000/restart/15000/restart/30000|Out-Null
    Get-NetFirewallRule -DisplayName $firewallName -ErrorAction SilentlyContinue|Remove-NetFirewallRule
    New-NetFirewallRule -DisplayName $firewallName -Direction Inbound -Action Allow -Protocol TCP -LocalPort 17420 -Profile Private -Program $exe -RemoteAddress $ServerIp|Out-Null
    Start-Service $serviceName
    $healthy=$false;for($attempt=0;$attempt-lt 20;$attempt++){Start-Sleep -Milliseconds 500;try{if((Invoke-WebRequest http://127.0.0.1:17420/health -UseBasicParsing -TimeoutSec 2).StatusCode-eq 200){$healthy=$true;break}}catch{}}
    if(-not$healthy){throw 'El servicio se instalo, pero no respondio la prueba de salud.'}
    $ip=(Get-NetIPAddress -AddressFamily IPv4|Where-Object{$_.IPAddress-notlike'169.254*'-and$_.IPAddress-ne'127.0.0.1'-and$_.InterfaceOperationalStatus-eq'Up'}|Select-Object -First 1 -ExpandProperty IPAddress)
    @("URL DEL AGENTE: http://$ip`:17420","TOKEN: $token",'',"Servidor autorizado en firewall: $ServerIp")|Set-Content (Join-Path $PSScriptRoot 'DATOS-DE-ESTA-CAJA.txt') -Encoding utf8
    Write-Host "Atlas Agent instalado, probado y activo. URL http://$ip`:17420" -ForegroundColor Green
}
catch
{
    if($created){Stop-Service $serviceName -Force -ErrorAction SilentlyContinue;sc.exe delete $serviceName|Out-Null}
    Get-NetFirewallRule -DisplayName $firewallName -ErrorAction SilentlyContinue|Remove-NetFirewallRule
    throw
}
