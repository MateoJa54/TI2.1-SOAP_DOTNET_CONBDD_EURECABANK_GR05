using System.ServiceModel;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores.mensajes
{
    [MessageContract(WrapperName = "consultarCostosMantenimiento", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class SolicitudConsultarCostosMantenimiento
    {
        [MessageBodyMember(Name = "cuenta", Namespace = "", Order = 0)]
        public string Cuenta { get; set; }

        [MessageBodyMember(Name = "token", Namespace = "", Order = 1)]
        public string Token { get; set; }

        public SolicitudConsultarCostosMantenimiento()
        {
        }

        public SolicitudConsultarCostosMantenimiento(string cuenta, string token)
        {
            Cuenta = cuenta;
            Token = token;
        }
    }

    [MessageContract(WrapperName = "consultarCostosMantenimientoResponse", WrapperNamespace = ec.edu.monster.utilidades.Codigos.EspacioDeNombres, IsWrapped = true)]
    public class RespuestaConsultarCostosMantenimientoMensaje
    {
        [MessageBodyMember(Name = "return", Namespace = "", Order = 0)]
        public Retorno Resultado { get; set; }

        public RespuestaConsultarCostosMantenimientoMensaje()
        {
        }

        public RespuestaConsultarCostosMantenimientoMensaje(Retorno resultado)
        {
            Resultado = resultado;
        }
    }
}
