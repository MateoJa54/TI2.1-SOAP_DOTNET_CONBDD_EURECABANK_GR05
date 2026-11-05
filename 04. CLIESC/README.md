# Cliente de escritorio (CLIESC)

Cliente WinForms (.NET Framework 4.8) para `WSEUREKABANK`. Capas: `controlador`, `modelo`, `vista` (con `controles`), `util` y `ws`.

## Pantallas

Acceso (marca arriba y `Monster.png` debajo del formulario), y una ventana principal con las pestañas Cuentas, Movimientos, Operación (depósito, retiro y transferencia) y Costos. El botón **Cerrar sesión** está siempre visible.

## Reglas

- Ninguna llamada al servidor se hace en el hilo de la interfaz: todas pasan por `TareaAsincrona`.
- El cliente no reimplementa reglas de negocio. Muestra `[CODIGO] mensaje` tal como llega. Un monto que no es número muestra el literal de `MON-001` sin llamar al servidor; cualquier otro monto se envía tal cual.
- `AUTH-003` y `AUTH-004` devuelven al acceso con el mensaje del servidor.
- El detalle técnico de `SER-001` va a `%TEMP%\eurekabank-cliesc.log`, nunca a la pantalla.
- Diseño: paleta del proyecto, un solo acento, radios de 12 px (16 px en tarjetas), etiqueta encima de cada campo, estados de carga, vacío y error, sin animaciones.

## Uso

```
CLIESC\bin\Debug\net48\CLIESC.exe
```

Dirección del servicio: variable `WS_URL`, `ws.url` de `App.config` o `http://localhost:50775/WSEUREKABANK.svc`.

El proxy se regenera con `scripts\regenerar-proxy.ps1`. `CLIESC.Pruebas` tiene 402 pruebas MSTest (controladores, utilidades, `TareaAsincrona`, navegación y presentación) que no abren ventanas.

## Limitaciones

El acceso, la búsqueda de cuentas y la pantalla principal se vieron en ejecución real contra el servidor; las pestañas de movimientos, operación y costos se verifican por pruebas, no por captura.
