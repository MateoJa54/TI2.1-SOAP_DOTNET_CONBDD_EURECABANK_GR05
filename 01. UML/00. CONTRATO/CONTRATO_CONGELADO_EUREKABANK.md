# CONTRATO CONGELADO — WSEUREKABANK v1.0

> **Documento de verdad única.** Todo el proyecto (BDD, ERS, ECUD, WSDL/XSD, diagramas
> PowerDesigner, servidor .NET y clientes de consola, escritorio y web) debe derivar **exclusivamente** de este archivo.
> Si algo discrepa, este archivo gana. Cambiarlo exige actualizar los 5 artefactos en el mismo día.
>
> Congelado: **2026-11-03** · Versión: **1.0** · Estado: **APROBADO**

---

## 1. Identificación del servicio

| Ítem | Valor |
|---|---|
| Nombre del servicio | `WSEUREKABANK` |
| Estilo SOAP | `document` / `literal` / **wrapped** |
| `targetNamespace` | `http://ws.monster.edu.ec/` |
| Puerto HTTP (IIS Express) | 50775 |
| Archivo de servicio | `WSEUREKABANK.svc` (WCF sobre .NET Framework 4.8) |
| **Endpoint de producción local** | `http://localhost:50775/WSEUREKABANK.svc` |
| URL del WSDL publicado | `http://localhost:50775/WSEUREKABANK.svc?singleWsdl` |
| Token de sesión | último parámetro de cada operación (decisión del grupo; no va en el encabezado SOAP) |
| Archivos del contrato | `04. SERVIDOR/contrato/WSEUREKABANK.wsdl` y `04. SERVIDOR/contrato/WSEUREKABANK_schema.xsd` |
| Base de datos | SQL Server (MSSQLSERVER), base `eurekabank`, autenticación de Windows |

---

## 2. Modelo de datos (17 tablas, heredado de `03.BDD` de la guía)

| Tabla | PK | FK | Propósito |
|---|---|---|---|
| `empleado` | `chr_emplcodigo` char(4) | — | Maestro de empleados (14 filas de prueba) |
| `usuario` | `chr_emplcodigo` char(4) | → `empleado` | Credenciales. `vch_emplusuario` único, `vch_emplclave` = SHA-256 hex (64) |
| `permiso` | (`chr_emplcodigo`,`int_moducodigo`) | → `usuario`, `modulo` | Permisos por módulo (6 módulos) |
| `modulo` | `int_moducodigo` int | — | Catálogo de módulos (Procesos, Tablas, Consultas, Reportes, Util, Seguridad) |
| `cliente` | `chr_cliecodigo` char(5) | — | Clientes del banco (20 filas), `chr_cliedni` char(8) |
| `sucursal` | `chr_sucucodigo` char(3) | — | 7 sucursales |
| `moneda` | `chr_monecodigo` char(2) | — | `01` = Soles, `02` = Dólares |
| `cuenta` | `chr_cuencodigo` char(8) | → `cliente`, `moneda`, `sucursal`, `empleado` | Saldo `dec_cuensaldo`, estado `vch_cuenestado` ∈ {ACTIVO, ANULADO, CANCELADO}, `int_cuencontmov` |
| `movimiento` | (`chr_cuencodigo`,`int_movinumero`) | → `cuenta`, `empleado`, `tipomovimiento` | Libro de movimientos; `chr_cuenreferencia` para transferencias |
| `tipomovimiento` | `chr_tipocodigo` char(3) | — | 10 tipos, `vch_tipoaccion` ∈ {INGRESO, SALIDA} |
| `cargomantenimiento` | `chr_monecodigo` | → `moneda` | `dec_cargMontoMaximo` (umbral), `dec_cargImporte` (costo fijo) |
| `costomovimiento` | `chr_monecodigo` | → `moneda` | `dec_costimporte` (costo por movimiento) |
| `interesmensual` | `chr_monecodigo` | → `moneda` | `dec_inteimporte` (porcentaje mensual) |
| `parametro` | `chr_paracodigo` char(3) | — | `001` = ITF (0.08), `002` = Nº operaciones sin costo (15) |
| `contador` | `vch_conttabla` | — | Secuencias de PK por tabla |
| `logsession` | `ID` autoinc | → `empleado` | `fec_ingreso`, `fec_salida`, `ip`, `hostname` |
| `asignado` | `chr_asigcodigo` char(6) | → `empleado`, `sucursal` | Asignación empleado-sucursal |

### Valores de `tipomovimiento` (congelados)

| Código | Descripción | Acción |
|---|---|---|
| `001` | Apertura de Cuenta | INGRESO |
| `002` | Cancelar Cuenta | SALIDA |
| `003` | Deposito | INGRESO |
| `004` | Retiro | SALIDA |
| `005` | Interes | INGRESO |
| `006` | Mantenimiento | SALIDA |
| `007` | ITF | SALIDA |
| `008` | Transferencia | INGRESO |
| `009` | Transferencia | SALIDA |
| `010` | Cargo por Movimiento | SALIDA |

### Valores de `parametro` (congelados)

| Código | Descripción | Valor | Interpretación |
|---|---|---|---|
| `001` | ITF - Impuesto a las Transacciones Financieras | `0.08` | **Porcentaje**: ITF = round(monto × 0.08 / 100, 2) |
| `002` | Número de Operaciones Sin Costo | `15` | El movimiento N es sin costo si N ≤ 15 |

---

## 3. Sobre de respuesta

