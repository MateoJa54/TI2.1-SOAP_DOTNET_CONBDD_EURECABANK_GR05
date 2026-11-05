using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "cerrarSesion", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudCerrarSesion
    {
        [MessageBodyMember(Name = "token", Namespace = "", Order = 0)]
        public string Token { get; set; }

        public SolicitudCerrarSesion()
        {
        }

        public SolicitudCerrarSesion(string token)
        {
            Token = token;
        }
    }

    [MessageContract(WrapperName = "cerrarSesionResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaCerrarSesionMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaCerrarSesionMensaje()
        {
        }

        public RespuestaCerrarSesionMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
