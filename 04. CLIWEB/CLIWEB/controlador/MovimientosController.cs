using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    public class MovimientosController : ControladorWeb
    {
        public MovimientosController() : this(ComposicionWeb.Cliente)
        {
        }

        public MovimientosController(IClienteEurekabank cliente) : base(cliente)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new MovimientosVista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Consultar(string cuenta)
        {
            RespuestaMovimientos respuesta = new ControladorMovimientos(Cliente, SesionActual()).Consultar(cuenta);
            if (Caducada(respuesta))
            {
                return IrAlLoginPorCaducidad(respuesta);
            }

            MovimientosVista vista = new MovimientosVista { Cuenta = cuenta, Consultado = true };
            if (Presentacion.EsExito(respuesta))
            {
                vista.Filas = (respuesta.Movimientos ?? new List<MovimientoDTO>())
                    .Select(m => new FilaMovimiento
                    {
                        Numero = m.Numero.ToString(),
                        Fecha = m.Fecha,
                        Tipo = m.TipoDescripcion ?? m.TipoCodigo,
                        Accion = m.TipoAccion,
                        Importe = Formato.Monto(m.Importe)
                    })
                    .ToList();
            }
            else
            {
                vista.Error = Presentacion.Mensaje(respuesta);
            }
            return View("Index", vista);
        }
    }
}
