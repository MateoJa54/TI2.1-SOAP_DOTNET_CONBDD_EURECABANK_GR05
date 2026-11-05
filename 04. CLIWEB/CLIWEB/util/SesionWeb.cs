using System.Web;
using ec.edu.monster.web.modelo;

namespace ec.edu.monster.web.util
{
    // La sesión de negocio vive en HttpSessionState, una instancia por sesión HTTP.
    public static class SesionWeb
    {
        public const string Clave = "sesion";

        public static SesionUsuario Obtener(HttpSessionStateBase sesionHttp)
        {
            return sesionHttp == null ? null : sesionHttp[Clave] as SesionUsuario;
        }

        public static SesionUsuario ObtenerOCrear(HttpSessionStateBase sesionHttp)
        {
            SesionUsuario sesion = Obtener(sesionHttp);
            if (sesion == null)
            {
                sesion = new SesionUsuario();
                sesionHttp[Clave] = sesion;
            }
            return sesion;
        }

        public static bool HaySesion(HttpSessionStateBase sesionHttp)
        {
            SesionUsuario sesion = Obtener(sesionHttp);
            return sesion != null && sesion.EstaVigente;
        }
    }
}
