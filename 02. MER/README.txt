================================================================================
02. MER - MODELO ENTIDAD-RELACION DE EURECABANK
Proyecto: TI2.1 SOAP_DOTNET_CONBDD_EURECABANK_GR05
================================================================================

1. QUE ES EL MER
--------------------------------------------------------------------------------
El Modelo Entidad-Relacion (MER) describe el modelo de datos del sistema
EURECABANK en tres niveles de abstraccion, cada uno en su propia carpeta y con
su propio proyecto de PowerDesigner:

  01. CONCEPTUAL  -> MC_EUREKABANK.cdm   (CDM) - el modelo conceptual
  02. LOGICO      -> ML_EUREKABANK.ldm   (LDM) - el modelo logico
  03. FISICO      -> MF_EUREKABANK.pdm   (PDM) - el modelo fisico, SQL Server

La fuente de verdad de los datos es el DDL de SQL Server
"03. BDD\SQLSERVER\01_estructura.sql" (esquema "eurekabank", 17 tablas) y el
CONTRATO CONGELADO (01. UML\00. CONTRATO). Ninguna tabla, columna, tipo o
restriccion de este MER es inventada: el modelo fisico se comprueba tabla por
tabla y columna por columna contra ese DDL.

Los tres PNG se exportan a "05. DOCUMENTACION\img\" para incrustarlos en el
ERS, el ECUD y el manual:
  MC_EUREKABANK.png, ML_EUREKABANK.png, MF_EUREKABANK.png


2. QUE CONTIENE CADA MODELO
--------------------------------------------------------------------------------
A) 02. MER\01. CONCEPTUAL\MC_EUREKABANK.cdm  (proyecto: 01_MODELO_CONCEPTUAL.prj)
   Modelo conceptual (CDM). Contiene las 17 entidades del negocio con sus
   atributos de NEGOCIO (nombres significativos, no las claves tecnicas
   chr_*, vch_*, dec_*), un identificador natural por entidad (ID_<ENTIDAD>),
   18 relaciones con sus cardinalidades y la autorrelacion logica de la
   cuenta referida. Entidades: CLIENTE, EMPLEADO, USUARIO, SUCURSAL, MONEDA,
   CUENTA, MOVIMIENTO, TIPO_MOVIMIENTO, ASIGNADO, MODULO, PERMISO, PARAMETRO,
   COSTO_MANTENIMIENTO, COSTO_MOVIMIENTO, INTERES_MENSUAL, CONTADOR,
   LOG_SESION.

B) 02. MER\02. LOGICO\ML_EUREKABANK.ldm  (proyecto: 02_MODELO_LOGICO.prj)
   Modelo logico (LDM). 17 entidades con los nombres de la guia en mayusculas
   (ASIGNADO, CARGO_MANTENIMIENTO, CLIENTE, CONTADOR, COSTO_MOVIMIENTO, CUENTA,
   EMPLEADO, INTERES_MENSUAL, LOG_SESSION, MODULO, MONEDA, MOVIMIENTO,
   PARAMETRO, PERMISO, SUCURSAL, TIPO_MOVIMIENTO, USUARIO) y los 78 atributos
   con el nombre de columna EXACTO de la BDD. Tipos de dato logicos:
   string(n), integer, decimal(12,2), date, datetime. Incluye clave primaria
   (PK_<ENTIDAD>, compuesta donde el DDL la tiene), regla de nulabilidad
   (Mandatory) y 17 relaciones con cardinalidad, roles y el join que declara
   la clave foranea logica.

C) 02. MER\03. FISICO\MF_EUREKABANK.pdm  (proyecto: 03_MODELO_FISICO.prj)
   Modelo fisico (PDM) con destino "Microsoft SQL Server 2012" (modulo
   sqlsv2012.xdb de PowerDesigner 16.5). 17 tablas, tipos char(n), varchar(n),
   decimal(12,2), int, date y datetime, obligatoriedad, valores por defecto,
   clave primaria con su nombre, claves foraneas con nombre de restriccion y
   reglas ON DELETE/ON UPDATE RESTRICT, indices (con U_Usuario_vch_emplusuario
   unico) y los CHECK de estado y de importe, mas los comentarios de tabla y de
   columna del DDL.

   El destino original era MySQL 5.0 y se cambio a SQL Server 2012 con
   ChangeDBMS; PowerDesigner convirtio las columnas date a datetime y se
   restauraron a date (columnas dtt_*) para coincidir con el DDL de SQL Server.
   Respaldo previo al cambio: fuera del repositorio.


3. COMO SE ABRE CADA UNO
--------------------------------------------------------------------------------
Opcion A - Workspace (recomendado):
  1. Abrir PowerDesigner 16.5 (32 bits) desde
     "C:\Program Files (x86)\Sybase\PowerDesigner 16".
  2. File > Open Workspace y elegir "SOAP_DOTNET_CONBDD_GR05.sws" en la raiz
     del proyecto. El workspace registra los tres .prj y los tres modelos con
     rutas relativas.

Opcion B - Proyecto suelto:
  File > Open y elegir cualquiera de estos archivos .prj:
     02. MER\01. CONCEPTUAL\01_MODELO_CONCEPTUAL.prj
     02. MER\02. LOGICO\02_MODELO_LOGICO.prj
     02. MER\03. FISICO\03_MODELO_FISICO.prj

Opcion C - Modelo suelto:
  File > Open y elegir el .cdm / .ldm / .pdm directamente.

Todos los .prj, el .sws y los modelos usan rutas RELATIVAS: el proyecto se
puede mover de carpeta sin romper las referencias.


4. COMO SE VERIFICA
--------------------------------------------------------------------------------
  - El PDM (fisico) tiene las 17 tablas del DDL de SQL Server, con PK, FK,
    indices, CHECK, valores por defecto y comentarios.
  - El LDM (logico) tiene las mismas 17 entidades con atributos de tipo logico,
    PK, FK y cardinalidades.
  - El CDM (conceptual) tiene las entidades de negocio y sus relaciones.

Si se modifica el DDL hay que actualizar el PDM a mano en PowerDesigner y
volver a comprobar que coincide con "03. BDD\SQLSERVER\01_estructura.sql".


5. FICHEROS DE ESTA CARPETA
--------------------------------------------------------------------------------
  README.txt                       este fichero
  01. CONCEPTUAL\                  modelo conceptual (CDM) + su proyecto .prj
  02. LOGICO\                      modelo logico (LDM) + su proyecto .prj
  03. FISICO\                      modelo fisico (PDM) + su proyecto .prj

Los modelos estan registrados en el workspace "SOAP_DOTNET_CONBDD_GR05.sws"
(raiz del proyecto) con rutas relativas, y sus imagenes van incrustadas en el
ERS, el ECUD y el manual tecnico.
================================================================================