Todo lo que devuelve el servicio es un `RespuestaBase` **extendido** con un tipo específico.

**`RespuestaBase`** (atributos, en este orden):

| Atributo | Tipo | Obligatorio | Descripción |
|---|---|---|---|
| `estado` | `string` | sí | `OK` o `ERROR` |
| `codigo` | `string` | sí | Código de resultado (§5) |
| `mensaje` | `string` | sí | Mensaje literal en español (§5) |

Tipos derivados (todos heredan de `RespuestaBase`):

| Tipo | Atributos propios |
|---|---|
| `RespuestaAutenticacion` | `token` (string), `empleado` (string), `nombreEmpleado` (string), `expiracion` (string ISO-8601), `rol` (string) |
| `RespuestaCuentas` | `cedula` (string), `nombreCliente` (string), `cuentas` (`CuentaDTO[]`, `maxOccurs=unbounded`) |
| `RespuestaCuenta` | `cuenta` (`CuentaDTO`) |
| `RespuestaMovimientos` | `cuenta` (string), `movimientos` (`MovimientoDTO[]`, `maxOccurs=unbounded`) |
| `RespuestaMovimiento` | `movimiento` (`MovimientoDTO`), `saldoNuevo` (decimal), `costoAplicado` (decimal), `itfAplicado` (decimal) |
| `RespuestaTransferencia` | `numeroOrigen` (int), `numeroDestino` (int), `saldoOrigen` (decimal), `saldoDestino` (decimal), `itfAplicado` (decimal) |
| `RespuestaCostos` | `saldoActual` (decimal), `costoMantenimiento` (decimal), `costoMovimiento` (decimal), `interesProyectado` (decimal), `limiteMantenimiento` (decimal), `operacionesSinCosto` (int) |

**Tipos de entidad:**

| Tipo | Atributos |
|---|---|
| `CuentaDTO` | `codigo` (string), `moneda` (string), `descripcionMoneda` (string), `saldo` (decimal), `estado` (string), `codigoCliente` (string), `nombreCliente` (string), `codigoSucursal` (string), `fechaCreacion` (string ISO-8601 `yyyy-MM-dd`), `clave` (string), `operacionesRealizadas` (int) |
| `MovimientoDTO` | `numero` (int), `fecha` (string ISO-8601 `yyyy-MM-dd`), `tipoCodigo` (string), `tipoDescripcion` (string), `tipoAccion` (string), `importe` (decimal), `referencia` (string), `empleado` (string) |

> **Nota de la versión .NET.** El XSD de este contrato es el mismo de la versión Java y es la fuente de verdad del
> formato en el cable. En .NET `decimal` ↔ `xs:decimal`, `int` ↔ `xs:int` y los modelos se serializan con
> `XmlSerializer` (`[XmlElement(Order = n, Form = Unqualified)]` y `[XmlType("respuestaX")]`), de modo que el orden
> alfabético de los elementos, los elementos locales sin espacio de nombres y los nombres de tipo en minúscula
> coinciden con el esquema (lo comprueba `FachadaTest`, que valida cada respuesta contra este XSD). Las menciones
> de JAXB, POJO y Metro de las viñetas siguientes describen la versión de referencia y se cumplen aquí con
> `XmlSerializer`. Un cliente Java generado desde este mismo WSDL funciona contra el servidor .NET cambiando solo la URL.
>
> **Correspondencia con el XSD (obligatoria, no negociable):**
> - `decimal` ↔ `xs:decimal` ↔ Java `BigDecimal`.
> - `int` ↔ `xs:int` ↔ Java `Integer`.
> - `string` ↔ `xs:string` ↔ Java `String`.
> - **Los campos de fecha y hora son `xs:string` con formato ISO-8601**, NO `xs:date`/`xs:dateTime`.
>   Razón: los modelos son POJOs planos sin anotaciones JAXB (regla del docente: en `modelos` solo
>   atributos, constructores, getters y setters) y JAXB no puede deserializar un `xs:dateTime` a un
>   campo `String`. Es el mismo criterio que usa el proyecto `TI.1.1` del docente
>   (`expiracion` es `xs:string` en su XSD).
> - **El XSD usa `elementFormDefault="unqualified"`** (no cualificado). Con elementos cualificados,
>   los POJOs planos no deserializan porque los hijos llegan con namespace y el mapeo por defecto de
>   JAXB los espera sin namespace. El `TI.1.1` usa el criterio inverso (nocualificado) y funciona.
> - Los elementos globales (`autenticar`, `autenticarResponse`, …) sí van con namespace `tns:`.
> - El contenedor de respuesta es `<return>` (no campos planos): Metro **rechaza** la forma plana
>   cuando un parámetro y un campo del sobre comparten nombre (`cuenta`, `cedula`).
> - El tipo base del XSD se llama `RespuestaBaseTipo` y los derivados usan `xs:extension`.
>   La herencia en Java (`RespuestaAutenticacion extends RespuestaBase`) sí propaga los campos
>   heredados, porque ambas clases están en el mismo contexto JAXB.
> - El nombre de la clase Java **no** tiene por qué coincidir con el nombre del tipo del XSD:
>   el servidor y el cliente usan **POJOs escritos a mano**, no las clases generadas por `wsimport`.
>   **Pero el XSD usa exactamente el nombre que genera Metro** (la inicial del nombre de la clase
>   Java en minúscula), de modo que el WSDL **publicado por el servidor sea idéntico** al contrato
>   escrito a mano:
>   `RespuestaBase`→`respuestaBase`, `RespuestaAutenticacion`→`respuestaAutenticacion`,
>   `RespuestaCuentas`→`respuestaCuentas`, `RespuestaCuenta`→`respuestaCuenta`,
>   `RespuestaMovimientos`→`respuestaMovimientos`, `RespuestaMovimiento`→`respuestaMovimiento`,
>   `RespuestaTransferencia`→`respuestaTransferencia`, `RespuestaCostos`→`respuestaCostos`,
>   `CuentaDTO`→`cuentaDTO`, `MovimientoDTO`→`movimientoDTO`.
>   Los tipos de petición y respuesta de operación se llaman como la operación
>   (`autenticar`, `autenticarResponse`, …). Es el mismo criterio que usa el `TI.1.1`.
> - **Orden de los elementos en el XML**: los tipos de **respuesta y entidad**
>   (`respuestaBase`, `respuestaAutenticacion`, `respuestaCuentas`, `respuestaCuenta`,
>   `respuestaMovimientos`, `respuestaMovimiento`, `respuestaTransferencia`, `respuestaCostos`,
>   `cuentaDTO`, `movimientoDTO`) declaran sus elementos **en orden alfabético**, porque es el
>   orden en que JAXB los serializa cuando no hay `@XmlType(propOrder)` (que el docente no
>   permite en `modelos`). Los tipos de **entrada** (`autenticar`, `realizarTransferencia`, …)
>   mantienen el **orden de los parámetros** de la operación, que lo firma la interfaz SEI.
>   Verificado: el esquema que publica el servidor coincide con este contrato en
>   **28 tipos y 78 elementos, con 0 diferencias**.
> - Todos los elementos **locales** llevan `minOccurs="0"` (como en el esquema publicado por
>   Metro); los **globales** no pueden llevarlo (lo prohíbe la especificación XSD).
> - `respuestaBase` **no** es abstracta en el XSD (porque `cerrarSesion` devuelve
>   `tns:respuestaBase` y JAXB necesita una clase instanciable). En Java, `RespuestaBase` es
>   una clase concreta con los tres atributos comunes.

