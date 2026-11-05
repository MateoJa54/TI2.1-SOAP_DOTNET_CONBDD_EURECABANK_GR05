using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "consultarCuenta", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudConsultarCuenta
    {
        [MessageBodyMember(Name = "cuenta", Namespace = "", Order = 0)]
        public string Cuenta { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 1)]
        public string Token { get; set; }

        public SolicitudConsultarCuenta()
        {
        }

        public SolicitudConsultarCuenta(string cuenta, string token)
        {
            Cuenta = cuenta;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "consultarCuentaResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaConsultarCuentaMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaConsultarCuentaMensaje()
        {
        }

        public RespuestaConsultarCuentaMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
