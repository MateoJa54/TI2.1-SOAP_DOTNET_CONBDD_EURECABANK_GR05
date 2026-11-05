using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    public class OperacionesController : ControladorWeb
    {
        public const string OpDeposito = "deposito";
        public const string OpRetiro = "retiro";
        public const string OpTransferencia = "transferencia";
        public const string CodigoDatos = "DAT-001";
        public const string MensajeDatos = "Los datos de la petición están incompletos o son incorrectos";

        public OperacionesController() : this(ComposicionWeb.Cliente)
        {
        }

        public OperacionesController(IClienteEurekabank cliente) : base(cliente)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new OperacionesVista { Operacion = OpDeposito });
        }

        // Lista cerrada: una operación desconocida no llama al servidor y nunca mueve dinero.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Ejecutar(string operacion, string cuenta, string cuentaDestino, string monto)
        {
            OperacionesVista vista = new OperacionesVista
            {
                Operacion = operacion,
                Cuenta = cuenta,
                CuentaDestino = cuentaDestino,
                Monto = monto
            };
            decimal valor;

            switch (operacion)
            {
                case OpDeposito:
                    if (!Formato.TryLeerMonto(monto, out valor))
                    {
                        return MostrarLocal(vista, Formato.CodigoMontoInvalido, Formato.MensajeMontoInvalido);
                    }
                    return Mostrar(vista, new ControladorMovimientos(Cliente, SesionActual()).Depositar(cuenta, valor));

                case OpRetiro:
                    if (!Formato.TryLeerMonto(monto, out valor))
                    {
                        return MostrarLocal(vista, Formato.CodigoMontoInvalido, Formato.MensajeMontoInvalido);
                    }
                    return Mostrar(vista, new ControladorMovimientos(Cliente, SesionActual()).Retirar(cuenta, valor));

                case OpTransferencia:
                    if (!Formato.TryLeerMonto(monto, out valor))
                    {
                        return MostrarLocal(vista, Formato.CodigoMontoInvalido, Formato.MensajeMontoInvalido);
                    }
                    return Mostrar(vista, new ControladorMovimientos(Cliente, SesionActual()).Transferir(cuenta, cuentaDestino, valor));

                default:
                    return MostrarLocal(vista, CodigoDatos, MensajeDatos);
            }
        }

        private ActionResult Mostrar(OperacionesVista vista, RespuestaBase respuesta)
        {
            if (Caducada(respuesta))
            {
                return IrAlLoginPorCaducidad(respuesta);
            }
            vista.Exito = Presentacion.EsExito(respuesta);
            vista.Mensaje = Presentacion.Mensaje(respuesta);
            if (vista.Exito)
            {
                DatosDe(vista, respuesta);
            }
            return View("Index", vista);
        }

        private static void DatosDe(OperacionesVista vista, RespuestaBase respuesta)
        {
            var movimiento = respuesta as RespuestaMovimiento;
            if (movimiento != null)
            {
                Agregar(vista, "Movimiento número", movimiento.Movimiento == null ? "" : movimiento.Movimiento.Numero.ToString());
                Agregar(vista, "Saldo nuevo", Formato.Monto(movimiento.SaldoNuevo));
                Agregar(vista, "Cargo por movimiento", Formato.Monto(movimiento.CostoAplicado));
                return;
            }
            var transferencia = respuesta as RespuestaTransferencia;
            if (transferencia != null)
            {
                Agregar(vista, "Movimiento de origen", transferencia.NumeroOrigen.ToString());
                Agregar(vista, "Movimiento de destino", transferencia.NumeroDestino.ToString());
                Agregar(vista, "ITF cobrado", Formato.Monto(transferencia.ItfAplicado));
                Agregar(vista, "Saldo de origen", Formato.Monto(transferencia.SaldoOrigen));
                Agregar(vista, "Saldo de destino", Formato.Monto(transferencia.SaldoDestino));
            }
        }

        private static void Agregar(OperacionesVista vista, string etiqueta, string valor)
        {
            vista.Datos.Add(new DatoOperacion { Etiqueta = etiqueta, Valor = valor });
        }

        private ActionResult MostrarLocal(OperacionesVista vista, string codigo, string mensaje)
        {
            vista.Exito = false;
            vista.Mensaje = Formato.Resultado(codigo, mensaje);
            return View("Index", vista);
        }
    }
}
