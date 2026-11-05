# Regenera el proxy WCF (CLIWEB\Connected Services\Referencia\Reference.cs) desde el contrato local.
# Equivale a "Agregar referencia de servicio", pero reproducible desde consola.
# Uso: powershell -ExecutionPolicy Bypass -File regenerar-proxy.ps1 [-Wsdl <ruta del WSDL>]
# El archivo generado no se edita a mano: si cambia el contrato, se vuelve a generar.
param([string]$Wsdl)

$ErrorActionPreference = 'Stop'
$raizCliente = Split-Path -Parent $PSScriptRoot
$raizRepositorio = Split-Path -Parent $raizCliente
if (-not $Wsdl) { $Wsdl = Join-Path $raizRepositorio '04. SERVIDOR\contrato\WSEUREKABANK.wsdl' }
$xsd = Join-Path (Split-Path -Parent $Wsdl) 'WSEUREKABANK_schema.xsd'
$salida = Join-Path $raizCliente 'CLIWEB\Connected Services\Referencia\Reference.cs'

$svcutil = Get-ChildItem (Join-Path ${env:ProgramFiles(x86)} 'Microsoft SDKs\Windows') -Recurse -Filter SvcUtil.exe -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match 'NETFX 4\.[0-9.]+ Tools\\SvcUtil\.exe$' } |
    Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $svcutil) { Write-Host 'No se encontro SvcUtil.exe (Windows SDK, NETFX 4.8 Tools).'; exit 2 }
if (-not (Test-Path $Wsdl)) { Write-Host "No se encontro el WSDL: $Wsdl"; exit 2 }

$temporal = Join-Path ([IO.Path]::GetTempPath()) ('proxy-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporal | Out-Null
try {
    & $svcutil $Wsdl $xsd /language:cs '/namespace:*,ec.edu.monster.ws.Referencia' /serializer:XmlSerializer /syncOnly /noConfig "/out:$temporal\Reference.cs" | Out-Null
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path "$temporal\Reference.cs")) { Write-Host 'SvcUtil fallo.'; exit 1 }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $salida) | Out-Null
    Copy-Item "$temporal\Reference.cs" $salida -Force
}
finally { Remove-Item $temporal -Recurse -Force -ErrorAction SilentlyContinue }
Write-Host "RESULTADO: proxy generado en $salida"
