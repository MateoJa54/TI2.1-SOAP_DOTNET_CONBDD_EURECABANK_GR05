using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "realizarTransferencia", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudRealizarTransferencia
    {
        [MessageBodyMember(Name = "cuentaOrigen", Namespace = "", Order = 0)]
        public string CuentaOrigen { get; set; }

        [MessageBodyMember(Name = "cuentaDestino", Namespace = "", Order = 1)]
        public string CuentaDestino { get; set; }

        [MessageBodyMember(Name = "monto", Namespace = "", Order = 2)]
        public decimal? Monto { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 3)]
        public string Token { get; set; }

        public SolicitudRealizarTransferencia()
        {
        }

        public SolicitudRealizarTransferencia(string cuentaOrigen, string cuentaDestino, decimal? monto, string token)
        {
            CuentaOrigen = cuentaOrigen;
            CuentaDestino = cuentaDestino;
            Monto = monto;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "realizarTransferenciaResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaRealizarTransferenciaMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaRealizarTransferenciaMensaje()
        {
        }

        public RespuestaRealizarTransferenciaMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
