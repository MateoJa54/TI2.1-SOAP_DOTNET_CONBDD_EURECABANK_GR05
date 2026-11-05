using ec.edu.monster.utilidades;
using MySqlConnector;

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
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@dni", dni);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        ? new DatosCliente(lector.GetString("chr_cliecodigo"), lector.GetString("nombre"))
                        : null;
                }
            }
        }
    }
}