> En Java los atributos viajan en **minúscula camelCase** (`codigo`, `saldo`, `numero`) y los nombres
> de clase son exactamente los de arriba. El `estado` y el `codigo` **siempre** se llenan, incluso en error.

---

## 4. Operaciones (9, congeladas)

Convención: el **token va siempre al último parámetro**, salvo en `autenticar` (que lo produce).

| # | Operación | Parámetros | Tipo de respuesta | CU |
|---|---|---|---|---|
| 1 | `autenticar` | `usuario` (string), `clave` (string) | `RespuestaAutenticacion` | CU-01 |
| 2 | `cerrarSesion` | `token` (string) | `RespuestaBase` | CU-01 |
| 3 | `listarCuentasPorCliente` | `cedula` (string), `token` (string) | `RespuestaCuentas` | CU-03 |
| 4 | `consultarCuenta` | `cuenta` (string), `token` (string) | `RespuestaCuenta` | CU-03 |
| 5 | `consultarMovimientos` | `cuenta` (string), `token` (string) | `RespuestaMovimientos` | CU-04 |
| 6 | `realizarDeposito` | `cuenta` (string), `monto` (decimal), `token` (string) | `RespuestaMovimiento` | CU-05 |
| 7 | `realizarRetiro` | `cuenta` (string), `monto` (decimal), `token` (string) | `RespuestaMovimiento` | CU-06 |
| 8 | `realizarTransferencia` | `cuentaOrigen` (string), `cuentaDestino` (string), `monto` (decimal), `token` (string) | `RespuestaTransferencia` | CU-07 |
| 9 | `consultarCostosMantenimiento` | `cuenta` (string), `token` (string) | `RespuestaCostos` | CU-08 |

---

## 5. Catálogo de códigos y mensajes (congelado)

**Regla:** el `mensaje` del sobre es **exactamente** la cadena de esta tabla. El ERS, el ECUD, el
`enum` del servidor y el cliente deben usar la misma cadena, sin variantes.

| Código | Categoría | HTTP | Mensaje literal (obligatorio) |
|---|---|---|---|
| `200` | Éxito | 200 | *(varía por operación — ver §8)* |
| `DAT-001` | Error de datos | 400 | `Los datos de la petición están incompletos o son incorrectos` |
| `AUTH-001` | Autenticación | 401 | `El usuario o la contraseña son incorrectos` |
| `AUTH-002` | Autenticación | 403 | `El usuario se encuentra inactivo` |
| `AUTH-003` | Autorización | 401 | `El token de sesión no es válido` |
| `AUTH-004` | Autorización | 401 | `El token de sesión ha expirado, inicie sesión nuevamente` |
| `CTA-001` | Cuenta | 404 | `La cuenta solicitada no existe` |
| `CTA-002` | Cuenta | 409 | `La cuenta no se encuentra en estado ACTIVO` |
| `CTA-003` | Cuenta | 400 | `La cuenta origen y la cuenta destino no pueden ser iguales` |
| `CTA-004` | Cuenta | 409 | `La cuenta de destino pertenece a una moneda distinta` |
| `CLI-001` | Cliente | 404 | `El cliente solicitado no existe` |
| `SAL-001` | Saldo | 409 | `El saldo de la cuenta es insuficiente para la operación` |
| `MON-001` | Monto | 400 | `El monto debe ser mayor que cero y con máximo dos decimales` |
| `MON-002` | Moneda | 409 | `La moneda de la cuenta no está soportada por el sistema` |
| `LIM-001` | Límite | 409 | `El monto supera el límite máximo de mantenimiento de la cuenta` |
| `SER-001` | Servicio | 503 | `El servicio no está disponible en este momento` |
| `ERR-001` | Sistema | 500 | `Ocurrió un error interno, intente nuevamente` |

