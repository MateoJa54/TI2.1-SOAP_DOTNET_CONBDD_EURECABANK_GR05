using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    public class TipoMovimientoDAO : ITipoMovimientoDAO
    {
        private const string Consulta =
            "SELECT chr_tipocodigo, vch_tipodescripcion, vch_tipoaccion FROM tipomovimiento WHERE chr_tipocodigo = @codigo";

        private readonly ProveedorConexion proveedor;

        public TipoMovimientoDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public DatosTipoMovimiento BuscarPorCodigo(string codigo)
        {
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlCommand consulta = new SqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@codigo", codigo);
                using (SqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        ? new DatosTipoMovimiento(lector.GetString(lector.GetOrdinal("chr_tipocodigo")),
                            lector.GetString(lector.GetOrdinal("vch_tipodescripcion")), lector.GetString(lector.GetOrdinal("vch_tipoaccion")))
                        : null;
                }
            }
        }
    }
}
