<#
    EURECABANK - Carga la base de datos en SQL Server (MSSQLSERVER, autenticacion de Windows).
    Ejecutar desde la carpeta 03. BDD:
        .\SQLSERVER\cargar_bdd_sqlserver.ps1                                  (crea eurekabank)
        .\SQLSERVER\cargar_bdd_sqlserver.ps1 -Esquema eurekabank_prueba -Recrear
    -Recrear borra la base si ya existe. Sin el, el script se detiene para no perder datos.
#>
param(
    [string]$Esquema = 'eurekabank',
    [string]$Servidor = 'localhost',
    [switch]$Recrear
)

$ErrorActionPreference = 'Stop'
$carpeta = $PSScriptRoot
$sqlcmd = Get-ChildItem "${env:ProgramFiles}\Microsoft SQL Server" -Recurse -Filter sqlcmd.exe -ErrorAction SilentlyContinue |
    Sort-Object FullName -Descending | Select-Object -First 1 -ExpandProperty FullName
if (-not $sqlcmd) { throw 'No se encontro sqlcmd.exe. Instala las herramientas de SQL Server.' }

if ($Esquema -notmatch '^[A-Za-z0-9_]+$') { throw 'El nombre de la base solo admite letras, numeros y guion bajo.' }

$existe = & $sqlcmd -S $Servidor -E -C -b -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$Esquema') IS NULL THEN 0 ELSE 1 END;" 2>&1
if ($LASTEXITCODE -ne 0) { throw "No se conecto a $Servidor. Revisa que el servicio MSSQLSERVER este iniciado.`n$existe" }
if ("$existe".Trim() -eq '1') {
    if (-not $Recrear) { throw "La base '$Esquema' ya existe. Usa -Recrear para borrarla y cargarla de nuevo." }
    Write-Host "Borrando la base '$Esquema' ..." -ForegroundColor Yellow
    & $sqlcmd -S $Servidor -E -C -b -Q "IF DB_ID(N'$Esquema') IS NOT NULL BEGIN ALTER DATABASE [$Esquema] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Esquema]; END"
    if ($LASTEXITCODE -ne 0) { throw 'No se pudo borrar la base.' }
}

$archivos = '00_esquema.sql', '01_estructura.sql', '02_datos.sql', '04_carga_prueba.sql'
foreach ($a in $archivos) {
    Write-Host "Ejecutando $a en '$Esquema' ..."
    & $sqlcmd -S $Servidor -E -C -b -v esquema=$Esquema -i (Join-Path $carpeta $a)
    if ($LASTEXITCODE -ne 0) { throw "Fallo $a" }
}

Write-Host 'Verificacion:' -ForegroundColor Cyan
& $sqlcmd -S $Servidor -E -C -b -d $Esquema -Q "SET NOCOUNT ON; SELECT 'tablas' AS concepto, COUNT(*) AS valor FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' UNION ALL SELECT 'cuentas', COUNT(*) FROM cuenta UNION ALL SELECT 'movimientos', COUNT(*) FROM movimiento UNION ALL SELECT 'empleados', COUNT(*) FROM empleado UNION ALL SELECT 'clientes', COUNT(*) FROM cliente;"
Write-Host "RESULTADO: base '$Esquema' cargada en $Servidor (17 tablas, 6 cuentas y 74 movimientos esperados)." -ForegroundColor Green
