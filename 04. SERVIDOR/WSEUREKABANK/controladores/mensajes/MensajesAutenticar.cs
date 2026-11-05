using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "autenticar", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudAutenticar
    {
        [MessageBodyMember(Name = "usuario", Namespace = "", Order = 0)]
        public string Usuario { get; set; }

        [MessageBodyMember(Name = "clave", Namespace = "", Order = 1)]
        public string Clave { get; set; }

        public SolicitudAutenticar()
        {
        }

        public SolicitudAutenticar(string usuario, string clave)
        {
            Usuario = usuario;
            Clave = clave;
        }
    }

    [MessageContract(WrapperName = "autenticarResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaAutenticarMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaAutenticarMensaje()
        {
        }

        public RespuestaAutenticarMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
