using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "listarCuentasPorCliente", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudListarCuentasPorCliente
    {
        [MessageBodyMember(Name = "cedula", Namespace = "", Order = 0)]
        public string Cedula { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 1)]
        public string Token { get; set; }

        public SolicitudListarCuentasPorCliente()
        {
        }

        public SolicitudListarCuentasPorCliente(string cedula, string token)
        {
            Cedula = cedula;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "listarCuentasPorClienteResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaListarCuentasPorClienteMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaListarCuentasPorClienteMensaje()
        {
        }

        public RespuestaListarCuentasPorClienteMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
