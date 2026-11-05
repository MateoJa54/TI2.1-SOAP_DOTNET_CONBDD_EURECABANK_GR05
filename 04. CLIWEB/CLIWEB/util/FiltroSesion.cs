using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace ec.edu.monster.web.util
{
    // Filtro global: toda acción exige sesión vigente, salvo el acceso, el error y los recursos estáticos.
    public class FiltroSesion : IAuthorizationFilter
    {
        public static bool EsPublica(string controlador, string accion, string ruta)
        {
            if (!string.IsNullOrEmpty(ruta)
                && (ruta.StartsWith("~/Content/", StringComparison.OrdinalIgnoreCase)
                    || ruta.StartsWith("/Content/", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            if (string.Equals(controlador, "Cuenta", StringComparison.OrdinalIgnoreCase)
                && string.Equals(accion, "Login", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return string.Equals(controlador, "Error", StringComparison.OrdinalIgnoreCase);
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            string controlador = filterContext.RouteData == null ? null : filterContext.RouteData.Values["controller"] as string;
            string accion = filterContext.RouteData == null ? null : filterContext.RouteData.Values["action"] as string;
            string ruta = filterContext.HttpContext.Request.AppRelativeCurrentExecutionFilePath;

            if (EsPublica(controlador, accion, ruta))
            {
                return;
            }
            if (SesionWeb.HaySesion(filterContext.HttpContext.Session))
            {
                return;
            }
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Cuenta", action = "Login" }));
        }
    }
}
