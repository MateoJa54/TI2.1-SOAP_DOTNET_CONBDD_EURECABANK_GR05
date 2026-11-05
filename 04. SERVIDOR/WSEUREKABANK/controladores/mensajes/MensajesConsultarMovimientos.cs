using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "consultarMovimientos", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudConsultarMovimientos
    {
        [MessageBodyMember(Name = "cuenta", Namespace = "", Order = 0)]
        public string Cuenta { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 1)]
        public string Token { get; set; }

        public SolicitudConsultarMovimientos()
        {
        }

        public SolicitudConsultarMovimientos(string cuenta, string token)
        {
            Cuenta = cuenta;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "consultarMovimientosResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaConsultarMovimientosMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaConsultarMovimientosMensaje()
        {
        }

        public RespuestaConsultarMovimientosMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
