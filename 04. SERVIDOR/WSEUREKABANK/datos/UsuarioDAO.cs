using ec.edu.monster.utilidades;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class UsuarioDAO : IUsuarioDAO
    {
        private const string Consulta =
            "SELECT u.chr_emplcodigo, u.vch_emplclave, u.vch_emplestado, " +
            "CONCAT(e.vch_emplnombre, ' ', e.vch_emplpaterno, ' ', e.vch_emplmaterno) AS vch_emplnombrecompleto, " +
            "(SELECT m.vch_modunombre FROM permiso p JOIN modulo m ON m.int_moducodigo = p.int_moducodigo " +
            " WHERE p.chr_emplcodigo = u.chr_emplcodigo AND p.vch_permestado = 'ACTIVO' " +
            " ORDER BY m.int_moducodigo LIMIT 1) AS vch_modunombre " +
            "FROM usuario u JOIN empleado e ON e.chr_emplcodigo = u.chr_emplcodigo " +
            "WHERE u.vch_emplusuario = @usuario";

        private readonly ProveedorConexion proveedor;

        public UsuarioDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public DatosUsuario BuscarPorNombre(string nombreUsuario)
        {
            if (Textos.EstaVacio(nombreUsuario))
            {
                return null;
            }
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@usuario", nombreUsuario);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    if (!lector.Read())
                    {
                        return null;
                    }
                    int rol = lector.GetOrdinal("vch_modunombre");
                    return new DatosUsuario(
                        lector.GetString("chr_emplcodigo"),
                        lector.GetString("vch_emplclave"),
                        lector.GetString("vch_emplestado"),
                        lector.GetString("vch_emplnombrecompleto"),
                        lector.IsDBNull(rol) ? null : lector.GetString(rol));
                }
            }
        }
    }
}
