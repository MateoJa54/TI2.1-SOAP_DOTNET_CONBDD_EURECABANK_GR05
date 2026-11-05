using System.Web.Mvc;
using System.Web.Routing;

namespace ec.edu.monster.web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Seccion",
                url: "{controller}",
                defaults: new { action = "Index" });

            routes.MapRoute(
                name: "Predeterminada",
                url: "{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional });

            // Sin defaults de controlador y acción en la ruta general, así la URL del login sale como /Cuenta/Login.
            routes.MapRoute(
                name: "Inicio",
                url: "",
                defaults: new { controller = "Cuenta", action = "Login" });
        }
    }
}
