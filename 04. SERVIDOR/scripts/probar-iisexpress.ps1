# Levanta WSEUREKABANK en IIS Express (puerto libre, sin administrador), recorre las 9 operaciones SOAP
# por HTTP crudo contra el esquema de pruebas y apaga el servidor. Imprime PASS/FAIL y sale con codigo != 0 si algo falla.
# Uso: powershell -ExecutionPolicy Bypass -File probar-iisexpress.ps1 [-Esquema eurekabank_prueba] [-NoCompilar] [-Puerto 0]
param(
    [string]$Esquema = 'eurekabank_prueba',
    [switch]$NoCompilar,
    [int]$Puerto = 0
)

$ErrorActionPreference = 'Stop'
$raiz = Split-Path -Parent $PSScriptRoot
$proyecto = Join-Path $raiz 'WSEUREKABANK'
$csproj = Join-Path $proyecto 'WSEUREKABANK.csproj'
$ws = 'http://ws.monster.edu.ec/'
$resultados = New-Object System.Collections.Generic.List[object]

function Buscar-MSBuild {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $ruta = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($ruta) { return $ruta }
    }
    throw 'No se encontro MSBuild (Visual Studio con carga de trabajo web).'
}

function Buscar-IisExpress {
    foreach ($base in $env:ProgramFiles, ${env:ProgramFiles(x86)}) {
        $ruta = Join-Path $base 'IIS Express\iisexpress.exe'
        if (Test-Path $ruta) { return $ruta }
    }
    throw 'No se encontro IIS Express.'
}

function Puerto-Libre {
    $escucha = New-Object System.Net.Sockets.TcpListener([System.Net.IPAddress]::Loopback, 0)
    $escucha.Start(); $p = $escucha.LocalEndpoint.Port; $escucha.Stop(); return $p
}

function Registrar([string]$caso, [string]$esperado, [string]$obtenido) {
    $estado = if ($esperado -eq $obtenido) { 'PASS' } else { 'FAIL' }
    $resultados.Add([pscustomobject]@{ Estado = $estado; Caso = $caso; Esperado = $esperado; Obtenido = $obtenido })
}

function Enviar([string]$url, [string]$cuerpo) {
    $peticion = [System.Net.HttpWebRequest]::Create($url)
    $peticion.Method = 'POST'; $peticion.Timeout = 30000
    $peticion.ContentType = 'text/xml; charset=utf-8'
    $peticion.Headers.Add('SOAPAction', '""')
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($cuerpo)
    $flujo = $peticion.GetRequestStream(); $flujo.Write($bytes, 0, $bytes.Length); $flujo.Close()
    try { $respuesta = $peticion.GetResponse() }
    catch [System.Net.WebException] {
        if ($null -eq $_.Exception.Response) { return [pscustomobject]@{ Http = 0; Cuerpo = $_.Exception.Message } }
        $respuesta = $_.Exception.Response
    }
    $lector = New-Object System.IO.StreamReader($respuesta.GetResponseStream(), [System.Text.Encoding]::UTF8)
    $texto = $lector.ReadToEnd(); $lector.Close(); $http = [int]$respuesta.StatusCode; $respuesta.Close()
    return [pscustomobject]@{ Http = $http; Cuerpo = $texto }
}

function Llamar([string]$url, [string]$operacion, [string]$hijos) {
    $sobre = "<soapenv:Envelope xmlns:soapenv=`"http://schemas.xmlsoap.org/soap/envelope/`"><soapenv:Body><ws:$operacion xmlns:ws=`"$ws`">$hijos</ws:$operacion></soapenv:Body></soapenv:Envelope>"
    $r = Enviar $url $sobre
    $campos = @{ _http = [string]$r.Http; _fault = [string]($r.Cuerpo -match 'Fault') }
    try {
        $xml = [xml]$r.Cuerpo
        $nodo = $xml.SelectSingleNode("//*[local-name()='return']")
        if ($nodo) { foreach ($h in $nodo.ChildNodes) { if (-not $campos.ContainsKey($h.LocalName)) { $campos[$h.LocalName] = $h.InnerText } } }
    } catch { }
    return $campos
}

