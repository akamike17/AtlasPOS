$deployDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
Get-Process -Name 'PuntoDeVentaAtlas.Web' -ErrorAction SilentlyContinue | Stop-Process
Push-Location $deployDirectory
try { docker compose stop }
finally { Pop-Location }
