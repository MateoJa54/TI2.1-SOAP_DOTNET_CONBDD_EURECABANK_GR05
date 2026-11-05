using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class LogSessionDAO : ILogSessionDAO
    {
        private const string Ingreso =
            "INSERT INTO logsession (chr_emplcodigo, fec_ingreso, ip, hostname) VALUES (@empleado, GETDATE(), @ip, @host); " +
            "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        private const string Salida =
            "UPDATE logsession SET fec_salida = GETDATE() WHERE ID = @id AND fec_salida IS NULL";

        private readonly ProveedorConexion proveedor;

        public LogSessionDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public int? RegistrarIngreso(string empleado, string ip, string hostname)
        {
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand insercion = new SqlCommand(Ingreso, conexion))
            {
                insercion.Parameters.AddWithValue("@empleado", empleado);
                insercion.Parameters.AddWithValue("@ip", ip ?? "NONE");
                insercion.Parameters.AddWithValue("@host", hostname ?? "NONE");
                return (int)insercion.ExecuteScalar();
            }
        }

        public void RegistrarSalida(int? idSesion)
        {
            if (!idSesion.HasValue)
            {
                return;
            }
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand actualizacion = new SqlCommand(Salida, conexion))
            {
                actualizacion.Parameters.AddWithValue("@id", idSesion.Value);
                actualizacion.ExecuteNonQuery();
            }
        }
    }
}
