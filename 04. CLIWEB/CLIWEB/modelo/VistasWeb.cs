using System.Collections.Generic;

namespace ec.edu.monster.web.modelo
{
    public class LoginVista
    {
        public string Usuario { get; set; }
        public string Error { get; set; }
        public string Aviso { get; set; }
    }

    public class CuentaFila
    {
        public string Codigo { get; set; }
        public string Moneda { get; set; }
        public string Saldo { get; set; }
        public string Estado { get; set; }
    }

    public class CuentasVista
    {
        public CuentasVista()
        {
            Cuentas = new List<CuentaFila>();
        }

        public string Cedula { get; set; }
        public bool Consultado { get; set; }
        public string Error { get; set; }
        public string NombreCliente { get; set; }
        public List<CuentaFila> Cuentas { get; set; }
    }

    public class CuentaDetalle
    {
        public string Codigo { get; set; }
        public string Moneda { get; set; }
        public string Saldo { get; set; }
        public string Estado { get; set; }
        public string NombreCliente { get; set; }
        public string CodigoSucursal { get; set; }
        public string FechaCreacion { get; set; }
        public int OperacionesRealizadas { get; set; }
    }

    public class ConsultaCuentaVista
    {
        public string Error { get; set; }
        public CuentaDetalle Detalle { get; set; }
    }

    public class FilaMovimiento
    {
        public string Numero { get; set; }
        public string Fecha { get; set; }
        public string Tipo { get; set; }
        public string Accion { get; set; }
        public string Importe { get; set; }

        // Las cinco celdas de la tabla, en el orden de sus columnas.
        public string[] Celdas()
        {
            return new[] { Numero, Fecha, Tipo, Accion, Importe };
        }
    }

    public class MovimientosVista
    {
        public MovimientosVista()
        {
            Filas = new List<FilaMovimiento>();
        }

        public string Cuenta { get; set; }
        public bool Consultado { get; set; }
        public string Error { get; set; }
        public List<FilaMovimiento> Filas { get; set; }
    }

    public class OperacionesVista
    {
        public string Operacion { get; set; }
        public string Cuenta { get; set; }
        public string CuentaDestino { get; set; }
        public string Monto { get; set; }
        public string Mensaje { get; set; }
        public bool Exito { get; set; }
    }

    public class CostosVista
    {
        public string Cuenta { get; set; }
        public bool Consultado { get; set; }
        public string Error { get; set; }
        public RespuestaCostos Datos { get; set; }
    }
}
