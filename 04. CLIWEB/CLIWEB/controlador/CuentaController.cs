using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    public class CuentaController : ControladorWeb
    {
        public CuentaController() : this(ComposicionWeb.Cliente)
        {
        }

        public CuentaController(IClienteEurekabank cliente) : base(cliente)
        {
        }

        [HttpGet]
        public ActionResult Login(string aviso)
        {
            if (SesionWeb.HaySesion(Session))
            {
                return RedirectToAction("Index", "Cuentas");
            }
            return View(new LoginVista { Aviso = aviso });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string usuario, string clave)
        {
            // El login anterior se invalida en el servidor y su estado sale de la sesión HTTP antes de autenticar.
            SesionUsuario anterior = SesionWeb.Obtener(Session);
            if (anterior != null && anterior.EstaVigente)
            {
                Cliente.CerrarSesion(anterior.Token);
            }
            Session.Clear();

            SesionUsuario nueva = new SesionUsuario();
            RespuestaAutenticacion respuesta = new ControladorAutenticacion(Cliente, nueva).Iniciar(usuario, clave);
            if (Presentacion.EsExito(respuesta) && nueva.EstaVigente)
            {
                Session[SesionWeb.Clave] = nueva;
                return RedirectToAction("Index", "Cuentas");
            }
            return View("Login", new LoginVista { Usuario = usuario, Error = Presentacion.Mensaje(respuesta) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            SesionUsuario sesion = SesionWeb.Obtener(Session) ?? new SesionUsuario();
            RespuestaBase respuesta = new ControladorAutenticacion(Cliente, sesion).Cerrar();
            Session.Abandon();
            return RedirectToAction("Login", new { aviso = Presentacion.Mensaje(respuesta) });
        }
    }
}
