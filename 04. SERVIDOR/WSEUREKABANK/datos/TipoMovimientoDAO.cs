using MySqlConnector;

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
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@codigo", codigo);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read()
                        ? new DatosTipoMovimiento(lector.GetString("chr_tipocodigo"),
                            lector.GetString("vch_tipodescripcion"), lector.GetString("vch_tipoaccion"))
                        : null;
                }
            }
        }
    }
}
