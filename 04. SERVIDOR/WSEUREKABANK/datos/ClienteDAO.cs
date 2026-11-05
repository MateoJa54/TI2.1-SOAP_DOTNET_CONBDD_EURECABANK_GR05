using ec.edu.monster.utilidades;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class ClienteDAO : IClienteDAO
    {
        private const string Consulta =
            "SELECT c.chr_cliecodigo, CONCAT(c.vch_clienombre, ' ', c.vch_cliepaterno, ' ', c.vch_cliematerno) AS nombre " +
            "FROM cliente c WHERE c.chr_cliedni = @dni";

        private readonly ProveedorConexion proveedor;

        public ClienteDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public DatosCliente BuscarPorDni(string dni)
        {
            if (Textos.EstaVacio(dni))
            {
                return null;
            }
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@dni", dni);
                using (SqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        ? new DatosCliente(lector.GetString(lector.GetOrdinal("chr_cliecodigo")), lector.GetString(lector.GetOrdinal("nombre")))
                        : null;
                }
            }
        }
    }
}
