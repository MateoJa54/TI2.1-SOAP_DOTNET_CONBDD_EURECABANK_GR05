using ec.edu.monster.utilidades;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class CostoDAO : ICostoDAO
    {
        private const string ConsultaMantenimiento =
            "SELECT dec_cargMontoMaximo, dec_cargImporte FROM cargomantenimiento WHERE chr_monecodigo = @moneda";

        private const string ConsultaMovimiento =
            "SELECT dec_costimporte FROM costomovimiento WHERE chr_monecodigo = @moneda";

        private const string ConsultaInteres =
            "SELECT dec_inteimporte FROM interesmensual WHERE chr_monecodigo = @moneda";

        private readonly ProveedorConexion proveedor;

        public CostoDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public DatosCostoMantenimiento CostoMantenimiento(string moneda)
        {
            if (Textos.EstaVacio(moneda))
            {
                return null;
            }
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(ConsultaMantenimiento, conexion))
            {
                consulta.Parameters.AddWithValue("@moneda", moneda);
                using (SqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        ? new DatosCostoMantenimiento(lector.GetDecimal(0), lector.GetDecimal(1))
                        : null;
                }
            }
        }

        public decimal? CostoMovimiento(string moneda)
        {
            return LeerDecimal(ConsultaMovimiento, moneda);
        }

        public decimal? InteresMensual(string moneda)
        {
            return LeerDecimal(ConsultaInteres, moneda);
        }

        private decimal? LeerDecimal(string sql, string moneda)
        {
            if (Textos.EstaVacio(moneda))
            {
                return null;
            }
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(sql, conexion))
            {
                consulta.Parameters.AddWithValue("@moneda", moneda);
                object valor = consulta.ExecuteScalar();
                return valor == null || valor is System.DBNull ? (decimal?)null : System.Convert.ToDecimal(valor);
            }
        }
    }
}
