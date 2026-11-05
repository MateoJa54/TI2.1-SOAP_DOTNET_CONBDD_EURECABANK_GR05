# Levanta WSEUREKABANK en IIS Express abierto a la red local para que otras computadoras usen los clientes.
# Ejecutar en PowerShell COMO ADMINISTRADOR (hace falta para reservar la URL y abrir el puerto del firewall).
#   powershell -ExecutionPolicy Bypass -File levantar-servidor-lan.ps1 [-Puerto 50775] [-Esquema eurekabank]
# Con -SoloLocal no se abre a la red y no exige administrador (sirve para probar).
# El servidor queda en ejecución hasta que presiones Enter; al salir apaga IIS Express y, con -QuitarReglas, borra la regla del firewall.
param(
    [ValidateRange(1, 65535)][int]$Puerto = 50775,
    [string]$Esquema = 'eurekabank',
    [switch]$SoloLocal,
    [switch]$QuitarReglas,
    [switch]$SinEsperar
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$proyecto = Join-Path $raiz 'WSEUREKABANK'
$dll = Join-Path $proyecto 'bin\WSEUREKABANK.dll'

$identidad = [Security.Principal.WindowsIdentity]::GetCurrent()
$esAdmin = (New-Object Security.Principal.WindowsPrincipal($identidad)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $SoloLocal -and -not $esAdmin) {
    throw 'Abre PowerShell como administrador (o usa -SoloLocal para probar solo en esta computadora).'
}

$iisExpress = @($env:ProgramFiles, ${env:ProgramFiles(x86)}) | ForEach-Object { Join-Path $_ 'IIS Express\iisexpress.exe' } | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $iisExpress) { throw 'No se encontro IIS Express.' }
$plantilla = Join-Path (Split-Path -Parent $iisExpress) 'config\templates\PersonalWebServer\applicationhost.config'

# Compila si hace falta
if (-not (Test-Path $dll)) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) { throw 'No se encontro MSBuild para compilar el servidor.' }
    Write-Host '== Compilando el servidor =='
    & $msbuild (Join-Path $proyecto 'WSEUREKABANK.csproj') -restore '-p:Configuration=Debug' -nologo -verbosity:minimal
    if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }
}

# applicationhost.config propio, con el sitio apuntando a este proyecto
$temporal = Join-Path ([IO.Path]::GetTempPath()) ('wseurekabank-lan-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporal | Out-Null
$config = Join-Path $temporal 'applicationhost.config'
$xml = [xml](Get-Content $plantilla -Raw)
$sitio = $xml.configuration.'system.applicationHost'.sites.site
$sitio.SetAttribute('name', 'WSEUREKABANK')
$sitio.application.virtualDirectory.SetAttribute('physicalPath', $proyecto)
$enlace = if ($SoloLocal) { "*:${Puerto}:localhost" } else { "*:${Puerto}:*" }
$sitio.bindings.binding.SetAttribute('bindingInformation', $enlace)
$xml.Save($config)

$regla = "EURECABANK WSEUREKABANK TCP $Puerto LAN"
$proceso = $null
try {
    if (-not $SoloLocal) {
        $acl = (& netsh http show urlacl "url=http://*:$Puerto/" 2>&1 | Out-String)
        if ($acl -notmatch [regex]::Escape("http://*:$Puerto/")) {
            & netsh http add urlacl "url=http://*:$Puerto/" "user=$($identidad.Name)" | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "No se pudo reservar http://*:$Puerto/ para $($identidad.Name)." }
        }
        if (-not (Get-NetFirewallRule -DisplayName $regla -ErrorAction SilentlyContinue)) {
            New-NetFirewallRule -DisplayName $regla -Direction Inbound -Protocol TCP -LocalPort $Puerto -Action Allow -Profile Any -RemoteAddress LocalSubnet | Out-Null
        }
    }

    $env:EUREKABANK_DB_ESQUEMA = $Esquema
    $proceso = Start-Process $iisExpress -ArgumentList "/config:`"$config`"", '/site:WSEUREKABANK' -PassThru -WindowStyle Hidden
    $url = "http://localhost:$Puerto/WSEUREKABANK.svc"
    $arriba = $false
    for ($i = 0; $i -lt 120 -and -not $arriba; $i++) {
        if ($proceso.HasExited) { break }
        try { $arriba = (New-Object Net.WebClient).DownloadString("${url}?wsdl").Contains('WSEUREKABANK') } catch { Start-Sleep -Milliseconds 500 }
    }
    if (-not $arriba) { throw 'El servicio no arranco. Revisa que MySQL y la compilacion esten bien.' }

    Write-Host ''
    Write-Host "Servidor en marcha (esquema '$Esquema')." -ForegroundColor Green
    Write-Host "  En esta computadora: $url"
    if (-not $SoloLocal) {
        Write-Host '  Para los companeros, la misma red local, usar una de estas direcciones:'
        Get-NetIPAddress -AddressFamily IPv4 |
            Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' -and $_.AddressState -eq 'Preferred' } |
            ForEach-Object { Write-Host ("    http://{0}:{1}/WSEUREKABANK.svc   ({2})" -f $_.IPAddress, $Puerto, $_.InterfaceAlias) }
        Write-Host '  Consola: CLICON.exe <url>      Escritorio y web: variable WS_URL o ws.url en su config.'
    }
    if ($SinEsperar) { return }
    Write-Host ''
    Read-Host 'Presiona Enter para detener el servidor'
}
finally {
    if ($proceso -and -not $proceso.HasExited) { Stop-Process -Id $proceso.Id -Force }
    Remove-Item $temporal -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item Env:\EUREKABANK_DB_ESQUEMA -ErrorAction SilentlyContinue
    if ($QuitarReglas -and -not $SoloLocal) {
        Remove-NetFirewallRule -DisplayName $regla -ErrorAction SilentlyContinue
        & netsh http delete urlacl "url=http://*:$Puerto/" | Out-Null
    }
}
