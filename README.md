# EURECABANK, versión SOAP en .NET

Caso de estudio de Arquitectura de Software (ESPE, docente Campaña, empresa ficticia Monster). Es el mismo sistema que la versión Java (`SOAP_JAVA_CONBDD_GR03`): mismos requisitos, base de datos MySQL en la versión Java y SQL Server en la versión .NET, mismo contrato SOAP y mismo comportamiento. Cambia la tecnología: servidor WCF sobre .NET Framework 4.8 alojado en IIS Express, y clientes de consola, escritorio (WinForms) y web (ASP.NET MVC 5). El cliente móvil no forma parte de esta versión.

## Estructura

| Carpeta | Contenido |
|---|---|
| `01. UML` | Contrato congelado, ERS, ECUD y diagramas de PowerDesigner |
| `02. MER` | Modelos conceptual, lógico y físico (idénticos a la versión Java) |
| `03. BDD\SQLSERVER` | Scripts T-SQL de SQL Server: `cargar_bdd_sqlserver.ps1`, esquema, datos y procedimiento de carga de prueba |
| `04. SERVIDOR` | `WSEUREKABANK` (WCF), `WSEUREKABANK.Pruebas` (MSTest), `contrato` (WSDL y XSD), `scripts` |
| `04. CLICON` | Cliente de consola |
| `04. CLIESC` | Cliente de escritorio WinForms |
| `04. CLIWEB` | Cliente web ASP.NET MVC 5 |
| `05. DOCUMENTACION` | Manual y PNG de los diagramas |

## Decisiones del grupo

- El token de sesión es el último parámetro de cada operación, igual que en Java. No va en el encabezado SOAP.
- Espacio de nombres base: `ec.edu.monster`. Base de datos: `eurekabank` en SQL Server (MSSQLSERVER), con autenticación de Windows. Acceso con `System.Data.SqlClient`.
- Retiro y transferencia validan `saldo >= monto + cargo por movimiento` (más el ITF en la transferencia) y responden `SAL-001` sin escribir (RB-03 ampliada).
- Las pruebas que escriben saldos usan el esquema `eurekabank_prueba`, nunca `eurekabank`.
- Endpoint local: `http://localhost:50775/WSEUREKABANK.svc`.

## Cómo compilar y probar

Requisitos: Visual Studio (MSBuild con la carga de trabajo web), .NET Framework 4.8, IIS Express, SQL Server (instancia MSSQLSERVER) con las herramientas `sqlcmd`.

```
powershell -ExecutionPolicy Bypass -File "04. SERVIDOR\scripts\compilar-y-probar.ps1"
powershell -ExecutionPolicy Bypass -File "03. BDD\SQLSERVER\cargar_bdd_sqlserver.ps1" -Esquema eurekabank_prueba -Recrear
powershell -ExecutionPolicy Bypass -File "04. SERVIDOR\scripts\probar-iisexpress.ps1"
```

## Estado

Servidor WCF y cliente de consola construidos; el resto está en curso. Consulta la lista de pendientes con el responsable del repositorio.
