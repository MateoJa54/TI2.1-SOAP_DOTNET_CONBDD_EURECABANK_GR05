using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    public class CuentasController : ControladorWeb
    {
        public CuentasController() : this(ComposicionWeb.Cliente)
        {
        }

        public CuentasController(IClienteEurekabank cliente) : base(cliente)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new CuentasVista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Listar(string cedula)
        {
            RespuestaCuentas respuesta = new ControladorCuentas(Cliente, SesionActual()).ListarPorCliente(cedula);
            if (Caducada(respuesta))
            {
                return IrAlLoginPorCaducidad(respuesta);
            }

            CuentasVista vista = new CuentasVista { Cedula = cedula, Consultado = true };
            if (Presentacion.EsExito(respuesta))
            {
                vista.NombreCliente = respuesta.NombreCliente;
                vista.Cuentas = (respuesta.Cuentas ?? new List<CuentaDTO>())
                    .Select(c => new CuentaFila
                    {
                        Codigo = c.Codigo,
                        Moneda = c.DescripcionMoneda ?? c.Moneda,
                        Saldo = Formato.Monto(c.Saldo),
                        Estado = c.Estado
                    })
                    .ToList();
            }
            else
            {
                vista.Error = Presentacion.Mensaje(respuesta);
            }
            return View("Index", vista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Consultar(string cuenta)
        {
            RespuestaCuenta respuesta = new ControladorCuentas(Cliente, SesionActual()).Consultar(cuenta);
            if (Caducada(respuesta))
            {
                return IrAlLoginPorCaducidad(respuesta);
            }

            ConsultaCuentaVista vista = new ConsultaCuentaVista();
            if (Presentacion.EsExito(respuesta) && respuesta.Cuenta != null)
            {
                CuentaDTO c = respuesta.Cuenta;
                vista.Detalle = new CuentaDetalle
                {
                    Codigo = c.Codigo,
                    Moneda = c.DescripcionMoneda ?? c.Moneda,
                    Saldo = Formato.Monto(c.Saldo),
                    Estado = c.Estado,
                    NombreCliente = c.NombreCliente,
                    CodigoSucursal = c.CodigoSucursal,
                    FechaCreacion = c.FechaCreacion,
                    OperacionesRealizadas = c.OperacionesRealizadas
                };
            }
            else
            {
                vista.Error = Presentacion.Mensaje(respuesta);
            }
            return View("Consultar", vista);
        }
    }
}
