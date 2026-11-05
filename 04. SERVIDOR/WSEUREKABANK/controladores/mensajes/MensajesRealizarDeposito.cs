using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "realizarDeposito", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudRealizarDeposito
    {
        [MessageBodyMember(Name = "cuenta", Namespace = "", Order = 0)]
        public string Cuenta { get; set; }

        [MessageBodyMember(Name = "monto", Namespace = "", Order = 1)]
        public decimal? Monto { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 2)]
        public string Token { get; set; }

        public SolicitudRealizarDeposito()
        {
        }

        public SolicitudRealizarDeposito(string cuenta, decimal? monto, string token)
        {
            Cuenta = cuenta;
            Monto = monto;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "realizarDepositoResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaRealizarDepositoMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaRealizarDepositoMensaje()
        {
        }

        public RespuestaRealizarDepositoMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
