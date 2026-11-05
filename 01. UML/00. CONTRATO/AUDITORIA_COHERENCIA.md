# Auditoría de coherencia, versión SOAP en .NET

Fecha de la revisión: 2026-11-04. Contrato de referencia: `CONTRATO_CONGELADO_EUREKABANK.md` v1.0 adaptado a .NET.

## Qué se comprobó y cómo

| Punto | Resultado | Evidencia |
|---|---|---|
| 9 operaciones del contrato | Coinciden | `FachadaTest.LasNueveOperacionesDelContratoExisten`; recorrido real con `probar-iisexpress.ps1` |
| 17 códigos con su mensaje literal, con tildes | Coinciden | `UtilidadesTest` compara los 17 mensajes con el catálogo |
| Mensajes de éxito por operación | Coinciden | `UtilidadesTest.MensajesDeExito_SonLosDelContrato` |
| Orden de validación (sección 6) | Cumplido | Pruebas de orden en `MovimientosTest` (por ejemplo `Orden_Mon001GanaSobreCta001`, `Orden_Sal001GanaSobreMon002`) |
| RB-01, importes con `decimal`, escala 2 | Cumplido | `FormatoDinero`; la prueba de escala detectó y corrigió un defecto de `decimal.Round` en .NET Framework |
| RB-02 y RB-03, transacción única y saldo nunca negativo | Cumplido | Pruebas de rollback y de saldo exacto; 15 pruebas de integración contra `eurekabank_prueba` |
| RB-13, clave en tiempo constante | Cumplido | `HashClave.IgualesEnTiempoConstante` (escrita a mano) |
| Forma de las respuestas contra el XSD | Cumplida | `FachadaTest` valida cada respuesta contra `04. SERVIDOR\contrato\WSEUREKABANK_schema.xsd` |
| Compatibilidad con el cliente Java | Cumplida | El cliente de consola Java funcionó contra el servidor .NET cambiando solo la URL |
| Clientes sin reglas de negocio | Cumplido | Revisión del código y pruebas de monto (`abc`, `0`, `-5`, `10.555`) en los tres clientes |

## Cambios respecto del contrato de la versión Java

1. Endpoint: `http://localhost:50775/WSEUREKABANK.svc` (IIS Express y WCF) en lugar de Payara.
2. Estructura del servidor y de los clientes, configuración por variables `EUREKABANK_DB_SERVIDOR`, `EUREKABANK_DB_PUERTO` y `EUREKABANK_DB_ESQUEMA` en lugar de la URL JDBC. En la versión .NET la base es SQL Server (no MySQL) y no hay usuario ni clave de aplicación. Se elimina `EUREKABANK_ORIGENES_CORS`.
3. RB-03 ampliada: el retiro exige `saldo >= monto + cargo por movimiento` y la transferencia `saldo >= monto + ITF + cargo por movimiento`, con `SAL-001` sin escribir. Un depósito cuyo cargo superaría `saldo + monto` también responde `SAL-001`; esta última extensión no existe en Java.
4. Un fallo de base de datos responde `ERR-001`, igual que en Java; `SER-001` solo lo genera el cliente cuando no alcanza al servicio.

## Desfases conocidos entre código, diagramas y documentos

1. **ERS y ECUD**: las 15 imágenes del ERS y las 11 del ECUD siguen siendo las exportadas de la versión Java. No se exportaron los diagramas a PNG por decisión del autor.
2. **ERS**: el conteo de pruebas del servidor se actualizó a 356 (341 sin base y 15 de integración). El ERS no menciona las 294 de la consola, las 402 del escritorio ni las 330 de la web, y el ECUD no tiene conteos.
3. **ERS**: algunas frases técnicas reescritas de forma automática no están revisadas (por ejemplo sobre la serialización y las clases del modelo). El glosario conserva el orden alfabético anterior.
4. **Diagramas de clases**: el diagrama del servidor no dibuja los miembros no públicos ni los constructores; `ref Message` de `SelectOperation` aparece como `Message` porque el generador omite el modificador.
5. **Diagrama de paquetes del servidor**: solo muestra las dependencias directas; las implicadas por otras se omiten. Existe una dependencia circular real entre `servicios` y `seguridad`.
6. **Diagramas de casos de uso, actividad y secuencia**: se copiaron de la versión Java. En actividad se sustituyó "Payara" por "IIS Express". No se ha verificado que ningún otro texto mencione tecnología de Java.
7. **Manual técnico**: no se ha redactado.
8. **Web**: el acceso no regenera el identificador de la sesión HTTP (riesgo de fijación de sesión, documentado en `04. CLIWEB\README.md`).
9. **Base de datos**: el servidor .NET usa SQL Server con los mismos 17 objetos y la misma semilla que la versión Java en MySQL; los datos cargados coinciden (6 cuentas, 74 movimientos).

## No verificado

- Los `.docx` no se abrieron en ONLYOFFICE tras la adaptación; solo se comprobó con python-docx que conservan las mismas tablas e imágenes.
- Las pestañas de movimientos, operación y costos del cliente de escritorio no se vieron en pantalla.
- El repositorio no se ha probado en otra PC; sí se comprobó que ningún archivo de texto contiene rutas absolutas ni secretos fuera de la clave de laboratorio del contrato.
