# Cliente web (CLIWEB)

Cliente ASP.NET MVC 5 (.NET Framework 4.8) para `WSEUREKABANK`. Controladores: `Cuenta` (acceso y cierre de sesión), `Cuentas`, `Movimientos`, `Operaciones` y `Costos`. Vistas Razor, `Content\site.css` y `Content\img\` (`marca.svg`, `Monster.png`).

## Uso

Con IIS Express en el puerto 50776 (`CLIWEB\CLIWEB.csproj` fija el puerto) y el servidor en `http://localhost:50775/WSEUREKABANK.svc` (`ws.url` de `Web.config`).

## Seguridad y reglas

- La sesión vive en el servidor (`HttpSessionState`). Un filtro global exige sesión en todas las páginas salvo el acceso, `Content\` y la página de error; sin sesión redirige al acceso.
- Todos los formularios llevan `AntiForgeryToken` y las acciones `POST` lo validan.
- `AUTH-003` y `AUTH-004` abandonan la sesión HTTP y devuelven al acceso con el mensaje del servidor.
- Una operación desconocida en `Operaciones` responde `DAT-001` sin llamar al servidor ni mover dinero: la lista de operaciones es cerrada.
- Un monto que no es número muestra el literal de `MON-001` sin llamar al servidor; cualquier otro monto se envía tal cual.
- La tabla de movimientos tiene cinco columnas: número, fecha, tipo, acción e importe.
- El detalle técnico de `SER-001` va a `%TEMP%\eurekabank-cliweb.log`.

## Pruebas

`CLIWEB.Pruebas` tiene 330 pruebas MSTest con un cliente falso y un contexto HTTP simulado. Se verificó además un recorrido HTTP real (acceso, cada pantalla con datos, errores, cierre de sesión y CSS sin sesión) y se vieron el acceso y la tabla de movimientos en el navegador.

## Limitación conocida

El acceso no regenera el identificador de la sesión HTTP: cierra en el servidor la sesión anterior y limpia la sesión local, pero el identificador de cookie se conserva, lo que deja abierto un riesgo de fijación de sesión.
