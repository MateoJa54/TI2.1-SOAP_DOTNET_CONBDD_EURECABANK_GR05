using System;
using System.Collections.Generic;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.seguridad;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.servicios
{
    // Operaciones 5 a 8 del contrato. RB-02/RB-03: una transacción por movimiento, cuenta bloqueada con
    // SELECT ... FOR UPDATE; el saldo nunca queda negativo (monto + ITF + cargo por movimiento).
    public class ServicioMovimientos
    {
        public const string TipoDeposito = "003";
        public const string TipoRetiro = "004";
        public const string TipoItf = "007";
        public const string TipoTransferenciaIngreso = "008";
        public const string TipoTransferenciaSalida = "009";
        public const string TipoCostoMovimiento = "010";
        public const string ParametroItf = "001";
        public const string ParametroOperacionesSinCosto = "002";

        private readonly ICuentaDAO cuentaDAO;
        private readonly IMovimientoDAO movimientoDAO;
        private readonly ITipoMovimientoDAO tipoMovimientoDAO;
        private readonly IParametroDAO parametroDAO;
        private readonly ICostoDAO costoDAO;
        private readonly IMonedaDAO monedaDAO;
        private readonly IUnidadTrabajo unidadTrabajo;
        private readonly TokenServicio tokenServicio;
        private readonly Func<DateTime> reloj;

        public ServicioMovimientos(ICuentaDAO cuentaDAO, IMovimientoDAO movimientoDAO,
            ITipoMovimientoDAO tipoMovimientoDAO, IParametroDAO parametroDAO, ICostoDAO costoDAO,
            IMonedaDAO monedaDAO, IUnidadTrabajo unidadTrabajo, TokenServicio tokenServicio)
            : this(cuentaDAO, movimientoDAO, tipoMovimientoDAO, parametroDAO, costoDAO, monedaDAO, unidadTrabajo,
                  tokenServicio, () => DateTime.Now)
        {
        }

        public ServicioMovimientos(ICuentaDAO cuentaDAO, IMovimientoDAO movimientoDAO,
            ITipoMovimientoDAO tipoMovimientoDAO, IParametroDAO parametroDAO, ICostoDAO costoDAO,
            IMonedaDAO monedaDAO, IUnidadTrabajo unidadTrabajo, TokenServicio tokenServicio, Func<DateTime> reloj)
        {
            if (cuentaDAO == null) throw new ArgumentNullException("cuentaDAO");
            if (movimientoDAO == null) throw new ArgumentNullException("movimientoDAO");
            if (tipoMovimientoDAO == null) throw new ArgumentNullException("tipoMovimientoDAO");
            if (parametroDAO == null) throw new ArgumentNullException("parametroDAO");
            if (costoDAO == null) throw new ArgumentNullException("costoDAO");
            if (monedaDAO == null) throw new ArgumentNullException("monedaDAO");
            if (unidadTrabajo == null) throw new ArgumentNullException("unidadTrabajo");
            if (tokenServicio == null) throw new ArgumentNullException("tokenServicio");
            if (reloj == null) throw new ArgumentNullException("reloj");
            this.cuentaDAO = cuentaDAO;
            this.movimientoDAO = movimientoDAO;
            this.tipoMovimientoDAO = tipoMovimientoDAO;
            this.parametroDAO = parametroDAO;
            this.costoDAO = costoDAO;
            this.monedaDAO = monedaDAO;
            this.unidadTrabajo = unidadTrabajo;
            this.tokenServicio = tokenServicio;
            this.reloj = reloj;
        }

        public RespuestaMovimientos ConsultarMovimientos(string cuenta, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.MovimientosFallidos(acceso);
            }
            if (Textos.EstaVacio(cuenta))
            {
                return FabricaRespuestas.MovimientosFallidos(CodigoRespuesta.Dat001);
            }
            string codigo = cuenta.Trim();
            CuentaDTO encontrada = cuentaDAO.BuscarPorCodigo(codigo);
            if (encontrada == null)
            {
                return FabricaRespuestas.MovimientosFallidos(CodigoRespuesta.Cta001);
            }
            if (encontrada.Estado != ServicioCuentas.EstadoActivo)
            {
                return FabricaRespuestas.MovimientosFallidos(CodigoRespuesta.Cta002);
            }
            return FabricaRespuestas.MovimientosExitosos(codigo, movimientoDAO.ListarPorCuenta(codigo));
        }

        public RespuestaMovimiento RealizarDeposito(string cuenta, decimal? monto, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.MovimientoFallido(acceso);
            }
            if (Textos.EstaVacio(cuenta) || !monto.HasValue)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Dat001);
            }
            if (!FormatoDinero.EsMontoValido(monto))
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Mon001);
            }
            string codigoCuenta = cuenta.Trim();
            CuentaDTO encontrada = cuentaDAO.BuscarPorCodigo(codigoCuenta);
            if (encontrada == null)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Cta001);
            }
            if (encontrada.Estado != ServicioCuentas.EstadoActivo)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Cta002);
            }
            decimal importe = FormatoDinero.ADosDecimales(monto.Value);
            decimal cargoPrevio = CostoPorMovimiento(encontrada, encontrada.OperacionesRealizadas + 1);
            if (FormatoDinero.Sumar(encontrada.Saldo, importe) < cargoPrevio)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Sal001);
            }
            if (!monedaDAO.EstaSoportada(encontrada.Moneda))
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Mon002);
            }
            string empleado = EmpleadoDelToken(token);
            ResultadoOperacion resultado = unidadTrabajo.Ejecutar(contexto =>
            {
                CuentaDTO bloqueada = Bloquear(contexto, codigoCuenta);
                if (bloqueada.Estado != ServicioCuentas.EstadoActivo)
                {
                    return ResultadoOperacion.Fallida(CodigoRespuesta.Cta002);
                }
                int numero = bloqueada.OperacionesRealizadas + 1;
                decimal costo = CostoPorMovimiento(bloqueada, numero);
                decimal saldo = FormatoDinero.Sumar(bloqueada.Saldo, importe);
                if (saldo < costo)
                {
                    return ResultadoOperacion.Fallida(CodigoRespuesta.Sal001);
                }
                MovimientoDTO movimiento = CrearMovimiento(numero, TipoDeposito, importe, null, empleado);
                movimientoDAO.Insertar(contexto, codigoCuenta, movimiento);
                int contador = numero;
                if (costo > 0m)
                {
                    contador++;
                    movimientoDAO.Insertar(contexto, codigoCuenta,
                        CrearMovimiento(contador, TipoCostoMovimiento, costo, null, empleado));
                    saldo = FormatoDinero.Restar(saldo, costo);
                }
                cuentaDAO.ActualizarSaldoYContador(contexto, codigoCuenta, saldo, contador);
                return ResultadoOperacion.Exitosa(movimiento, saldo, costo);
            });
            return resultado.EsExitosa
                ? FabricaRespuestas.DepositoExitoso(resultado.Movimiento, resultado.Saldo, resultado.Costo, FormatoDinero.Cero)
                : FabricaRespuestas.MovimientoFallido(resultado.Codigo);
        }

        public RespuestaMovimiento RealizarRetiro(string cuenta, decimal? monto, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.MovimientoFallido(acceso);
            }
            if (Textos.EstaVacio(cuenta) || !monto.HasValue)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Dat001);
            }
            if (!FormatoDinero.EsMontoValido(monto))
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Mon001);
            }
            string codigoCuenta = cuenta.Trim();
            CuentaDTO encontrada = cuentaDAO.BuscarPorCodigo(codigoCuenta);
            if (encontrada == null)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Cta001);
            }
            if (encontrada.Estado != ServicioCuentas.EstadoActivo)
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Cta002);
            }
            decimal importe = FormatoDinero.ADosDecimales(monto.Value);
            decimal cargoPrevio = CostoPorMovimiento(encontrada, encontrada.OperacionesRealizadas + 1);
            if (encontrada.Saldo < FormatoDinero.Sumar(importe, cargoPrevio))
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Sal001);
            }
            if (!monedaDAO.EstaSoportada(encontrada.Moneda))
            {
                return FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Mon002);
            }
            string empleado = EmpleadoDelToken(token);
            ResultadoOperacion resultado = unidadTrabajo.Ejecutar(contexto =>
            {
                CuentaDTO bloqueada = Bloquear(contexto, codigoCuenta);
                if (bloqueada.Estado != ServicioCuentas.EstadoActivo)
                {
                    return ResultadoOperacion.Fallida(CodigoRespuesta.Cta002);
                }
                int numero = bloqueada.OperacionesRealizadas + 1;
                decimal costo = CostoPorMovimiento(bloqueada, numero);
                decimal saldo = bloqueada.Saldo;
                // RB-03: el retiro y su cargo por movimiento no pueden dejar el saldo negativo.
                if (saldo < FormatoDinero.Sumar(importe, costo))
                {
                    return ResultadoOperacion.Fallida(CodigoRespuesta.Sal001);
                }
                saldo = FormatoDinero.Restar(saldo, importe);
                MovimientoDTO movimiento = CrearMovimiento(numero, TipoRetiro, importe, null, empleado);
                movimientoDAO.Insertar(contexto, codigoCuenta, movimiento);
                int contador = numero;
                if (costo > 0m)
                {
                    contador++;
                    movimientoDAO.Insertar(contexto, codigoCuenta,
                        CrearMovimiento(contador, TipoCostoMovimiento, costo, null, empleado));
                    saldo = FormatoDinero.Restar(saldo, costo);
                }
                cuentaDAO.ActualizarSaldoYContador(contexto, codigoCuenta, saldo, contador);
                return ResultadoOperacion.Exitosa(movimiento, saldo, costo);
            });
            return resultado.EsExitosa
                ? FabricaRespuestas.RetiroExitoso(resultado.Movimiento, resultado.Saldo, resultado.Costo, FormatoDinero.Cero)
                : FabricaRespuestas.MovimientoFallido(resultado.Codigo);
        }

        public RespuestaTransferencia RealizarTransferencia(string cuentaOrigen, string cuentaDestino,
            decimal? monto, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.TransferenciaFallida(acceso);
            }
            if (Textos.EstaVacio(cuentaOrigen) || Textos.EstaVacio(cuentaDestino) || !monto.HasValue)
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Dat001);
            }
            if (!FormatoDinero.EsMontoValido(monto))
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Mon001);
            }
            string origen = cuentaOrigen.Trim();
            string destino = cuentaDestino.Trim();
            if (origen == destino)
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Cta003);
            }
            CuentaDTO datoOrigen = cuentaDAO.BuscarPorCodigo(origen);
            CuentaDTO datoDestino = datoOrigen == null ? null : cuentaDAO.BuscarPorCodigo(destino);
            if (datoOrigen == null || datoDestino == null)
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Cta001);
            }
            if (datoOrigen.Estado != ServicioCuentas.EstadoActivo || datoDestino.Estado != ServicioCuentas.EstadoActivo)
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Cta002);
            }
            if (datoOrigen.Moneda != datoDestino.Moneda)
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Cta004);
            }
            decimal importe = FormatoDinero.ADosDecimales(monto.Value);
            decimal itf = CalcularItf(importe);
            decimal cargoPrevio = CostoPorMovimiento(datoOrigen, datoOrigen.OperacionesRealizadas + 1);
            if (datoOrigen.Saldo < FormatoDinero.Sumar(FormatoDinero.Sumar(importe, itf), cargoPrevio))
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Sal001);
            }
            if (!monedaDAO.EstaSoportada(datoOrigen.Moneda) || !monedaDAO.EstaSoportada(datoDestino.Moneda))
            {
                return FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Mon002);
            }
            string empleado = EmpleadoDelToken(token);
            ResultadoTransferencia resultado = unidadTrabajo.Ejecutar(contexto =>
            {
                // Las dos cuentas se bloquean siempre en el mismo orden para que dos transferencias cruzadas no se bloqueen entre sí.
                string primero = string.CompareOrdinal(origen, destino) < 0 ? origen : destino;
                string segundo = primero == origen ? destino : origen;
                CuentaDTO bloqueadaPrimero = Bloquear(contexto, primero);
                CuentaDTO bloqueadaSegundo = Bloquear(contexto, segundo);
                CuentaDTO bloqueadaOrigen = primero == origen ? bloqueadaPrimero : bloqueadaSegundo;
                CuentaDTO bloqueadaDestino = primero == origen ? bloqueadaSegundo : bloqueadaPrimero;
                if (bloqueadaOrigen.Estado != ServicioCuentas.EstadoActivo
                    || bloqueadaDestino.Estado != ServicioCuentas.EstadoActivo)
                {
                    return ResultadoTransferencia.Fallida(CodigoRespuesta.Cta002);
                }
                int numeroOrigen = bloqueadaOrigen.OperacionesRealizadas + 1;
                decimal costoOrigen = CostoPorMovimiento(bloqueadaOrigen, numeroOrigen);
                decimal saldoOrigen = bloqueadaOrigen.Saldo;
                // RB-03: salida, ITF y cargo por movimiento juntos no pueden dejar el saldo negativo.
                decimal total = FormatoDinero.Sumar(FormatoDinero.Sumar(importe, itf), costoOrigen);
                if (saldoOrigen < total)
                {
                    return ResultadoTransferencia.Fallida(CodigoRespuesta.Sal001);
                }
                int numeroDestino = bloqueadaDestino.OperacionesRealizadas + 1;
                decimal costoDestino = CostoPorMovimiento(bloqueadaDestino, numeroDestino);
                decimal saldoDestino = FormatoDinero.Sumar(bloqueadaDestino.Saldo, importe);
                if (saldoDestino < costoDestino)
                {
                    return ResultadoTransferencia.Fallida(CodigoRespuesta.Sal001);
                }
                saldoOrigen = FormatoDinero.Restar(saldoOrigen, importe);
                movimientoDAO.Insertar(contexto, origen,
                    CrearMovimiento(numeroOrigen, TipoTransferenciaSalida, importe, destino, empleado));
                int contadorOrigen = numeroOrigen;
                if (itf > 0m)
                {
                    contadorOrigen++;
                    movimientoDAO.Insertar(contexto, origen,
                        CrearMovimiento(contadorOrigen, TipoItf, itf, destino, empleado));
                    saldoOrigen = FormatoDinero.Restar(saldoOrigen, itf);
                }
                if (costoOrigen > 0m)
                {
                    contadorOrigen++;
                    movimientoDAO.Insertar(contexto, origen,
                        CrearMovimiento(contadorOrigen, TipoCostoMovimiento, costoOrigen, null, empleado));
                    saldoOrigen = FormatoDinero.Restar(saldoOrigen, costoOrigen);
                }
                cuentaDAO.ActualizarSaldoYContador(contexto, origen, saldoOrigen, contadorOrigen);

                movimientoDAO.Insertar(contexto, destino,
                    CrearMovimiento(numeroDestino, TipoTransferenciaIngreso, importe, origen, empleado));
                int contadorDestino = numeroDestino;
                if (costoDestino > 0m)
                {
                    contadorDestino++;
                    movimientoDAO.Insertar(contexto, destino,
                        CrearMovimiento(contadorDestino, TipoCostoMovimiento, costoDestino, null, empleado));
                    saldoDestino = FormatoDinero.Restar(saldoDestino, costoDestino);
                }
                cuentaDAO.ActualizarSaldoYContador(contexto, destino, saldoDestino, contadorDestino);
                return ResultadoTransferencia.Exitosa(numeroOrigen, numeroDestino, saldoOrigen, saldoDestino, itf);
            });
            return resultado.EsExitosa
                ? FabricaRespuestas.TransferenciaExitosa(resultado.NumeroOrigen, resultado.NumeroDestino,
                    resultado.SaldoOrigen, resultado.SaldoDestino, resultado.Itf)
                : FabricaRespuestas.TransferenciaFallida(resultado.Codigo);
        }

        // RB-08: itf = round(monto x parametro 001 / 100, 2).
        public decimal CalcularItf(decimal importe)
        {
            decimal? porcentaje = parametroDAO.Valor(ParametroItf);
            return porcentaje.HasValue ? FormatoDinero.Porcentaje(importe, porcentaje.Value) : FormatoDinero.Cero;
        }

        // RB-09: el movimiento N es sin costo si N no supera el parametro 002.
        public decimal CostoPorMovimiento(CuentaDTO cuenta, int numeroMovimiento)
        {
            decimal? sinCosto = parametroDAO.Valor(ParametroOperacionesSinCosto);
            int operacionesSinCosto = sinCosto.HasValue ? (int)sinCosto.Value : 0;
            if (numeroMovimiento <= operacionesSinCosto)
            {
                return FormatoDinero.Cero;
            }
            decimal? costo = costoDAO.CostoMovimiento(cuenta.Moneda);
            return costo.HasValue ? FormatoDinero.ADosDecimales(costo.Value) : FormatoDinero.Cero;
        }

        private CuentaDTO Bloquear(ContextoTransaccional contexto, string codigo)
        {
            CuentaDTO bloqueada = cuentaDAO.BloquearPorCodigo(contexto, codigo);
            if (bloqueada == null)
            {
                throw new InvalidOperationException("La cuenta " + codigo + " desapareció durante la operación");
            }
            return bloqueada;
        }

        private MovimientoDTO CrearMovimiento(int numero, string tipo, decimal importe, string referencia, string empleado)
        {
            DatosTipoMovimiento tipoMovimiento = tipoMovimientoDAO.BuscarPorCodigo(tipo);
            if (tipoMovimiento == null)
            {
                throw new InvalidOperationException("El tipo de movimiento " + tipo + " no existe en el catálogo");
            }
            return new MovimientoDTO(empleado, reloj().ToString(Codigos.FormatoFecha), FormatoDinero.ADosDecimales(importe),
                numero, referencia, tipoMovimiento.Accion, tipo, tipoMovimiento.Descripcion);
        }

        private string EmpleadoDelToken(string token)
        {
            SesionToken sesion = tokenServicio.ObtenerSesion(token);
            if (sesion == null)
            {
                throw new InvalidOperationException("El token no tiene una sesión asociada");
            }
            return sesion.Empleado;
        }

        private sealed class ResultadoOperacion
        {
            public CodigoRespuesta Codigo { get; private set; }
            public MovimientoDTO Movimiento { get; private set; }
            public decimal Saldo { get; private set; }
            public decimal Costo { get; private set; }
            public bool EsExitosa { get { return Codigo.Exitoso; } }

            public static ResultadoOperacion Fallida(CodigoRespuesta codigo)
            {
                return new ResultadoOperacion { Codigo = codigo, Saldo = FormatoDinero.Cero, Costo = FormatoDinero.Cero };
            }

            public static ResultadoOperacion Exitosa(MovimientoDTO movimiento, decimal saldo, decimal costo)
            {
                return new ResultadoOperacion { Codigo = CodigoRespuesta.Ok200, Movimiento = movimiento, Saldo = saldo, Costo = costo };
            }
        }

        private sealed class ResultadoTransferencia
        {
            public CodigoRespuesta Codigo { get; private set; }
            public int NumeroOrigen { get; private set; }
            public int NumeroDestino { get; private set; }
            public decimal SaldoOrigen { get; private set; }
            public decimal SaldoDestino { get; private set; }
            public decimal Itf { get; private set; }
            public bool EsExitosa { get { return Codigo.Exitoso; } }

            public static ResultadoTransferencia Fallida(CodigoRespuesta codigo)
            {
                return new ResultadoTransferencia { Codigo = codigo, SaldoOrigen = FormatoDinero.Cero, SaldoDestino = FormatoDinero.Cero, Itf = FormatoDinero.Cero };
            }

            public static ResultadoTransferencia Exitosa(int numeroOrigen, int numeroDestino, decimal saldoOrigen,
                decimal saldoDestino, decimal itf)
            {
                return new ResultadoTransferencia
                {
                    Codigo = CodigoRespuesta.Ok200, NumeroOrigen = numeroOrigen, NumeroDestino = numeroDestino,
                    SaldoOrigen = saldoOrigen, SaldoDestino = saldoDestino, Itf = itf
                };
            }
        }
    }
}
