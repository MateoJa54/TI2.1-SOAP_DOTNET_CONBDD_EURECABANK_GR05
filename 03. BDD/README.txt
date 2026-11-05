================================================================================
03. BDD - Base de datos EURECABANK (SQL Server, versión .NET)
================================================================================

QUE ES
  Base de datos de EURECABANK: esquema "eurekabank" con 17 tablas y semilla
  de datos de prueba (personas y documentos ficticios). Motor: SQL Server
  (instancia MSSQLSERVER, interpretación T-SQL). Autenticación de Windows.

CONTENIDO
  SQLSERVER\00_esquema.sql        Crea la base con collation Latin1_General_100_CI_AS
  SQLSERVER\01_estructura.sql     Las 17 tablas (PK, FK, CHECK) y 15 índices
  SQLSERVER\02_datos.sql          Semilla: 14 empleados, 20 clientes,
                                  7 sucursales, 2 monedas, 10 tipos de movimiento,
                                  11 asignados, 6 módulos, 6 cuentas,
                                  74 movimientos, 2 parámetros, 42 permisos,
                                  11 usuarios y 7 contadores
  SQLSERVER\04_carga_prueba.sql   Procedimiento dbo.sp_cargar_datos_prueba
  cargar_bdd_sqlserver.ps1        Ejecuta los scripts en orden y verifica los conteos

COMO LEVANTARLA (desde esta carpeta, PowerShell)
  Base de datos de uso normal (eurekabank):
      .\SQLSERVER\cargar_bdd_sqlserver.ps1
  Base de pruebas que el servidor puede modificar (eurekabank_prueba):
      .\SQLSERVER\cargar_bdd_sqlserver.ps1 -Esquema eurekabank_prueba -Recrear

  Sin -Recrear el script se detiene si la base ya existe, para no perder datos.
  Con -Recrear borra la base y la carga de nuevo.

CREDENCIALES
  No hay usuario ni clave de aplicación. La conexión usa autenticación de
  Windows (Integrated Security); la cuenta de Windows que ejecuta el servidor
  necesita acceso a la base.

RECARGA RÁPIDA DE LOS DATOS DE PRUEBA (no usar en producción)
      EXEC dbo.sp_cargar_datos_prueba;
  Borra y recarga movimiento, cuenta y logsession, y recalcula dec_cuensaldo e
  int_cuencontmov a partir de los movimientos.
================================================================================
