using System.Globalization;
using ec.edu.monster.utilidades;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class ParametroDAO : IParametroDAO
    {
        private const string Consulta =
            "SELECT vch_paravalor FROM parametro WHERE chr_paracodigo = @codigo AND vch_paraestado = 'ACTIVO'";

        private readonly ProveedorConexion proveedor;

        public ParametroDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public decimal? Valor(string codigo)
        {
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@codigo", codigo);
                object valor = consulta.ExecuteScalar();
                string texto = valor as string;
                decimal numero;
                if (Textos.EstaVacio(texto)
                    || !decimal.TryParse(texto.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out numero))
                {
                    return null;
                }
                return numero;
            }
        }
    }
}