function Verificar([string]$caso, [string]$esperado, $campos, [string]$campo = 'codigo') {
    $obtenido = if ($campos.ContainsKey($campo)) { $campos[$campo] } else { "(sin $campo, http $($campos['_http']))" }
    Registrar $caso $esperado $obtenido
}

$iis = $null
try {
    $msbuild = Buscar-MSBuild
    $iisExpress = Buscar-IisExpress
    $dll = Join-Path $proyecto 'bin\WSEUREKABANK.dll'
    if (-not $NoCompilar) {
        Write-Host '== Compilando =='
        & $msbuild $csproj -restore '-p:Configuration=Debug' -nologo -verbosity:minimal
        if ($LASTEXITCODE -ne 0) { throw 'La compilacion fallo.' }
    }
    if (-not (Test-Path $dll)) { throw "No existe $dll" }

    if ($Puerto -eq 0) { $Puerto = Puerto-Libre }
    $env:EUREKABANK_DB_ESQUEMA = $Esquema
    Write-Host "== IIS Express en el puerto $Puerto, esquema $Esquema =="
    $iis = Start-Process -FilePath $iisExpress -ArgumentList "/path:`"$proyecto`"", "/port:$Puerto" -PassThru -WindowStyle Hidden
    $url = "http://localhost:$Puerto/WSEUREKABANK.svc"
    $arriba = $false
    for ($i = 0; $i -lt 120 -and -not $arriba; $i++) {
        if ($iis.HasExited) { break }
        try { $w = (New-Object System.Net.WebClient).DownloadString("${url}?wsdl"); $arriba = $w.Contains('WSEUREKABANK') } catch { Start-Sleep -Milliseconds 500 }
    }
    Registrar 'IIS Express publica el WSDL' 'True' ([string]$arriba)
    if (-not $arriba) { throw 'El servicio no arranco.' }

    $a = Llamar $url 'autenticar' '<usuario>cromero</usuario><clave>Cromero2026</clave>'
    Verificar 'autenticar correcto' '200' $a
    $t = $a['token']
    Registrar 'autenticar entrega token y expiracion ISO-8601' 'True' ([string](($t -match '^[0-9a-f-]{36}$') -and ($a['expiracion'] -match '^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$')))
    Verificar 'autenticar con clave mala' 'AUTH-001' (Llamar $url 'autenticar' '<usuario>cromero</usuario><clave>mala</clave>')
    Verificar 'autenticar sin datos' 'DAT-001' (Llamar $url 'autenticar' '<usuario></usuario><clave></clave>')
    Verificar 'autenticar con hash SHA-256' '200' (Llamar $url 'autenticar' '<usuario>cromero</usuario><clave>891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4</clave>')

    $c = Llamar $url 'listarCuentasPorCliente' "<cedula>06914897</cedula><token>$t</token>"
    Verificar 'listarCuentasPorCliente' '200' $c
    Verificar 'listarCuentasPorCliente cliente inexistente' 'CLI-001' (Llamar $url 'listarCuentasPorCliente' "<cedula>99999999</cedula><token>$t</token>")
    Verificar 'consultarCuenta activa' '200' (Llamar $url 'consultarCuenta' "<cuenta>00200002</cuenta><token>$t</token>")
    Verificar 'consultarCuenta inexistente' 'CTA-001' (Llamar $url 'consultarCuenta' "<cuenta>99999999</cuenta><token>$t</token>")
    Verificar 'consultarCuenta cancelada' 'CTA-002' (Llamar $url 'consultarCuenta' "<cuenta>00300001</cuenta><token>$t</token>")
    Verificar 'consultarMovimientos' '200' (Llamar $url 'consultarMovimientos' "<cuenta>00200002</cuenta><token>$t</token>")

    $d = Llamar $url 'realizarDeposito' "<cuenta>00200001</cuenta><monto>10.00</monto><token>$t</token>"
    Verificar 'realizarDeposito' '200' $d
    Registrar 'realizarDeposito devuelve el importe con dos decimales' 'True' ([string]($d['saldoNuevo'] -match '^\d+\.\d{2}$'))
    Verificar 'realizarDeposito monto con tres decimales' 'MON-001' (Llamar $url 'realizarDeposito' "<cuenta>00200001</cuenta><monto>10.555</monto><token>$t</token>")
    Verificar 'realizarDeposito monto cero' 'MON-001' (Llamar $url 'realizarDeposito' "<cuenta>00200001</cuenta><monto>0</monto><token>$t</token>")
    Verificar 'realizarRetiro' '200' (Llamar $url 'realizarRetiro' "<cuenta>00200001</cuenta><monto>5.00</monto><token>$t</token>")
    Verificar 'realizarRetiro saldo insuficiente' 'SAL-001' (Llamar $url 'realizarRetiro' "<cuenta>00200001</cuenta><monto>99999999.00</monto><token>$t</token>")
    $x = Llamar $url 'realizarTransferencia' "<cuentaOrigen>00200002</cuentaOrigen><cuentaDestino>00200001</cuentaDestino><monto>100.00</monto><token>$t</token>"
    Verificar 'realizarTransferencia' '200' $x
    Verificar 'realizarTransferencia mismas cuentas' 'CTA-003' (Llamar $url 'realizarTransferencia' "<cuentaOrigen>00200002</cuentaOrigen><cuentaDestino>00200002</cuentaDestino><monto>1.00</monto><token>$t</token>")
    Verificar 'realizarTransferencia moneda distinta' 'CTA-004' (Llamar $url 'realizarTransferencia' "<cuentaOrigen>00200002</cuentaOrigen><cuentaDestino>00100002</cuentaDestino><monto>1.00</monto><token>$t</token>")
    Verificar 'consultarCostosMantenimiento' '200' (Llamar $url 'consultarCostosMantenimiento' "<cuenta>00200001</cuenta><token>$t</token>")

    Verificar 'token falso' 'AUTH-003' (Llamar $url 'consultarCuenta' '<cuenta>00200002</cuenta><token>falso</token>')
    Verificar 'sin token' 'DAT-001' (Llamar $url 'consultarCuenta' '<cuenta>00200002</cuenta><token></token>')
    Registrar 'las reglas de negocio no producen SOAP Fault' 'False' (Llamar $url 'consultarCuenta' '<cuenta>00200002</cuenta><token>falso</token>')['_fault']

    Verificar 'cerrarSesion' '200' (Llamar $url 'cerrarSesion' "<token>$t</token>")
    Verificar 'token tras cerrar sesion' 'AUTH-003' (Llamar $url 'consultarCuenta' "<cuenta>00200002</cuenta><token>$t</token>")
}
catch {
    Registrar 'El script termino sin errores' 'True' $_.Exception.Message
}
finally {
    if ($iis -and -not $iis.HasExited) { Stop-Process -Id $iis.Id -Force -ErrorAction SilentlyContinue }
    Remove-Item Env:\EUREKABANK_DB_ESQUEMA -ErrorAction SilentlyContinue
}

$resultados | Format-Table Estado, Caso, Esperado, Obtenido -AutoSize | Out-String -Width 220 | Write-Host
$fallos = @($resultados | Where-Object { $_.Estado -eq 'FAIL' }).Count
if ($fallos -gt 0) { Write-Host "RESULTADO: $fallos de $($resultados.Count) comprobaciones fallan."; exit 1 }
Write-Host "RESULTADO: las $($resultados.Count) comprobaciones pasan."
exit 0
