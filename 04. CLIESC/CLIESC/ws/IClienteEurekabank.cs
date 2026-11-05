using ec.edu.monster.escritorio.modelo;

namespace ec.edu.monster.escritorio.ws
{
    public interface IClienteEurekabank
    {
        RespuestaAutenticacion Autenticar(string usuario, string claveHash);
        RespuestaBase CerrarSesion(string token);
        RespuestaCuentas ListarCuentasPorCliente(string cedula, string token);
        RespuestaCuenta ConsultarCuenta(string cuenta, string token);
        RespuestaMovimientos ConsultarMovimientos(string cuenta, string token);
        RespuestaMovimiento RealizarDeposito(string cuenta, decimal monto, string token);
        RespuestaMovimiento RealizarRetiro(string cuenta, decimal monto, string token);
        RespuestaTransferencia RealizarTransferencia(string cuentaOrigen, string cuentaDestino, decimal monto, string token);
        RespuestaCostos ConsultarCostosMantenimiento(string cuenta, string token);
    }
}
