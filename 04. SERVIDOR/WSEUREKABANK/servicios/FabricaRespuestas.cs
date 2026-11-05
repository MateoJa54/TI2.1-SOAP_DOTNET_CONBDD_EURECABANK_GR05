using System;
using System.Collections.Generic;
using ec.edu.monster.modelos;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.servicios
{
    // Único lugar donde se arman los sobres: estado, código y mensaje salen siempre del catálogo.
    public static class FabricaRespuestas
    {
        public static RespuestaAutenticacion AutenticacionExitosa(string token, string empleado,
            string nombreEmpleado, string expiracion, string rol)
        {
            return new RespuestaAutenticacion(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeAutenticacionOk,
                empleado, expiracion, nombreEmpleado, rol, token);
        }

        public static RespuestaAutenticacion AutenticacionFallida(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaAutenticacion(codigo.Estado, codigo.Codigo, codigo.Mensaje, null, null, null, null, null);
        }

        public static RespuestaBase CierreSesionExitoso()
        {
            return new RespuestaBase(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeCerrarSesionOk);
        }

        public static RespuestaBase CierreSesionFallido(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaBase(codigo.Estado, codigo.Codigo, codigo.Mensaje);
        }

        public static RespuestaCuentas CuentasExitosas(string cedula, string nombreCliente, List<CuentaDTO> cuentas)
        {
            return new RespuestaCuentas(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeCuentasOk,
                cedula, cuentas, nombreCliente);
        }

        public static RespuestaCuentas CuentasFallidas(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaCuentas(codigo.Estado, codigo.Codigo, codigo.Mensaje, null, new List<CuentaDTO>(), null);
        }

        public static RespuestaCuenta CuentaExitosa(CuentaDTO cuenta)
        {
            return new RespuestaCuenta(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeCuentaOk, cuenta);
        }

        public static RespuestaCuenta CuentaFallida(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaCuenta(codigo.Estado, codigo.Codigo, codigo.Mensaje, null);
        }

        public static RespuestaMovimientos MovimientosExitosos(string cuenta, List<MovimientoDTO> movimientos)
        {
            return new RespuestaMovimientos(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeMovimientosOk,
                cuenta, movimientos);
        }

        public static RespuestaMovimientos MovimientosFallidos(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaMovimientos(codigo.Estado, codigo.Codigo, codigo.Mensaje, null, new List<MovimientoDTO>());
        }

        public static RespuestaMovimiento DepositoExitoso(MovimientoDTO movimiento, decimal saldoNuevo,
            decimal costoAplicado, decimal itfAplicado)
        {
            return Movimiento(Codigos.MensajeDepositoOk, movimiento, saldoNuevo, costoAplicado, itfAplicado);
        }

        public static RespuestaMovimiento RetiroExitoso(MovimientoDTO movimiento, decimal saldoNuevo,
            decimal costoAplicado, decimal itfAplicado)
        {
            return Movimiento(Codigos.MensajeRetiroOk, movimiento, saldoNuevo, costoAplicado, itfAplicado);
        }

        public static RespuestaMovimiento MovimientoFallido(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaMovimiento(codigo.Estado, codigo.Codigo, codigo.Mensaje,
                FormatoDinero.Cero, FormatoDinero.Cero, null, FormatoDinero.Cero);
        }

        public static RespuestaTransferencia TransferenciaExitosa(int numeroOrigen, int numeroDestino,
            decimal saldoOrigen, decimal saldoDestino, decimal itfAplicado)
        {
            return new RespuestaTransferencia(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeTransferenciaOk,
                FormatoDinero.ADosDecimales(itfAplicado), numeroDestino, numeroOrigen,
                FormatoDinero.ADosDecimales(saldoDestino), FormatoDinero.ADosDecimales(saldoOrigen));
        }

        public static RespuestaTransferencia TransferenciaFallida(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaTransferencia(codigo.Estado, codigo.Codigo, codigo.Mensaje,
                FormatoDinero.Cero, 0, 0, FormatoDinero.Cero, FormatoDinero.Cero);
        }

        public static RespuestaCostos CostosExitosos(decimal saldoActual, decimal costoMantenimiento,
            decimal costoMovimiento, decimal interesProyectado, decimal limiteMantenimiento, int operacionesSinCosto)
        {
            return new RespuestaCostos(Codigos.EstadoOk, Codigos.CodigoOk, Codigos.MensajeCostosOk,
                FormatoDinero.ADosDecimales(costoMantenimiento), FormatoDinero.ADosDecimales(costoMovimiento),
                FormatoDinero.ADosDecimales(interesProyectado), FormatoDinero.ADosDecimales(limiteMantenimiento),
                operacionesSinCosto, FormatoDinero.ADosDecimales(saldoActual));
        }

        public static RespuestaCostos CostosFallidos(CodigoRespuesta codigo)
        {
            ExigirError(codigo);
            return new RespuestaCostos(codigo.Estado, codigo.Codigo, codigo.Mensaje,
                FormatoDinero.Cero, FormatoDinero.Cero, FormatoDinero.Cero, FormatoDinero.Cero, 0, FormatoDinero.Cero);
        }

        private static RespuestaMovimiento Movimiento(string mensaje, MovimientoDTO movimiento, decimal saldoNuevo,
            decimal costoAplicado, decimal itfAplicado)
        {
            return new RespuestaMovimiento(Codigos.EstadoOk, Codigos.CodigoOk, mensaje,
                FormatoDinero.ADosDecimales(costoAplicado), FormatoDinero.ADosDecimales(itfAplicado),
                movimiento, FormatoDinero.ADosDecimales(saldoNuevo));
        }

        private static void ExigirError(CodigoRespuesta codigo)
        {
            if (codigo == null)
            {
                throw new ArgumentNullException("codigo", "El código de error es obligatorio");
            }
            if (codigo.Exitoso)
            {
                throw new ArgumentException("El código " + codigo + " no es un código de error", "codigo");
            }
        }
    }
}
