using System.Collections.Generic;

namespace ec.edu.monster.consola.modelo
{
    public class RespuestaBase
    {
        public RespuestaBase()
        {
        }

        public RespuestaBase(string estado, string codigo, string mensaje)
        {
            Estado = estado;
            Codigo = codigo;
            Mensaje = mensaje;
        }

        public string Estado { get; set; }
        public string Codigo { get; set; }
        public string Mensaje { get; set; }
    }

    public class CuentaDTO
    {
        public string Codigo { get; set; }
        public string Moneda { get; set; }
        public string DescripcionMoneda { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; }
        public string CodigoCliente { get; set; }
        public string NombreCliente { get; set; }
        public string CodigoSucursal { get; set; }
        public string FechaCreacion { get; set; }
        public string Clave { get; set; }
        public int OperacionesRealizadas { get; set; }
    }

    public class MovimientoDTO
    {
        public int Numero { get; set; }
        public string Fecha { get; set; }
        public string TipoCodigo { get; set; }
        public string TipoDescripcion { get; set; }
        public string TipoAccion { get; set; }
        public decimal Importe { get; set; }
        public string Referencia { get; set; }
        public string Empleado { get; set; }
    }

    public class RespuestaAutenticacion : RespuestaBase
    {
        public string Token { get; set; }
        public string Empleado { get; set; }
        public string NombreEmpleado { get; set; }
        public string Expiracion { get; set; }
        public string Rol { get; set; }
    }

    public class RespuestaCuentas : RespuestaBase
    {
        public string Cedula { get; set; }
        public string NombreCliente { get; set; }
        public List<CuentaDTO> Cuentas { get; set; } = new List<CuentaDTO>();
    }

    public class RespuestaCuenta : RespuestaBase
    {
        public CuentaDTO Cuenta { get; set; }
    }

    public class RespuestaMovimientos : RespuestaBase
    {
        public string Cuenta { get; set; }
        public List<MovimientoDTO> Movimientos { get; set; } = new List<MovimientoDTO>();
    }

    public class RespuestaMovimiento : RespuestaBase
    {
        public MovimientoDTO Movimiento { get; set; }
        public decimal SaldoNuevo { get; set; }
        public decimal CostoAplicado { get; set; }
        public decimal ItfAplicado { get; set; }
    }

    public class RespuestaTransferencia : RespuestaBase
    {
        public int NumeroOrigen { get; set; }
        public int NumeroDestino { get; set; }
        public decimal SaldoOrigen { get; set; }
        public decimal SaldoDestino { get; set; }
        public decimal ItfAplicado { get; set; }
    }

    public class RespuestaCostos : RespuestaBase
    {
        public decimal SaldoActual { get; set; }
        public decimal CostoMantenimiento { get; set; }
        public decimal CostoMovimiento { get; set; }
        public decimal InteresProyectado { get; set; }
        public decimal LimiteMantenimiento { get; set; }
        public int OperacionesSinCosto { get; set; }
    }
}
