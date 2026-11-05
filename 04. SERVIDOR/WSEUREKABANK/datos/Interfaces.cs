using System.Collections.Generic;
using ec.edu.monster.modelos;

namespace ec.edu.monster.datos
{
    public interface IClienteDAO
    {
        DatosCliente BuscarPorDni(string dni);
    }

    public interface ICuentaDAO
    {
        List<CuentaDTO> ListarPorCliente(string codigoCliente);
        CuentaDTO BuscarPorCodigo(string codigo);
        CuentaDTO BloquearPorCodigo(ContextoTransaccional contexto, string codigo);
        void ActualizarSaldoYContador(ContextoTransaccional contexto, string codigo, decimal saldo, int contador);
    }

    public interface IMovimientoDAO
    {
        List<MovimientoDTO> ListarPorCuenta(string codigoCuenta);
        void Insertar(ContextoTransaccional contexto, string codigoCuenta, MovimientoDTO movimiento);
    }

    public interface IUsuarioDAO
    {
        DatosUsuario BuscarPorNombre(string nombreUsuario);
    }

    public interface ILogSessionDAO
    {
        int? RegistrarIngreso(string empleado, string ip, string hostname);
        void RegistrarSalida(int? idSesion);
    }

    public interface IParametroDAO
    {
        decimal? Valor(string codigo);
    }

    public interface ITipoMovimientoDAO
    {
        DatosTipoMovimiento BuscarPorCodigo(string codigo);
    }

    public interface IMonedaDAO
    {
        bool EstaSoportada(string codigoMoneda);
    }

    public interface ICostoDAO
    {
        DatosCostoMantenimiento CostoMantenimiento(string moneda);
        decimal? CostoMovimiento(string moneda);
        decimal? InteresMensual(string moneda);
    }
}
