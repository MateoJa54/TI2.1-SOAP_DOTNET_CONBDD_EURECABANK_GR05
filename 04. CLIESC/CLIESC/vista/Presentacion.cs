using System;
using System.Collections.Generic;
using System.Globalization;
using ec.edu.monster.escritorio.controlador;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.ws;

namespace ec.edu.monster.escritorio.vista
{
    // Convierte respuestas y datos del servidor en texto para la pantalla. No decide reglas de negocio:
    // lo que el servidor responde se muestra como llega, con el formato [CODIGO] mensaje.
    public static class Presentacion
    {
        public const string Consultando = "Consultando...";
        public const string SinResultados = "Sin resultados";
        public const string CodigoExito = "200";
        public const string SesionCerrada = "Sesión cerrada correctamente";

        public static bool EsExitosa(RespuestaBase respuesta)
        {
            return respuesta != null && respuesta.Codigo == CodigoExito;
        }

        public static bool EsSesionInvalida(RespuestaBase respuesta)
        {
            return respuesta != null
                && (respuesta.Codigo == ControladorProtegido.CodigoTokenInvalido
                    || respuesta.Codigo == ControladorProtegido.CodigoTokenExpirado);
        }

        // Respuesta nula: el servicio no respondió. Igual que el resto de errores de conexión, sin detalle técnico.
        public static string Mensaje(RespuestaBase respuesta)
        {
            if (respuesta == null)
            {
                return Formato.Resultado(ClienteWSEurekabank.CodigoServicioNoDisponible,
                    ClienteWSEurekabank.MensajeServicioNoDisponible);
            }
            return Formato.Resultado(respuesta.Codigo, respuesta.Mensaje);
        }

        // Excepción no prevista: el detalle va al registro y a la pantalla solo el mensaje de servicio no disponible.
        public static string FalloInesperado(Exception causa, string contexto)
        {
            Registro.Error(contexto, causa);
            return Formato.Resultado(ClienteWSEurekabank.CodigoServicioNoDisponible,
                ClienteWSEurekabank.MensajeServicioNoDisponible);
        }

        // El monto se valida solo como número: cero, negativos y más decimales los decide el servidor.
        public static bool TryMonto(string texto, out decimal monto, out string error)
        {
            if (Formato.TryLeerMonto(texto, out monto))
            {
                error = null;
                return true;
            }
            error = Formato.Resultado(Formato.CodigoMontoInvalido, Formato.MensajeMontoInvalido);
            return false;
        }

        public static string[] FilaCuenta(CuentaDTO cuenta)
        {
            return new[]
            {
                Formato.Texto(cuenta.Codigo),
                Formato.Texto(cuenta.Moneda),
                Formato.Monto(cuenta.Saldo),
                Formato.Texto(cuenta.Estado)
            };
        }

        public static string[] FilaMovimiento(MovimientoDTO movimiento)
        {
            return new[]
            {
                movimiento.Numero.ToString(CultureInfo.InvariantCulture),
                Formato.Texto(movimiento.Fecha),
                Tipo(movimiento),
                Formato.Texto(movimiento.TipoAccion),
                Formato.Monto(movimiento.Importe)
            };
        }

        public static string Tipo(MovimientoDTO movimiento)
        {
            return string.IsNullOrEmpty(movimiento.TipoDescripcion)
                ? Formato.Texto(movimiento.TipoCodigo)
                : movimiento.TipoDescripcion;
        }

        public static List<KeyValuePair<string, string>> DetalleCuenta(CuentaDTO cuenta)
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Código", Formato.Texto(cuenta.Codigo)),
                new KeyValuePair<string, string>("Moneda", Formato.Texto(cuenta.Moneda)),
                new KeyValuePair<string, string>("Saldo", Formato.Monto(cuenta.Saldo)),
                new KeyValuePair<string, string>("Estado", Formato.Texto(cuenta.Estado)),
                new KeyValuePair<string, string>("Titular", Formato.Texto(cuenta.NombreCliente)),
                new KeyValuePair<string, string>("Sucursal", Formato.Texto(cuenta.CodigoSucursal)),
                new KeyValuePair<string, string>("Creada", Formato.Texto(cuenta.FechaCreacion))
            };
        }

        public static List<KeyValuePair<string, string>> Costos(RespuestaCostos costos)
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Saldo actual", Formato.Monto(costos.SaldoActual)),
                new KeyValuePair<string, string>("Costo de mantenimiento", Formato.Monto(costos.CostoMantenimiento)),
                new KeyValuePair<string, string>("Límite de mantenimiento", Formato.Monto(costos.LimiteMantenimiento)),
                new KeyValuePair<string, string>("Costo por movimiento", Formato.Monto(costos.CostoMovimiento)),
                new KeyValuePair<string, string>("Operaciones sin costo", costos.OperacionesSinCosto.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("Interés proyectado", Formato.Monto(costos.InteresProyectado))
            };
        }
    }
}
