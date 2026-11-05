using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using ec.edu.monster.ws.Referencia;
using Ref = ec.edu.monster.ws.Referencia;

namespace ec.edu.monster.consola.ws
{
    // Adaptador del proxy WCF generado. Un fallo de comunicación se convierte en el sobre SER-001
    // y el detalle técnico (URL, causa) va solo al registro.
    public class ClienteWSEurekabank : IClienteEurekabank
    {
        public const string CodigoServicioNoDisponible = "SER-001";
        public const string MensajeServicioNoDisponible = "El servicio no está disponible en este momento";

        private readonly string url;

        public ClienteWSEurekabank(string url)
        {
            this.url = url;
        }

        public string Url
        {
            get { return url; }
        }

        public RespuestaAutenticacion Autenticar(string usuario, string claveHash)
        {
            return Invocar(c => Convertir(c.autenticar(usuario, claveHash)), Error<RespuestaAutenticacion>);
        }

        public RespuestaBase CerrarSesion(string token)
        {
            return Invocar(c => Base(c.cerrarSesion(token)), Error<RespuestaBase>);
        }

        public RespuestaCuentas ListarCuentasPorCliente(string cedula, string token)
        {
            return Invocar(c => Convertir(c.listarCuentasPorCliente(cedula, token)), Error<RespuestaCuentas>);
        }

        public RespuestaCuenta ConsultarCuenta(string cuenta, string token)
        {
            return Invocar(c => Convertir(c.consultarCuenta(cuenta, token)), Error<RespuestaCuenta>);
        }

        public RespuestaMovimientos ConsultarMovimientos(string cuenta, string token)
        {
            return Invocar(c => Convertir(c.consultarMovimientos(cuenta, token)), Error<RespuestaMovimientos>);
        }

        public RespuestaMovimiento RealizarDeposito(string cuenta, decimal monto, string token)
        {
            return Invocar(c => Convertir(c.realizarDeposito(cuenta, monto, token)), Error<RespuestaMovimiento>);
        }

        public RespuestaMovimiento RealizarRetiro(string cuenta, decimal monto, string token)
        {
            return Invocar(c => Convertir(c.realizarRetiro(cuenta, monto, token)), Error<RespuestaMovimiento>);
        }

        public RespuestaTransferencia RealizarTransferencia(string cuentaOrigen, string cuentaDestino, decimal monto, string token)
        {
            return Invocar(c => Convertir(c.realizarTransferencia(cuentaOrigen, cuentaDestino, monto, token)), Error<RespuestaTransferencia>);
        }

        public RespuestaCostos ConsultarCostosMantenimiento(string cuenta, string token)
        {
            return Invocar(c => Convertir(c.consultarCostosMantenimiento(cuenta, token)), Error<RespuestaCostos>);
        }

        private T Invocar<T>(Func<WSEUREKABANKClient, T> operacion, Func<T> alFallar)
        {
            WSEUREKABANKClient cliente = null;
            try
            {
                cliente = CrearCliente();
                T resultado = operacion(cliente);
                cliente.Close();
                return resultado;
            }
            catch (Exception e) when (e is CommunicationException || e is TimeoutException || e is InvalidOperationException
                || e is UriFormatException || e is System.Xml.XmlException || e is ArgumentException)
            {
                Registro.Error("Fallo al invocar " + url, e);
                if (cliente != null)
                {
                    cliente.Abort();
                }
                return alFallar();
            }
        }

        private WSEUREKABANKClient CrearCliente()
        {
            BasicHttpBinding enlace = new BasicHttpBinding
            {
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(30),
                MaxReceivedMessageSize = 1048576
            };
            return new WSEUREKABANKClient(enlace, new EndpointAddress(url));
        }

        private static T Error<T>() where T : RespuestaBase, new()
        {
            return new T { Estado = "ERROR", Codigo = CodigoServicioNoDisponible, Mensaje = MensajeServicioNoDisponible };
        }

        private static T Rellenar<T>(T destino, Ref.respuestaBase origen) where T : RespuestaBase
        {
            destino.Estado = origen.estado;
            destino.Codigo = origen.codigo;
            destino.Mensaje = origen.mensaje;
            return destino;
        }

        private static RespuestaBase Base(Ref.respuestaBase r)
        {
            return r == null ? Error<RespuestaBase>() : Rellenar(new RespuestaBase(), r);
        }

