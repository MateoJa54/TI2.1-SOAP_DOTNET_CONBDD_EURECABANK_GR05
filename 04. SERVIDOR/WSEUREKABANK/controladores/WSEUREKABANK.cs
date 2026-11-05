using System;
using System.Diagnostics;
using System.ServiceModel;
using ec.edu.monster.controladores.mensajes;
using ec.edu.monster.modelos;
using ec.edu.monster.seguridad;
using ec.edu.monster.servicios;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores
{
    // Fachada del servicio: solo delega. Ninguna regla de negocio sale como excepción; lo inesperado responde ERR-001.
    [ServiceBehavior(Name = "WSEUREKABANK", Namespace = Codigos.EspacioDeNombres,
        InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WSEUREKABANK : IWSEUREKABANK
    {
        private static readonly TraceSource Registro = CrearRegistro();

        private readonly ServicioAutenticacion autenticacion;
        private readonly ServicioCuentas cuentas;
        private readonly ServicioMovimientos movimientos;
        private readonly ServicioCostos costos;

        public WSEUREKABANK() : this(RaizComposicion.DelSistema())
        {
        }

        private WSEUREKABANK(RaizComposicion raiz)
            : this(raiz.Autenticacion, raiz.Cuentas, raiz.Movimientos, raiz.Costos)
        {
        }

        public WSEUREKABANK(ServicioAutenticacion autenticacion, ServicioCuentas cuentas,
            ServicioMovimientos movimientos, ServicioCostos costos)
        {
            if (autenticacion == null) throw new ArgumentNullException("autenticacion");
            if (cuentas == null) throw new ArgumentNullException("cuentas");
            if (movimientos == null) throw new ArgumentNullException("movimientos");
            if (costos == null) throw new ArgumentNullException("costos");
            this.autenticacion = autenticacion;
            this.cuentas = cuentas;
            this.movimientos = movimientos;
            this.costos = costos;
        }

        public RespuestaAutenticarMensaje Autenticar(SolicitudAutenticar solicitud)
        {
            return new RespuestaAutenticarMensaje(Retorno.De(EjecutarSeguro(
                () => autenticacion.Autenticar(solicitud.Usuario, solicitud.Clave),
                () => FabricaRespuestas.AutenticacionFallida(CodigoRespuesta.Err001))));
        }

        public RespuestaCerrarSesionMensaje CerrarSesion(SolicitudCerrarSesion solicitud)
        {
            return new RespuestaCerrarSesionMensaje(Retorno.De(EjecutarSeguro(
                () => autenticacion.CerrarSesion(solicitud.Token),
                () => FabricaRespuestas.CierreSesionFallido(CodigoRespuesta.Err001))));
        }

        public RespuestaListarCuentasPorClienteMensaje ListarCuentasPorCliente(SolicitudListarCuentasPorCliente solicitud)
        {
            return new RespuestaListarCuentasPorClienteMensaje(Retorno.De(EjecutarSeguro(
                () => cuentas.ListarCuentasPorCliente(solicitud.Cedula, solicitud.Token),
                () => FabricaRespuestas.CuentasFallidas(CodigoRespuesta.Err001))));
        }

        public RespuestaConsultarCuentaMensaje ConsultarCuenta(SolicitudConsultarCuenta solicitud)
        {
            return new RespuestaConsultarCuentaMensaje(Retorno.De(EjecutarSeguro(
                () => cuentas.ConsultarCuenta(solicitud.Cuenta, solicitud.Token),
                () => FabricaRespuestas.CuentaFallida(CodigoRespuesta.Err001))));
        }

        public RespuestaConsultarMovimientosMensaje ConsultarMovimientos(SolicitudConsultarMovimientos solicitud)
        {
            return new RespuestaConsultarMovimientosMensaje(Retorno.De(EjecutarSeguro(
                () => movimientos.ConsultarMovimientos(solicitud.Cuenta, solicitud.Token),
                () => FabricaRespuestas.MovimientosFallidos(CodigoRespuesta.Err001))));
        }

        public RespuestaRealizarDepositoMensaje RealizarDeposito(SolicitudRealizarDeposito solicitud)
        {
            return new RespuestaRealizarDepositoMensaje(Retorno.De(EjecutarSeguro(
                () => movimientos.RealizarDeposito(solicitud.Cuenta, solicitud.Monto, solicitud.Token),
                () => FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Err001))));
        }

        public RespuestaRealizarRetiroMensaje RealizarRetiro(SolicitudRealizarRetiro solicitud)
        {
            return new RespuestaRealizarRetiroMensaje(Retorno.De(EjecutarSeguro(
                () => movimientos.RealizarRetiro(solicitud.Cuenta, solicitud.Monto, solicitud.Token),
                () => FabricaRespuestas.MovimientoFallido(CodigoRespuesta.Err001))));
        }

        public RespuestaRealizarTransferenciaMensaje RealizarTransferencia(SolicitudRealizarTransferencia solicitud)
        {
            return new RespuestaRealizarTransferenciaMensaje(Retorno.De(EjecutarSeguro(
                () => movimientos.RealizarTransferencia(solicitud.CuentaOrigen, solicitud.CuentaDestino, solicitud.Monto, solicitud.Token),
                () => FabricaRespuestas.TransferenciaFallida(CodigoRespuesta.Err001))));
        }

        public RespuestaConsultarCostosMantenimientoMensaje ConsultarCostosMantenimiento(SolicitudConsultarCostosMantenimiento solicitud)
        {
            return new RespuestaConsultarCostosMantenimientoMensaje(Retorno.De(EjecutarSeguro(
                () => costos.ConsultarCostosMantenimiento(solicitud.Cuenta, solicitud.Token),
                () => FabricaRespuestas.CostosFallidos(CodigoRespuesta.Err001))));
        }

        // El detalle de un fallo inesperado va a un archivo del servidor, nunca a la respuesta.
        private static TraceSource CrearRegistro()
        {
            TraceSource fuente = new TraceSource("WSEUREKABANK", SourceLevels.Error);
            fuente.Listeners.Clear();
            fuente.Listeners.Add(new TextWriterTraceListener(
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "eurekabank-servidor.log")));
            Trace.AutoFlush = true;
            return fuente;
        }

        private static R EjecutarSeguro<R>(Func<R> operacion, Func<R> alFallar)
        {
            try
            {
                return operacion();
            }
            catch (Exception e)
            {
                Registro.TraceEvent(TraceEventType.Error, 1, "Error inesperado al procesar la petición: {0}", e);
                return alFallar();
            }
        }
    }
}