### Mensajes de éxito por operación

| Operación | `mensaje` con `codigo` 200 |
|---|---|
| `autenticar` | `Autenticación exitosa` |
| `cerrarSesion` | `Sesión cerrada correctamente` |
| `listarCuentasPorCliente` | `Cuentas consultadas correctamente` |
| `consultarCuenta` | `Cuenta consultada correctamente` |
| `consultarMovimientos` | `Movimientos consultados correctamente` |
| `realizarDeposito` | `Depósito realizado correctamente` |
| `realizarRetiro` | `Retiro realizado correctamente` |
| `realizarTransferencia` | `Transferencia realizada correctamente` |
| `consultarCostosMantenimiento` | `Costos de mantenimiento consultados correctamente` |

---

## 6. Orden de validación (obligatorio, idéntico en ERS, ECUD, diagramas y código)

### 6.1 Operaciones autenticadas (3 a 9)

```
1. DAT-001  → falta el token (null/vacío)
2. AUTH-003 → el token no existe en el registro de sesiones
3. AUTH-004 → el token expiró (TTL vencido)
4. DAT-001  → falta un parámetro propio de la operación (cuenta, cedula, monto, cuentaDestino)
5. MON-001  → monto ≤ 0 o con más de 2 decimales   [solo 6, 7, 8]
6. CTA-003  → cuenta origen = cuenta destino       [solo 8]
7. CTA-001  → la cuenta no existe
8. CTA-002  → la cuenta no está en estado ACTIVO
9. CTA-004  → la cuenta destino es de otra moneda   [solo 8]
10. SAL-001 → saldo insuficiente                    [solo 7, 8]
11. MON-002 → la moneda de la cuenta no está soportada
12. Éxito o error de persistencia → ERR-001
```

> El orden importa: si una petición viola dos reglas, **gana la que aparece primero en la lista**.
> Los flujos alternativos del ECUD deben respetar esta precedencia.

### 6.2 `autenticar`

```
1. DAT-001  → usuario o clave vacíos
2. AUTH-001 → el usuario no existe o el hash SHA-256 no coincide
3. AUTH-002 → `usuario.vch_emplestado` ∈ {ANULADO, CANCELADO}
4. Éxito    → genera token (TTL 30 min), inserta `logsession`
```

### 6.3 `listarCuentasPorCliente`

```
1. DAT-001 → cedula vacía o token vacío
2. AUTH-003 / AUTH-004 → validación de token
3. CLI-001 → no existe cliente con ese DNI
4. Éxito   → devuelve todas sus cuentas (incluidas no ACTIVO, con su estado visible)
```

### 6.4 `cerrarSesion`

```
1. DAT-001 → token vacío
2. AUTH-003 → el token no existe
3. AUTH-004 → el token expiró (igualmente se cierra la sesión en `logsession`)
4. Éxito    → marca `logsession.fec_salida` y descarta el token
```

---

## 7. Reglas de negocio (congeladas)

| Regla | Enunciado | Fuente |
|---|---|---|
| **RB-01** | Todo importe se maneja con `decimal` (.NET), escala 2, redondeo `MidpointRounding.AwayFromZero` (equivale a `HALF_UP`). Nunca `double`/`float`. | BDD `decimal(12,2)` |
| **RB-02** | Un movimiento es una transacción única: actualiza `cuenta.dec_cuensaldo`, `cuenta.int_cuencontmov` e inserta en `movimiento`. Si algo falla, **rollback completo**. | Integridad referencial |
| **RB-03** | El saldo se bloquea con `SELECT ... FOR UPDATE` antes de operar; el saldo nunca queda negativo. El retiro exige `saldo >= monto + cargo por movimiento` y la transferencia `saldo >= monto + ITF + cargo por movimiento`; si no alcanza responde `SAL-001` sin escribir. Un depósito cuyo cargo por movimiento superaría `saldo + monto` también responde `SAL-001`. En una transferencia las dos cuentas se bloquean siempre en el mismo orden (por código). | Consistencia |
| **RB-04** | `int_movinumero` = `cuenta.int_cuencontmov` anterior + 1. | `cuenta.int_cuencontmov` |
| **RB-05** | **Depósito**: saldo += monto; movimiento tipo `003` (INGRESO); `chr_cuenreferencia` = NULL. | `tipomovimiento` |
| **RB-06** | **Retiro**: saldo −= monto; movimiento tipo `004` (SALIDA); `chr_cuenreferencia` = NULL; exige saldo suficiente. | `tipomovimiento` |
| **RB-07** | **Transferencia**: saldo origen −= monto, saldo destino += monto, **en una sola transacción**. Dos movimientos: `009` (SALIDA) en el origen con `chr_cuenreferencia` = cuenta destino, y `008` (INGRESO) en el destino con `chr_cuenreferencia` = cuenta origen. Cada cuenta usa su propio contador. | `tipomovimiento` |
| **RB-08** | **ITF**: solo en transferencias. `itf = round(monto × valor(parametro '001') / 100, 2)`. Si `itf > 0`, se registra un movimiento `007` (SALIDA) adicional en la cuenta origen y se descuenta del saldo. | `parametro 001` |
| **RB-09** | **Costo por movimiento**: si el número de movimiento N supera `parametro '002'` (15), se cobra `costomovimiento.dec_costimporte` de la moneda de la cuenta y se registra movimiento `010` (SALIDA). El movimiento N es sin costo si N ≤ 15. | `parametro 002` + `costomovimiento` |
| **RB-10** | **Costo de mantenimiento**: se aplica (proyecta) cuando `cuenta.dec_cuensaldo ≤ cargomantenimiento.dec_cargMontoMaximo`; importe = `cargomantenimiento.dec_cargImporte`. | `cargomantenimiento` |
| **RB-11** | **Interés mensual (proyectado)**: `round(saldo × interesmensual.dec_inteimporte / 100, 2)`. | `interesmensual` |
| **RB-12** | El **cobro efectivo** del mantenimiento y el **abono** del interés mensual los ejecuta un **proceso batch externo** fuera del alcance de este servicio; el servicio solo los **proyecta** en `consultarCostosMantenimiento`. | Límite de alcance documentado |
| **RB-13** | La autenticación compara el **hash SHA-256** recibido contra `usuario.vch_emplclave` con una comparación en tiempo constante escrita a mano (`CryptographicOperations.FixedTimeEquals` no existe en .NET Framework 4.8). | Seguridad |
| **RB-14** | El token es un UUID aleatorio, con TTL de **30 minutos**, almacenado en un `ConcurrentDictionary` en memoria. Cada sesión abierta inserta una fila en `logsession`; `cerrarSesion` la cierra con `fec_salida`. | `logsession` |

