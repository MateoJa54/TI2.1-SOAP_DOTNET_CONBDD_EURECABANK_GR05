using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "realizarRetiro", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudRealizarRetiro
    {
        [MessageBodyMember(Name = "cuenta", Namespace = "", Order = 0)]
        public string Cuenta { get; set; }

        [MessageBodyMember(Name = "monto", Namespace = "", Order = 1)]
        public decimal? Monto { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 2)]
        public string Token { get; set; }

        public SolicitudRealizarRetiro()
        {
        }

        public SolicitudRealizarRetiro(string cuenta, decimal? monto, string token)
        {
            Cuenta = cuenta;
            Monto = monto;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "realizarRetiroResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaRealizarRetiroMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaRealizarRetiroMensaje()
        {
        }

        public RespuestaRealizarRetiroMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
