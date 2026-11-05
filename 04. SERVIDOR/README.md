# Servidor WSEUREKABANK (WCF, .NET Framework 4.8)

Servicio SOAP 1.1 `document/literal wrapped` en `http://localhost:50775/WSEUREKABANK.svc`, alojado en IIS Express. Las 9 operaciones, los 17 códigos y el orden de validación son los de `01. UML\00. CONTRATO\CONTRATO_CONGELADO_EUREKABANK.md`.

## Carpetas

| Carpeta | Contenido |
|---|---|
| `WSEUREKABANK` | Proyecto web WCF (`.svc`, `Web.config`) con las capas `controladores`, `seguridad`, `servicios`, `datos`, `modelos` y `utilidades` |
| `WSEUREKABANK.Pruebas` | MSTest: 341 pruebas sin base de datos y 15 de integración |
| `contrato` | `WSEUREKABANK.wsdl` y `WSEUREKABANK_schema.xsd`, la fuente de los proxies de los clientes |
| `scripts` | `compilar-y-probar.ps1` y `probar-iisexpress.ps1` |

## Configuración

Cada valor se lee de la variable de entorno y, si no existe, de `appSettings` de `Web.config`: `EUREKABANK_DB_SERVIDOR` (por defecto `localhost`), `EUREKABANK_DB_ESQUEMA` (por defecto `eurekabank`) y `EUREKABANK_TOKEN_MINUTOS`. La base usa autenticación de Windows, así que no hay usuario ni clave en la configuración.

## Cómo compilar y probar

```
powershell -ExecutionPolicy Bypass -File scripts\compilar-y-probar.ps1
powershell -ExecutionPolicy Bypass -File scripts\compilar-y-probar.ps1 -Integracion
powershell -ExecutionPolicy Bypass -File scripts\probar-iisexpress.ps1
```

Las pruebas de integración y el recorrido de `probar-iisexpress.ps1` escriben saldos y filas de `logsession`, por eso usan el esquema `eurekabank_prueba` (`03. BDD\SQLSERVER\cargar_bdd_sqlserver.ps1 -Esquema eurekabank_prueba -Recrear`) y nunca `eurekabank`.

## Decisiones de diseño

- **Token** como último parámetro de cada operación, igual que en Java.
- **Sin SOAP Fault por reglas de negocio**: la fachada convierte cualquier excepción inesperada en `ERR-001` y registra el detalle en `%TEMP%\eurekabank-servidor.log`.
- **SOAPAction vacío**: `DespachoPorElementoCuerpo` elige la operación por el elemento raíz del cuerpo, como hace JAX-WS, para que el cliente Java funcione sin cambios.
- **Contenedor `<return>`**: XmlSerializer no admite dos elementos `return` de tipos distintos en el mismo contrato, así que `Retorno` arma el cuerpo de `<return>` aparte. El esquema resultante se valida contra el XSD congelado en `FachadaTest`.
- **Saldo**: retiro y transferencia validan `saldo >= monto + cargo (+ ITF)` y responden `SAL-001` sin escribir. Un depósito cuyo cargo por movimiento superaría `saldo + monto` también responde `SAL-001`.
- **Concurrencia**: cada movimiento es una transacción y la cuenta se lee con `WITH (UPDLOCK, ROWLOCK)`; en transferencias las dos cuentas se bloquean siempre en el mismo orden.
- **Clave**: se compara en tiempo constante con una función propia, porque `CryptographicOperations.FixedTimeEquals` no existe en .NET Framework 4.8.
- **Base de datos**: SQL Server con autenticación de Windows. El servidor no guarda claves; la cuenta de Windows que ejecuta IIS Express necesita acceso a la base.

## Pendiente conocido

Sin pendientes conocidos. No se ha probado todavía el servidor desde otra PC.
