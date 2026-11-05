using ec.edu.monster.web.modelo;
using ec.edu.monster.web.ws;

namespace ec.edu.monster.web.util
{
    // Traduce la respuesta del servidor a texto de pantalla sin añadir reglas propias.
    public static class Presentacion
    {
        public static bool EsExito(RespuestaBase respuesta)
        {
            return respuesta != null && respuesta.Estado == "OK" && respuesta.Codigo == "200";
        }

        // Éxito: el mensaje del servidor tal cual. Error: "[CODIGO] mensaje".
        public static string Mensaje(RespuestaBase respuesta)
        {
            if (respuesta == null)
            {
                return Formato.Resultado(ClienteWSEurekabank.CodigoServicioNoDisponible, ClienteWSEurekabank.MensajeServicioNoDisponible);
            }
            return EsExito(respuesta) ? Formato.Texto(respuesta.Mensaje) : Formato.Resultado(respuesta.Codigo, respuesta.Mensaje);
        }
    }
}