---

## 8. Casos de uso (11, congelados)

| CU | Nombre | Operación(es) | Códigos de error propios | Modelos |
|---|---|---|---|---|
| **CU-01** | Autenticarse en el sistema | `autenticar`, `cerrarSesion` | DAT-001, AUTH-001, AUTH-002, AUTH-003, AUTH-004 | `CU_01_Autenticarse.oom` / `DA_01_Autenticarse.oom` / `DS_01_Autenticarse.oom` |
| **CU-02** | Validar la sesión del usuario | transversal (ops 3–9) | AUTH-003, AUTH-004 | `CU_02_ValidarSesion.oom` / `DA_02_ValidarSesion.oom` / `DS_02_TokenInvalido.oom` |
| **CU-03** | Consultar las cuentas de un cliente | `listarCuentasPorCliente`, `consultarCuenta` | CLI-001, CTA-001, CTA-002 | `CU_03_ConsultarCuentas.oom` / `DA_03_ConsultarCuentas.oom` / `DS_03_ConsultarCuentas.oom` |
| **CU-04** | Consultar los movimientos de una cuenta | `consultarMovimientos` | CTA-001, CTA-002 | `CU_04_ConsultarMovimientos.oom` / `DA_04_ConsultarMovimientos.oom` / `DS_04_ConsultarMovimientos.oom` |
| **CU-05** | Realizar un depósito | `realizarDeposito` | MON-001, CTA-001, CTA-002, MON-002 | `CU_05_RealizarDeposito.oom` / `DA_05_RealizarDeposito.oom` / `DS_05_RealizarDeposito.oom` |
| **CU-06** | Realizar un retiro | `realizarRetiro` | MON-001, CTA-001, CTA-002, SAL-001, MON-002 | `CU_06_RealizarRetiro.oom` / `DA_06_RealizarRetiro.oom` / `DS_06_RealizarRetiro.oom` |
| **CU-07** | Realizar una transferencia | `realizarTransferencia` | MON-001, CTA-001, CTA-002, CTA-003, CTA-004, SAL-001, MON-002 | `CU_07_RealizarTransferencia.oom` / `DA_07_RealizarTransferencia.oom` / `DS_07_RealizarTransferencia.oom` |
| **CU-08** | Consultar los costos de mantenimiento | `consultarCostosMantenimiento` | CTA-001, CTA-002, MON-002 | `CU_08_ConsultarCostosMantenimiento.oom` / `DA_08_ConsultarCostosMantenimiento.oom` / `DS_08_ConsultarCostosMantenimiento.oom` |
| **CU-09** | Generar la respuesta estructurada | transversal (todas) | — | `CU_09_RespuestaEstructurada.oom` / `DA_09_RespuestaEstructurada.oom` / `DS_09_RespuestaEstructurada.oom` |
| **CU-10** | Gestionar token inválido o expirado | transversal (ops 3–9) | AUTH-003, AUTH-004 | `CU_10_TokenInvalidoExpirado.oom` / `DA_10_TokenInvalidoExpirado.oom` / `DS_10_SesionExpirada.oom` |
| **CU-11** | Gestionar errores de negocio | transversal (todas) | CTA-001..004, CLI-001, SAL-001, MON-001/002, LIM-001, DAT-001, ERR-001 | `CU_11_ErroresNegocio.oom` / `DA_11_ErroresNegocio.oom` / `DS_11_ErrorNegocio.oom` |

> **Los 11 CU tienen ficha propia en el ECUD y modelo propio en las 3 familias de diagramas.**
> Esto replica exactamente el patrón del proyecto `TI.1.1` del docente (11 CU, 11 fichas, 12 `.oom`
> por familia). Los CU transversales (02, 09, 10, 11) se dibujan con `<<include>>` hacia los CU que
> los usan, pero **sí** tienen ficha y modelo propio.
>
> Además: `CU_Sistema.oom` (diagrama general de casos de uso), `DA_00_Sistema_General.oom` y
> `DS_12_Sistema_General.oom`.