        private static RespuestaAutenticacion Convertir(Ref.respuestaAutenticacion r)
        {
            if (r == null) return Error<RespuestaAutenticacion>();
            RespuestaAutenticacion m = Rellenar(new RespuestaAutenticacion(), r);
            m.Token = r.token;
            m.Empleado = r.empleado;
            m.NombreEmpleado = r.nombreEmpleado;
            m.Expiracion = r.expiracion;
            m.Rol = r.rol;
            return m;
        }

        private static RespuestaCuentas Convertir(Ref.respuestaCuentas r)
        {
            if (r == null) return Error<RespuestaCuentas>();
            RespuestaCuentas m = Rellenar(new RespuestaCuentas(), r);
            m.Cedula = r.cedula;
            m.NombreCliente = r.nombreCliente;
            m.Cuentas = (r.cuentas ?? new Ref.cuentaDTO[0]).Select(Convertir).ToList();
            return m;
        }

        private static RespuestaCuenta Convertir(Ref.respuestaCuenta r)
        {
            if (r == null) return Error<RespuestaCuenta>();
            RespuestaCuenta m = Rellenar(new RespuestaCuenta(), r);
            m.Cuenta = r.cuenta == null ? null : Convertir(r.cuenta);
            return m;
        }

        private static RespuestaMovimientos Convertir(Ref.respuestaMovimientos r)
        {
            if (r == null) return Error<RespuestaMovimientos>();
            RespuestaMovimientos m = Rellenar(new RespuestaMovimientos(), r);
            m.Cuenta = r.cuenta;
            m.Movimientos = (r.movimientos ?? new Ref.movimientoDTO[0]).Select(Convertir).ToList();
            return m;
        }

        private static RespuestaMovimiento Convertir(Ref.respuestaMovimiento r)
        {
            if (r == null) return Error<RespuestaMovimiento>();
            RespuestaMovimiento m = Rellenar(new RespuestaMovimiento(), r);
            m.Movimiento = r.movimiento == null ? null : Convertir(r.movimiento);
            m.SaldoNuevo = r.saldoNuevo;
            m.CostoAplicado = r.costoAplicado;
            m.ItfAplicado = r.itfAplicado;
            return m;
        }

        private static RespuestaTransferencia Convertir(Ref.respuestaTransferencia r)
        {
            if (r == null) return Error<RespuestaTransferencia>();
            RespuestaTransferencia m = Rellenar(new RespuestaTransferencia(), r);
            m.NumeroOrigen = r.numeroOrigen;
            m.NumeroDestino = r.numeroDestino;
            m.SaldoOrigen = r.saldoOrigen;
            m.SaldoDestino = r.saldoDestino;
            m.ItfAplicado = r.itfAplicado;
            return m;
        }

        private static RespuestaCostos Convertir(Ref.respuestaCostos r)
        {
            if (r == null) return Error<RespuestaCostos>();
            RespuestaCostos m = Rellenar(new RespuestaCostos(), r);
            m.SaldoActual = r.saldoActual;
            m.CostoMantenimiento = r.costoMantenimiento;
            m.CostoMovimiento = r.costoMovimiento;
            m.InteresProyectado = r.interesProyectado;
            m.LimiteMantenimiento = r.limiteMantenimiento;
            m.OperacionesSinCosto = r.operacionesSinCosto;
            return m;
        }

        private static CuentaDTO Convertir(Ref.cuentaDTO c)
        {
            return new CuentaDTO
            {
                Codigo = c.codigo, Moneda = c.moneda, DescripcionMoneda = c.descripcionMoneda, Saldo = c.saldo,
                Estado = c.estado, CodigoCliente = c.codigoCliente, NombreCliente = c.nombreCliente,
                CodigoSucursal = c.codigoSucursal, FechaCreacion = c.fechaCreacion, Clave = c.clave,
                OperacionesRealizadas = c.operacionesRealizadas
            };
        }

        private static MovimientoDTO Convertir(Ref.movimientoDTO m)
        {
            return new MovimientoDTO
            {
                Numero = m.numero, Fecha = m.fecha, TipoCodigo = m.tipoCodigo, TipoDescripcion = m.tipoDescripcion,
                TipoAccion = m.tipoAccion, Importe = m.importe, Referencia = m.referencia, Empleado = m.empleado
            };
        }
    }
}
