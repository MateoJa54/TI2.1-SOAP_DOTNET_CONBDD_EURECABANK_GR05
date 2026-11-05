# Cliente de consola (CLICON)

Cliente de consola en C# (.NET Framework 4.8) para el servicio `WSEUREKABANK`. Capas: `controlador`, `modelo`, `vista`, `util` y `ws`.

## Uso

```
CLICON\bin\Debug\net48\CLICON.exe [url]
```

La dirección del servicio se resuelve en este orden: argumento, variable de entorno `WS_URL`, `ws.url` de `App.config` y `http://localhost:50775/WSEUREKABANK.svc`.

Menú: cuentas de un cliente, consultar una cuenta, movimientos, depósito, retiro, transferencia, costos de mantenimiento y cerrar sesión.

## Reglas

- Los errores se muestran como `[CODIGO] mensaje`, tal como llegan.
- Un monto que no es un número muestra el literal de `MON-001` sin llamar al servidor. Cualquier monto numérico (cero, negativo, `10.555`) se envía tal cual y decide el servidor. Se acepta la coma como separador decimal solo si no hay punto (`10,50`), de modo que `1,234.56` se rechaza.
- `AUTH-003` y `AUTH-004` cierran la sesión local y devuelven al inicio de sesión.
- Un fallo de comunicación se muestra como `SER-001`; la URL y la causa van a `%TEMP%\eurekabank-clicon.log`.

## Proxy y pruebas

El proxy WCF (`Connected Services\Referencia\Reference.cs`) se genera con `scripts\regenerar-proxy.ps1` desde el WSDL local de `04. SERVIDOR\contrato`, de modo que el cliente compila con el servidor apagado. El archivo generado no se edita a mano.

`CLICON.Pruebas` tiene 294 pruebas MSTest con un cliente falso y sin red. Se ejecutan con `dotnet test CLICON.Pruebas\CLICON.Pruebas.csproj`.
