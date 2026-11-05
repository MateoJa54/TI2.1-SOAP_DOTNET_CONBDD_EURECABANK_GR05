using System;
using System.Web.Mvc;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.controlador
{
    // Base de los controladores MVC: el cliente es compartido y la sesión de negocio sale de HttpSessionState.
    public abstract class ControladorWeb : Controller
    {
        protected readonly IClienteEurekabank Cliente;

        protected ControladorWeb(IClienteEurekabank cliente)
        {
            if (cliente == null) throw new ArgumentNullException("cliente");
            Cliente = cliente;
        }

        protected SesionUsuario SesionActual()
        {
            return SesionWeb.ObtenerOCrear(Session);
        }

        protected static bool Caducada(RespuestaBase respuesta)
        {
            return respuesta != null
                && (respuesta.Codigo == ControladorProtegido.CodigoTokenInvalido
                    || respuesta.Codigo == ControladorProtegido.CodigoTokenExpirado);
        }

        // AUTH-003 y AUTH-004 invalidan la sesión HTTP y devuelven al acceso con el mensaje del servidor.
        protected ActionResult IrAlLoginPorCaducidad(RespuestaBase respuesta)
        {
            Session.Abandon();
            return RedirectToAction("Login", "Cuenta", new { aviso = Presentacion.Mensaje(respuesta) });
        }
    }
}
