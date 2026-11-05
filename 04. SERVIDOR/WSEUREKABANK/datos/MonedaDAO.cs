using ec.edu.monster.utilidades;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class MonedaDAO : IMonedaDAO
    {
        // Una moneda está soportada si tiene cargo de mantenimiento, costo por movimiento e interés definidos.
        private const string Consulta =
            "SELECT (SELECT COUNT(*) FROM moneda WHERE chr_monecodigo = @moneda), " +
            "(SELECT COUNT(*) FROM cargomantenimiento WHERE chr_monecodigo = @moneda), " +
            "(SELECT COUNT(*) FROM costomovimiento WHERE chr_monecodigo = @moneda), " +
            "(SELECT COUNT(*) FROM interesmensual WHERE chr_monecodigo = @moneda)";

        private readonly ProveedorConexion proveedor;

        public MonedaDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public bool EstaSoportada(string codigoMoneda)
        {
            if (Textos.EstaVacio(codigoMoneda))
            {
                return false;
            }
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@moneda", codigoMoneda);
                using (SqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        && lector.GetInt32(0) > 0 && lector.GetInt32(1) > 0
                        && lector.GetInt32(2) > 0 && lector.GetInt32(3) > 0;
                }
            }
        }
    }
}
