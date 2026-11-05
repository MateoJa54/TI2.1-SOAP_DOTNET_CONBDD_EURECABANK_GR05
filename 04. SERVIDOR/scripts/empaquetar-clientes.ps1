# Arma un .zip con los clientes de consola, escritorio y web ya compilados, para pasarlos a otras computadoras.
# Cada cliente trae un iniciar.cmd que pide la direccion del servidor (la que imprime levantar-servidor-lan.ps1).
#   powershell -ExecutionPolicy Bypass -File empaquetar-clientes.ps1 [-Salida C:\ruta\clientes.zip]
param([string]$Salida)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $Salida) { $Salida = Join-Path (Split-Path -Parent $raiz) 'clientes-eurekabank-dotnet.zip' }

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'No se encontro MSBuild.' }
foreach ($p in '04. CLICON\CLICON\CLICON.csproj', '04. CLIESC\CLIESC\CLIESC.csproj', '04. CLIWEB\CLIWEB\CLIWEB.csproj') {
    & $msbuild (Join-Path $raiz $p) -restore '-p:Configuration=Release' -nologo -verbosity:minimal
    if ($LASTEXITCODE -ne 0) { throw "Fallo la compilacion de $p" }
}

$tmp = Join-Path ([IO.Path]::GetTempPath()) ('clientes-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tmp | Out-Null
$ansi = [Text.Encoding]::GetEncoding(850)

function Escribir($ruta, $texto) { [IO.File]::WriteAllText($ruta, $texto, $ansi) }

# --- consola y escritorio: la direccion se pasa como argumento
foreach ($c in @(@('CLICON', 'consola'), @('CLIESC', 'escritorio'))) {
    $origen = Join-Path $raiz "04. $($c[0])\$($c[0])\bin\Release\net48"
    if (-not (Test-Path $origen)) { $origen = Join-Path $raiz "04. $($c[0])\$($c[0])\bin\Debug\net48" }
    $destino = Join-Path $tmp "cliente-$($c[1])"
    New-Item -ItemType Directory -Path $destino | Out-Null
    Get-ChildItem $origen -File | Where-Object { $_.Extension -notin '.pdb', '.xml' } | Copy-Item -Destination $destino
    $arranque = if ($c[0] -eq 'CLICON') { 'CLICON.exe "%URL%"' } else { 'start "" CLIESC.exe "%URL%"' }
    Escribir (Join-Path $destino 'iniciar.cmd') ("@echo off`r`ncd /d `"%~dp0`"`r`nset /p URL=Direccion del servidor (ej. http://10.0.0.5:50775/WSEUREKABANK.svc): `r`n$arranque`r`n")
}

# --- web: se reescribe ws.url y se levanta con IIS Express
$sitio = Join-Path $tmp 'cliente-web\sitio'
New-Item -ItemType Directory -Path $sitio -Force | Out-Null
$web = Join-Path $raiz '04. CLIWEB\CLIWEB'
foreach ($e in 'bin', 'Views', 'Content') { Copy-Item (Join-Path $web $e) (Join-Path $sitio $e) -Recurse }
Copy-Item (Join-Path $web 'Web.config'), (Join-Path $web 'Global.asax') $sitio
$ps = @'
param([string]$Url)
$cfg = Join-Path $PSScriptRoot 'sitio\Web.config'
$t = Get-Content $cfg -Raw
$t = [regex]::Replace($t, '(<add key="ws.url" value=")[^"]*(")', ('${1}' + $Url + '${2}'))
Set-Content $cfg $t -Encoding UTF8
$iis = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | ForEach-Object { Join-Path $_ 'IIS Express\iisexpress.exe' } | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iis) { Write-Host 'Instala IIS Express (gratis, de Microsoft) y vuelve a ejecutar.'; exit 1 }
Start-Process 'http://localhost:50776/'
& $iis "/path:$(Join-Path $PSScriptRoot 'sitio')" /port:50776
'@
Escribir (Join-Path $tmp 'cliente-web\levantar.ps1') $ps
Escribir (Join-Path $tmp 'cliente-web\iniciar.cmd') "@echo off`r`nset /p URL=Direccion del servidor (ej. http://10.0.0.5:50775/WSEUREKABANK.svc): `r`npowershell -NoProfile -ExecutionPolicy Bypass -File `"%~dp0levantar.ps1`" -Url `"%URL%`"`r`n"

Escribir (Join-Path $tmp 'LEEME.txt') @"
EURECABANK, clientes SOAP en .NET
1. Pide a quien tiene el servidor la direccion que imprime levantar-servidor-lan.ps1, por ejemplo
   http://10.40.59.73:50775/WSEUREKABANK.svc  (misma red local).
2. Elige un cliente y ejecuta su iniciar.cmd; te pedira esa direccion.
   cliente-consola     Menu en la ventana de comandos.
   cliente-escritorio  Ventana WinForms.
   cliente-web         Abre http://localhost:50776/ (necesita IIS Express instalado).
Requisitos: Windows con .NET Framework 4.8 (ya viene en Windows 10 y 11).
Usuario de laboratorio: cromero / Cromero2026
"@

if (Test-Path $Salida) { Remove-Item $Salida -Force }
Compress-Archive -Path (Join-Path $tmp '*') -DestinationPath $Salida
Remove-Item $tmp -Recurse -Force
Write-Host "Paquete creado: $Salida ($([math]::Round((Get-Item $Salida).Length / 1MB, 1)) MB)"
