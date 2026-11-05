using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class LogSessionDAO : ILogSessionDAO
    {
        private const string Ingreso =
            "INSERT INTO logsession (chr_emplcodigo, fec_ingreso, ip, hostname) VALUES (@empleado, NOW(), @ip, @host)";

        private const string Salida =
            "UPDATE logsession SET fec_salida = NOW() WHERE ID = @id AND fec_salida IS NULL";

        private readonly ProveedorConexion proveedor;

        public LogSessionDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public int? RegistrarIngreso(string empleado, string ip, string hostname)
        {
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand insercion = new MySqlCommand(Ingreso, conexion))
            {
                insercion.Parameters.AddWithValue("@empleado", empleado);
                insercion.Parameters.AddWithValue("@ip", ip ?? "NONE");
                insercion.Parameters.AddWithValue("@host", hostname ?? "NONE");
                insercion.ExecuteNonQuery();
                return (int)insercion.LastInsertedId;
            }
        }

        public void RegistrarSalida(int? idSesion)
        {
            if (!idSesion.HasValue)
            {
                return;
            }
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand actualizacion = new MySqlCommand(Salida, conexion))
            {
                actualizacion.Parameters.AddWithValue("@id", idSesion.Value);
                actualizacion.ExecuteNonQuery();
            }
        }
    }
}
