<#
    EURECABANK - Crea el esquema de pruebas `eurekabank_prueba`.

    Las pruebas de integracion del servidor escriben saldos y filas de logsession.
    Para no tocar los datos de `eurekabank`, se apuntan a este esquema, que es una
    copia de los cinco scripts de 03. BDD\docker\init con el nombre del esquema cambiado.

    Ejecutar desde la raiz del proyecto (pide la clave de root de forma segura):
        .\03. BDD\crear_bdd_prueba.ps1

    Opciones:
        -Usuario root -Puerto 3306 -Mysql "C:\ruta\mysql.exe"
        -Esquema eurekabank_prueba

    Es idempotente. Para volver a la semilla despues de correr las pruebas basta
    con ejecutarlo otra vez.
#>

param(
    [string]$Usuario = "root",
    [string]$Clave,
    [string]$Host_MySql = "127.0.0.1",
    [int]$Puerto = 3306,
    [string]$Esquema = "eurekabank_prueba",
    [string]$Mysql = "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe"
)

$ErrorActionPreference = "Stop"

if ($Esquema -notmatch '^[A-Za-z0-9_]+$') {
    Write-Host "ERROR: el nombre del esquema solo admite letras, numeros y guion bajo." -ForegroundColor Red
    exit 1
}
if ($Esquema -eq "eurekabank") {
    Write-Host "ERROR: este script es para el esquema de pruebas, no para 'eurekabank'." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path -LiteralPath $Mysql)) {
    Write-Host "ERROR: no se encuentra el cliente MySQL en $Mysql (usa -Mysql)." -ForegroundColor Red
    exit 1
}

$raiz = Split-Path -Parent $PSScriptRoot
$init = Join-Path $raiz "03. BDD\docker\init"
if (-not (Test-Path -LiteralPath $init)) {
    Write-Host "ERROR: no se encuentra $init" -ForegroundColor Red
    exit 1
}

if ($PSBoundParameters.ContainsKey("Clave")) {
    $claveUsada = $Clave
}
else {
    $segura = Read-Host "Clave de MySQL para el usuario '$Usuario'" -AsSecureString
    $apuntador = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($segura)
    $claveUsada = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($apuntador)
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($apuntador)
}

$temporal = Join-Path $env:TEMP ("eurekabank-prueba-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
New-Item -ItemType Directory -Path $temporal | Out-Null
$cfg = Join-Path $temporal "cliente.ini"
$claveEscapada = $claveUsada.Replace('\', '\\').Replace('"', '\"')
@"
[client]
user=$Usuario
password="$claveEscapada"
host=$Host_MySql
port=$Puerto
default-character-set=utf8mb4
protocol=TCP
"@ | Set-Content -LiteralPath $cfg -Encoding ASCII

try {
    # `eurekabank` pasa a `$Esquema`, salvo en el usuario de aplicacion `eurekabank_app`.
    $lineasDriver = @()
    foreach ($nombre in "00_esquema", "01_estructura", "02_datos", "03_usuarios_app", "04_carga_prueba") {
        $texto = Get-Content -LiteralPath (Join-Path $init "$nombre.sql") -Raw -Encoding UTF8
        $texto = [regex]::Replace($texto, '(?<![\w])eurekabank(?![\w])', $Esquema)
        $destino = Join-Path $temporal "$nombre.sql"
        [IO.File]::WriteAllText($destino, $texto, (New-Object Text.UTF8Encoding($false)))
        $lineasDriver += "source $destino"
    }
    $driver = Join-Path $temporal "cargar.sql"
    $lineasDriver | Set-Content -LiteralPath $driver -Encoding UTF8

    $argumentos = @("--defaults-extra-file=$cfg", "--default-character-set=utf8mb4")
    Write-Host "Creando el esquema '$Esquema' en ${Host_MySql}:$Puerto ..." -ForegroundColor Cyan
    $salida = & $Mysql @argumentos -e "source $driver" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: la carga fallo:" -ForegroundColor Red
        $salida | ForEach-Object { Write-Host ("  " + $_) -ForegroundColor DarkYellow }
        exit 1
    }

    $consulta = "SELECT 'tablas' AS concepto, COUNT(*) AS valor FROM information_schema.tables WHERE table_schema='$Esquema' UNION ALL SELECT 'cuentas', COUNT(*) FROM $Esquema.cuenta UNION ALL SELECT 'movimientos', COUNT(*) FROM $Esquema.movimiento;"
    $verificacion = & $Mysql @argumentos -e $consulta 2>&1
    $verificacion | ForEach-Object { Write-Host ("  " + $_) }

    # La semilla (6 cuentas, 74 movimientos) la crea el procedimiento, que solo corre con root.
    & $Mysql @argumentos -e "CALL $Esquema.sp_cargar_datos_prueba();" 2>&1 | Out-Null
    $conteo = & $Mysql @argumentos -N -e "SELECT (SELECT COUNT(*) FROM $Esquema.cuenta), (SELECT COUNT(*) FROM $Esquema.movimiento);" 2>&1
    Write-Host ("Cuentas y movimientos tras la semilla: {0}" -f ($conteo -join " "))
    Write-Host "Listo. Las pruebas de integracion usan el esquema '$Esquema'." -ForegroundColor Green
}
finally {
    Remove-Item -LiteralPath $temporal -Recurse -Force -ErrorAction SilentlyContinue
}