---

## 9. Actores (congelados)

| Actor | Código | Tipo | Descripción |
|---|---|---|---|
| Cliente Bancario | `ACT-001` | humano | Persona con cuenta en EURECABANK; opera por el ATM/ventanilla, represented por los clientes de software |
| Cliente de Software Consola | `ACT-002` | sistema | `04. CLICON/EUREKABANK_SOAP_JAVA_CON` — **responsabilidad de Mateo** |
| Cliente de Software Escritorio | `ACT-003` | sistema | `04. CLIESC/` — responsabilidad del compañero 1 |
| Cliente de Software Web | `ACT-004` | sistema | `04. CLIWEB/` — responsabilidad del compañero 2 |
| Cliente de Software Móvil | `ACT-005` | sistema | `04. CLIMOV/` — responsabilidad del compañero 3 |
| Base de Datos EUREKABANK | `ACT-006` | sistema externo | SQL Server con la base `eurekabank` (17 tablas) |
| Proceso Batch de Mantenimiento | `ACT-007` | sistema externo | Proceso que cobra mantenimiento y abona interés (fuera de alcance, ver RB-12) |

---

## 10. Estructura del servidor (espacio de nombres `ec.edu.monster`, congelada)

```
ec.edu.monster
├── controladores
│   ├── IWSEUREKABANK                 ← contrato WCF (9 operaciones, SOAP 1.1 document/literal wrapped)
│   ├── WSEUREKABANK                  ← fachada: SIN lógica de negocio, solo delega; excepción inesperada => ERR-001
│   ├── RaizComposicion               ← ensambla DAOs, servicios y token (Composition Root)
│   ├── DespachoPorElementoCuerpo     ← elige la operación por el elemento raíz del cuerpo (SOAPAction vacío)
│   └── mensajes                      ← Solicitud<Op> / Respuesta<Op>Mensaje (contratos de mensaje, solo propiedades)
├── seguridad
│   ├── TokenServicio, SesionToken    ← registro de tokens (ConcurrentDictionary) + TTL
│   └── ServicioAutenticacion         ← valida usuario/clave contra la tabla `usuario`
├── servicios
│   ├── ServicioCuentas, ServicioMovimientos, ServicioCostos
│   └── FabricaRespuestas             ← único lugar donde se arman los sobres
├── modelos                           ← SOLO propiedades y constructores
│   ├── RespuestaBase, RespuestaAutenticacion, RespuestaCuentas, RespuestaCuenta,
│   │   RespuestaMovimientos, RespuestaMovimiento, RespuestaTransferencia, RespuestaCostos
│   ├── CuentaDTO, MovimientoDTO
│   └── CodigoRespuesta               ← los 17 códigos de §5
├── datos                             ← ADO.NET con System.Data.SqlClient, consultas parametrizadas
│   ├── ProveedorConexion, IUnidadTrabajo / UnidadTrabajoSql, ContextoTransaccional
│   └── ClienteDAO, CuentaDAO, MovimientoDAO, UsuarioDAO, ParametroDAO,
│       TipoMovimientoDAO, MonedaDAO, CostoDAO, LogSessionDAO (+ interfaces I*DAO)
└── utilidades
    ├── Codigos, FormatoDinero, HashClave, Textos
    └── Retorno                       ← cuerpo del contenedor <return> del contrato
```

**Reglas del docente que no se negocian:**
- En `modelos` **solo** propiedades y constructores. Cero lógica.
- La lógica de negocio vive en `servicios`; el acceso a datos en `datos`; la fachada no decide nada.
- Servicio WCF `WSEUREKABANK` (`[ServiceContract(Name="WSEUREKABANK", Namespace="http://ws.monster.edu.ec/")]`), `document/literal wrapped`.
- Cada operación devuelve **siempre** el sobre; nunca lanza excepciones al cliente por regla de negocio.
- Alojamiento: IIS Express, archivo `WSEUREKABANK.svc`, `Web.config`.
- Los elementos hijos de los contenedores de entrada y de `<return>` **no llevan espacio de nombres** (`elementFormDefault="unqualified"`); solo los contenedores `<operacion>` y `<operacionResponse>` lo llevan.

## 11. Configuración (variables de entorno y `appSettings`, congelada)

Cada valor se lee primero de la variable de entorno y, si no existe, de `appSettings` de `Web.config`.

| Variable | Defecto | Uso |
|---|---|---|
| `EUREKABANK_DB_SERVIDOR` | `localhost` | Instancia de SQL Server (MSSQLSERVER) |
| `EUREKABANK_DB_ESQUEMA` | `eurekabank` | Base de datos (las pruebas usan `eurekabank_prueba`) |

La conexión usa autenticación de Windows: no hay usuario ni clave en la configuración.
| `EUREKABANK_TOKEN_MINUTOS` | `30` | TTL del token de sesión |

El filtro CORS de la versión Java no existe en .NET: ningún cliente es un navegador que llame directamente al servicio.

## 12. Semilla de usuarios (congelada) — contraseña = SHA-256 hex de la clave entre paréntesis

