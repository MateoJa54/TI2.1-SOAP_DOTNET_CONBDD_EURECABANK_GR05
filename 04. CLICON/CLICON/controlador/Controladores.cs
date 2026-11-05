using System;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using ec.edu.monster.consola.ws;

namespace ec.edu.monster.consola.controlador
{
    public abstract class ControladorProtegido
    {
        public const string CodigoTokenInvalido = "AUTH-003";
        public const string MensajeTokenInvalido = "El token de sesión no es válido";
        public const string CodigoTokenExpirado = "AUTH-004";

        protected readonly IClienteEurekabank cliente;
        protected readonly SesionUsuario sesion;

        protected ControladorProtegido(IClienteEurekabank cliente, SesionUsuario sesion)
        {
            if (cliente == null) throw new ArgumentNullException("cliente");
            if (sesion == null) throw new ArgumentNullException("sesion");
            this.cliente = cliente;
            this.sesion = sesion;
        }

        public bool HaySesionActiva
        {
            get { return sesion.EstaVigente; }
        }

        // Sin sesión local no se llama al servidor: se responde igual que él lo haría con un token ausente.
        protected R SinSesion<R>() where R : RespuestaBase, new()
        {
            return new R { Estado = "ERROR", Codigo = CodigoTokenInvalido, Mensaje = MensajeTokenInvalido };
        }

        // AUTH-003 y AUTH-004 invalidan la sesión: la vista vuelve al login.
        protected R RevisarSesion<R>(R respuesta) where R : RespuestaBase
        {
            if (respuesta != null && (respuesta.Codigo == CodigoTokenInvalido || respuesta.Codigo == CodigoTokenExpirado))
            {
                sesion.Limpiar();
            }
            return respuesta;
        }
    }

    public class ControladorAutenticacion
    {
        private readonly IClienteEurekabank cliente;
        private readonly SesionUsuario sesion;

        public ControladorAutenticacion(IClienteEurekabank cliente, SesionUsuario sesion)
        {
            if (cliente == null) throw new ArgumentNullException("cliente");
            if (sesion == null) throw new ArgumentNullException("sesion");
            this.cliente = cliente;
            this.sesion = sesion;
        }

        public RespuestaAutenticacion Iniciar(string usuario, string clave)
        {
            RespuestaAutenticacion respuesta = cliente.Autenticar(usuario, HashClave.Sha256(clave));
            if (respuesta != null && respuesta.Codigo == "200" && !string.IsNullOrEmpty(respuesta.Token))
            {
                sesion.Iniciar(respuesta);
            }
            return respuesta;
        }

        public RespuestaBase Cerrar()
        {
            if (!sesion.EstaVigente)
            {
                return new RespuestaBase("OK", "200", "Sesión cerrada correctamente");
            }
            try
            {
                return cliente.CerrarSesion(sesion.Token);
            }
            finally
            {
                sesion.Limpiar();
            }
        }
    }

    public class ControladorCuentas : ControladorProtegido
    {
        public ControladorCuentas(IClienteEurekabank cliente, SesionUsuario sesion) : base(cliente, sesion)
        {
        }

        public RespuestaCuentas ListarPorCliente(string cedula)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.ListarCuentasPorCliente(cedula, sesion.Token))
                : SinSesion<RespuestaCuentas>();
        }

        public RespuestaCuenta Consultar(string cuenta)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.ConsultarCuenta(cuenta, sesion.Token))
                : SinSesion<RespuestaCuenta>();
        }
    }

    public class ControladorMovimientos : ControladorProtegido
    {
        public ControladorMovimientos(IClienteEurekabank cliente, SesionUsuario sesion) : base(cliente, sesion)
        {
        }

        public RespuestaMovimientos Consultar(string cuenta)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.ConsultarMovimientos(cuenta, sesion.Token))
                : SinSesion<RespuestaMovimientos>();
        }

        public RespuestaMovimiento Depositar(string cuenta, decimal monto)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.RealizarDeposito(cuenta, monto, sesion.Token))
                : SinSesion<RespuestaMovimiento>();
        }

        public RespuestaMovimiento Retirar(string cuenta, decimal monto)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.RealizarRetiro(cuenta, monto, sesion.Token))
                : SinSesion<RespuestaMovimiento>();
        }

        public RespuestaTransferencia Transferir(string origen, string destino, decimal monto)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.RealizarTransferencia(origen, destino, monto, sesion.Token))
                : SinSesion<RespuestaTransferencia>();
        }
    }

    public class ControladorCostos : ControladorProtegido
    {
        public ControladorCostos(IClienteEurekabank cliente, SesionUsuario sesion) : base(cliente, sesion)
        {
        }

        public RespuestaCostos Consultar(string cuenta)
        {
            return HaySesionActiva
                ? RevisarSesion(cliente.ConsultarCostosMantenimiento(cuenta, sesion.Token))
                : SinSesion<RespuestaCostos>();
        }
    }
}
