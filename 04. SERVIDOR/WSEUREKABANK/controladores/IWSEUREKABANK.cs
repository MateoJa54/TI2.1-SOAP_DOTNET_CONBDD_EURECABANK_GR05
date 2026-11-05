using System.ServiceModel;
using ec.edu.monster.controladores.mensajes;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.controladores
{
    [ServiceContract(Name = "WSEUREKABANK", Namespace = Codigos.EspacioDeNombres)]
    [XmlSerializerFormat(Style = OperationFormatStyle.Document, Use = OperationFormatUse.Literal)]
    [DespachoPorElementoCuerpo]
    public interface IWSEUREKABANK
    {
        [OperationContract(Name = "autenticar")]
        RespuestaAutenticarMensaje Autenticar(SolicitudAutenticar solicitud);

        [OperationContract(Name = "cerrarSesion")]
        RespuestaCerrarSesionMensaje CerrarSesion(SolicitudCerrarSesion solicitud);

        [OperationContract(Name = "listarCuentasPorCliente")]
        RespuestaListarCuentasPorClienteMensaje ListarCuentasPorCliente(SolicitudListarCuentasPorCliente solicitud);

        [OperationContract(Name = "consultarCuenta")]
        RespuestaConsultarCuentaMensaje ConsultarCuenta(SolicitudConsultarCuenta solicitud);

        [OperationContract(Name = "consultarMovimientos")]
        RespuestaConsultarMovimientosMensaje ConsultarMovimientos(SolicitudConsultarMovimientos solicitud);

        [OperationContract(Name = "realizarDeposito")]
        RespuestaRealizarDepositoMensaje RealizarDeposito(SolicitudRealizarDeposito solicitud);

        [OperationContract(Name = "realizarRetiro")]
        RespuestaRealizarRetiroMensaje RealizarRetiro(SolicitudRealizarRetiro solicitud);

        [OperationContract(Name = "realizarTransferencia")]
        RespuestaRealizarTransferenciaMensaje RealizarTransferencia(SolicitudRealizarTransferencia solicitud);

        [OperationContract(Name = "consultarCostosMantenimiento")]
        RespuestaConsultarCostosMantenimientoMensaje ConsultarCostosMantenimiento(SolicitudConsultarCostosMantenimiento solicitud);
    }
}
