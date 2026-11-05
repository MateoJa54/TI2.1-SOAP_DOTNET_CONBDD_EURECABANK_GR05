using ec.edu.monster.utilidades;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class UsuarioDAO : IUsuarioDAO
    {
        private const string Consulta =
            "SELECT u.chr_emplcodigo, u.vch_emplclave, u.vch_emplestado, " +
            "CONCAT(e.vch_emplnombre, ' ', e.vch_emplpaterno, ' ', e.vch_emplmaterno) AS vch_emplnombrecompleto, " +
            "(SELECT TOP 1 m.vch_modunombre FROM permiso p JOIN modulo m ON m.int_moducodigo = p.int_moducodigo " +
            " WHERE p.chr_emplcodigo = u.chr_emplcodigo AND p.vch_permestado = 'ACTIVO' " +
            " ORDER BY m.int_moducodigo) AS vch_modunombre " +
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
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@usuario", nombreUsuario);
                using (SqlDataReader lector = consulta.ExecuteReader())
                {
                    if (!lector.Read())
                    {
                        return null;
                    }
                    int rol = lector.GetOrdinal("vch_modunombre");
                    return new DatosUsuario(
                        lector.GetString(lector.GetOrdinal("chr_emplcodigo")),
                        lector.GetString(lector.GetOrdinal("vch_emplclave")),
                        lector.GetString(lector.GetOrdinal("vch_emplestado")),
                        lector.GetString(lector.GetOrdinal("vch_emplnombrecompleto")),
                        lector.IsDBNull(rol) ? null : lector.GetString(rol));
                }
            }
        }
    }
}
