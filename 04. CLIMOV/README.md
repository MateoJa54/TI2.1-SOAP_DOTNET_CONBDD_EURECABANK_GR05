# Cliente móvil EUREKABANK SOAP .NET

Aplicación Android nativa en Java, con paquetes `ec.edu.monster` y estructura MVC. Reutiliza el
contrato SOAP congelado del servidor .NET y la guía visual del cliente móvil Java.

## Operaciones

- Inicio y cierre de sesión (credenciales con SHA-256).
- Listar cuentas por cédula y consultar una cuenta.
- Consultar movimientos.
- Depósito, retiro y transferencia.
- Consultar costos de mantenimiento.

Los resultados conservan el código y el mensaje que devuelve el servidor. Como en CLICON, solo se
rechaza localmente el texto que no es un número; cero, negativos y más de dos decimales se envían
para que el servidor aplique `MON-001`.

## Servidor

La pantalla de acceso guarda la URL elegida. El valor inicial funciona desde el emulador Android:

`http://10.0.2.2:50775/WSEUREKABANK.svc`

Para un teléfono físico conectado a la misma red Wi-Fi, reemplaza `10.0.2.2` por la IPv4 LAN del
equipo donde corre IIS Express, por ejemplo `http://192.168.1.25:50775/WSEUREKABANK.svc`. IIS
Express y el firewall de Windows deben aceptar conexiones entrantes en el puerto 50775. `localhost`
en un teléfono apunta al propio teléfono, no al PC. Si el servidor usa otra ruta o puerto, cambia
la URL completa en el campo **Servidor**.

Para conectarse desde otro dispositivo, IIS Express también debe publicar el sitio en la interfaz
de red (su configuración de desarrollo puede escuchar solo en `localhost`); configura el binding
HTTP para la IP LAN y permite el puerto en el firewall de Windows.

## Abrir y compilar

Abre `04. CLIMOV/CLIMOV` en Android Studio. El `applicationId` es
`ec.edu.monster.eurekabank.soapdotnet`, el nombre visible es `EurekaBan_SoapDotnet` y el APK
compilado para entrega queda en `04. CLIMOV/APK/EurekaBan_SoapDotnet.apk`.

Pruebas unitarias (no requieren red ni servidor):

```powershell
cd "04. CLIMOV\CLIMOV"
./gradlew.bat testDebugUnitTest
```

Las pruebas cubren serialización/parsing SOAP, autenticación y hash, sesión, las siete operaciones,
orden de parámetros y validación local de montos.