| `chr_emplcodigo` | `vch_emplusuario` | `vch_emplclave` (SHA-256) | Clave real | Estado |
|---|---|---|---|---|
| `0001` | `cromero` | `891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4` | `Cromero2026` | ACTIVO |
| `0002` | `lcastro` | `d4b83a0cf9194ca8c9d5cb3f714596cfeb6543df33b618f9b05ef174df87aea4` | `Castro2026` | ACTIVO |
| `0003` | `creyes` | `32b3491336522e073489725b5daf298cd749007a` | *(sin clave conocida — ANULADO)* | ANULADO |
| `0004` | `aramos` | `f0bd8805123716f1d5237930643ae643b43c7f378ff8d387090eacfa73a128ee` | `Ramos2026` | ACTIVO |
| `0005` | `cvalencia` | `c8a50f632c3c4baf27fc05facb1883104e1d16ef` | *(sin clave conocida — ACTIVO)* | ACTIVO |
| `0006` | `rcruz` | `c6a5a412bea19bfa4133c0a15a97c20ddd49dc519330e07d82bdd9db926cc0ac` | `Cruz2026` | ACTIVO |
| `0007` | `ediaz` | `3718e00ac45cec21633e2211af9b77cd0a193698` | *(sin clave conocida — ANULADO)* | ANULADO |
| `0008` | `csarmiento` | `4503389bee2ee5ed2daf0125d825d7e364215989cce0f15804e67ac0fbce9adc` | `Sarmiento2026` | ANULADO |
| `0009` | `lpachas` | `386796f4e78be3c0ee919f1d97805e891fd076a940ba7e09db5601d94797b088` | `Pachas2026` | ACTIVO |
| `0010` | `htello` | `c35eb4169bb3099e24bf0590f0b3a6f3bab1a1d0cee46b182e8fb639e51b71e0` | `Tello2026` | ACTIVO |
| `0011` | `pcarrasco` | `558c296936404a839676a427bc1e63e94db38f5404d489cbe2bde9ab79a51c85` | `Carrasco2026` | ACTIVO |

> Los usuarios `creyes`, `cvalencia`, `ediaz` conservan el hash del *dump* de la guía (SHA-1 de 40
> caracteres). **No son utilizables** para autenticarse porque el sistema exige SHA-256 de 64
> caracteres (RB-13): la comparación falla y se devuelve `AUTH-001`. Se conservan para que la tabla
> tenga el mismo contenido que la guía. `creyes` y `ediaz` además están `ANULADO`, lo que activa
> `AUTH-002` en caso de que se prueben.

---

## 13. Estructura de los clientes (congelada)

Cada cliente tiene las capas `controlador`, `modelo`, `vista`, `util` y `ws`.

```
CLICON (ec.edu.monster.consola)    Program, ws/{IClienteEurekabank, ClienteWSEurekabank},
                                   controlador/{ControladorAutenticacion, ControladorProtegido, ControladorCuentas,
                                                ControladorMovimientos, ControladorCostos},
                                   modelo/{Respuestas, SesionUsuario},
                                   vista/{VistaAutenticacion, VistaMenu, VistaCuentas, VistaMovimientos, VistaCostos},
                                   util/{Consola, Formato, HashClave, DireccionServicio, Registro}
CLIESC (WinForms) y CLIWEB (ASP.NET MVC 5): mismas capas; ver sus README.
```

**Reglas:**
- El proxy WCF se genera con `scripts
egenerar-proxy.ps1` (svcutil, serializador XmlSerializer) desde el WSDL **local** de `04. SERVIDOR\contrato\`; el cliente compila con el servidor apagado. El archivo generado no se edita.
- La dirección del servicio se resuelve: argumento → variable `WS_URL` → `App.config`/`Web.config` (`ws.url`) → `http://localhost:50775/WSEUREKABANK.svc`.
- Timeouts: conexión 5 s, lectura 30 s.
- Un fallo de comunicación se muestra como `SER-001`; el detalle técnico (URL, causa) va solo a un archivo de registro.
- Los clientes no reimplementan reglas de negocio: un monto que no es número muestra el literal de `MON-001` sin llamar al servidor; cualquier monto numérico se envía tal cual.

## 14. Reglas de los diagramas PowerDesigner

| Aspecto | Regla |
|---|---|
| Casos de uso | `01_CASOS_USO/`: **12 `.oom`** = `CU_Sistema.oom` (general, con todos los actores y las relaciones `<<include>>`) + `CU_01_Autenticarse.oom` … `CU_11_ErroresNegocio.oom` (uno por CU). |
| Actividad | `02_ACTIVIDAD/`: **12 `.oom`** = `DA_00_Sistema_General.oom` + `DA_01_Autenticarse.oom` … `DA_11_ErroresNegocio.oom`. |
| Secuencia | `03_SECUENCIA/`: **12 `.oom`** = `DS_01_Autenticarse.oom` … `DS_11_ErrorNegocio.oom` + `DS_12_Sistema_General.oom`. Un modelo por CU con el flujo principal; los CU 02, 09, 10 y 11 muestran el flujo de error. |
| Clases | `04_CLASES/`: **4 `.oom`** = `DC_Servidor.oom`, `DC_Cliente_Consola.oom`, `DC_Cliente_Escritorio.oom` y `DC_Cliente_Web.oom`. Deben ser **verificables contra el código real**: los nombres de clase, atributos y métodos se toman del código C# implementado, no inventados. |
| Arquitectura | `05_ARQUITECTURA/`: **1 `.eam`** = `DA_EUREKABANK_SOAP_DOTNET.eam`: 3 áreas (Consumidores / Servicios con IIS Express y WCF / Datos con SQL Server), 3 aplicaciones cliente (consola, escritorio, web), enlace SOAP y enlace ADO.NET. |
| MER | `02. MER/02. LOGICO/ML_EUREKABANK.ldm` (17 entidades con atributos, PK, FK y cardinalidades) + `02. MER/03. FISICO/MF_EUREKABANK.pdm` (17 tablas con tipos SQL Server, PK, FK, índices y CHECK). El conceptual va en `02. MER/01. CONCEPTUAL/`. |
| Rutas | Todos los `.prj` y el `.sws` con **rutas relativas**. Cero rutas absolutas. Nombre del workspace: `SOAP_DOTNET_CONBDD_GR05.sws` en la raíz del proyecto. |
| Figuras | Cada modelo se exporta a imagen desde PowerDesigner y se incrusta en el ERS, el ECUD y el manual con su pie numerado. |
| Archivos por carpeta | Cada familia de diagramas lleva su `<FAMILIA>.prj` y `<FAMILIA>.pjb` (proyecto PowerDesigner) al que se registran sus modelos. |

