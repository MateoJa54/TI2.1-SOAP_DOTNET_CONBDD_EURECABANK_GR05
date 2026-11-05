# Compila el servidor y sus pruebas desde cero y ejecuta las pruebas MSTest.
# Uso: powershell -ExecutionPolicy Bypass -File compilar-y-probar.ps1 [-Integracion]
#   Sin opciones corre solo las pruebas sin base de datos (TestCategory != Integracion).
#   -Integracion corre las pruebas que usan el esquema eurekabank_prueba (ver 03. BDD\crear_bdd_prueba.ps1).
param([switch]$Integracion)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'No se encontro MSBuild.' }

& $msbuild (Join-Path $raiz 'WSEUREKABANK.Pruebas\WSEUREKABANK.Pruebas.csproj') -restore -t:Rebuild '-p:Configuration=Debug' -nologo -verbosity:minimal
if ($LASTEXITCODE -ne 0) { Write-Host 'La compilacion fallo.'; exit 1 }

$filtro = if ($Integracion) { 'TestCategory=Integracion' } else { 'TestCategory!=Integracion' }
dotnet test (Join-Path $raiz 'WSEUREKABANK.Pruebas\WSEUREKABANK.Pruebas.csproj') --no-build --filter $filtro
exit $LASTEXITCODE
