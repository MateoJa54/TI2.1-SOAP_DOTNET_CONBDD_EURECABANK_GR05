using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    public class CostosController : ControladorWeb
    {
        public CostosController() : this(ComposicionWeb.Cliente)
        {
        }

        public CostosController(IClienteEurekabank cliente) : base(cliente)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(new CostosVista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Consultar(string cuenta)
        {
            RespuestaCostos respuesta = new ControladorCostos(Cliente, SesionActual()).Consultar(cuenta);
            if (Caducada(respuesta))
            {
                return IrAlLoginPorCaducidad(respuesta);
            }

            CostosVista vista = new CostosVista { Cuenta = cuenta, Consultado = true };
            if (Presentacion.EsExito(respuesta))
            {
                vista.Datos = respuesta;
            }
            else
            {
                vista.Error = Presentacion.Mensaje(respuesta);
            }
            return View("Index", vista);
        }
    }
}