### Plantilla de la ficha del ECUD (obligatoria, replicada del `TI.1.1` del docente)

Cada ficha es **una tabla** con estas filas, en este orden:

| Fila | Contenido |
|---|---|
| Nombre Caso de Uso | Nombre del CU |
| Identificador | `CU-01` … `CU-11` |
| Descripción | Párrafo de 3–5 líneas: qué hace, con qué operación SOAP, qué devuelve |
| Meta | Objetivo del CU desde el punto de vista del usuario |
| Estado / Versión | `Activo` / `1.0` |
| Autores | Los nombres del grupo (los del `TI.1.1` son *Eduardo García, Brayan Jacome, Mateo Iza*; **confirma con Mateo los de este grupo** y usa esos si no hay respuesta) |
| Fecha creación / Fecha modificación | `DD/MM/AAAA` (usar `03/11/2026`) |
| Datos de entrada | Lista `parámetro: descripción` |
| Datos de salida | Lista `resultado: descripción` (extensión sobre el `TI.1.1`) |
| Precondiciones | Lista de condiciones que deben cumplirse |
| Post condiciones | `Éxito: ...` / `Fallo: ...` |
| Gráfico | `Ilustración N. Caso de Uso de <nombre>` + la imagen del modelo |
| Flujo básico | Tabla de dos columnas `Actor` / `Sistema` con los pasos numerados 1..n |
| Flujo Alternativo | `FA-01 <condición> (paso n): <código> <mensaje literal>` … (un FA por cada código de error del CU) |
| Casos de uso incluidos | `<<include>>` hacia los CU que usa |
| Casos de uso extendidos | `<<extend>>` o vacío |
| Pruebas de aceptación | 2–4 condiciones verificables (extensión sobre el `TI.1.1`) |

Reglas de la plantilla:
- El **orden de los pasos** del flujo básico debe coincidir con el **orden de validación** de §6.
- Los **mensajes literales** de los FA son los de §5, **idénticos**, sin variantes.
- Cada "Gráfico" lleva su modelo en `01_CASOS_USO` y su imagen incrustada en el propio ECUD.
- El documento lleva **Índice de tablas** e **Índice de ilustraciones** como campos de Word (TOC), con los títulos `Tabla N. ECUD – CU-01: <nombre>` e `Ilustración N. Caso de Uso de <nombre>`.

---

## 15. Trazabilidad de requisitos (semilla para el ERS)

| RF | Nombre | CU | Operación |
|---|---|---|---|
| RF-01 | Autenticar usuario | CU-01 | `autenticar` |
| RF-02 | Cerrar sesión | CU-01 | `cerrarSesion` |
| RF-03 | Validar token de sesión | CU-02 | transversal |
| RF-04 | Listar cuentas por cliente | CU-03 | `listarCuentasPorCliente` |
| RF-05 | Consultar una cuenta | CU-03 | `consultarCuenta` |
| RF-06 | Consultar movimientos | CU-04 | `consultarMovimientos` |
| RF-07 | Realizar depósito | CU-05 | `realizarDeposito` |
| RF-08 | Realizar retiro | CU-06 | `realizarRetiro` |
| RF-09 | Realizar transferencia | CU-07 | `realizarTransferencia` |
| RF-10 | Consultar costos de mantenimiento | CU-08 | `consultarCostosMantenimiento` |
| RF-11 | Emitir respuesta estructurada | CU-09 | transversal |

---

## 16. Entorno de pruebas (congelado)

| Recurso | Valor |
|---|---|
| SQL Server | `localhost`, instancia MSSQLSERVER (servicio de Windows) |
| Esquema real | `eurekabank` (datos de la práctica; las pruebas que escriben NO se ejecutan aquí) |
| Esquema de pruebas | `eurekabank_prueba`, creado con `03. BDD\SQLSERVER\cargar_bdd_sqlserver.ps1 -Esquema eurekabank_prueba -Recrear` |
| Acceso a datos | System.Data.SqlClient (incluido en .NET Framework 4.8) |
| Compilación | MSBuild de Visual Studio, .NET Framework 4.8, `-restore` |
| Pruebas | MSTest 3.6.4, ejecutadas con `dotnet test` |
| Servidor de pruebas | IIS Express en puerto libre (`04. SERVIDOR\scripts\probar-iisexpress.ps1`) |
| PowerDesigner | `C:\Program Files (x86)\Sybase\PowerDesigner 16` (32 bits) |

