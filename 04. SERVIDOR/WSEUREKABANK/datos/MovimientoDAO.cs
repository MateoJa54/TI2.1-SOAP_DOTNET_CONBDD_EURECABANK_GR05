using System;
using System.Collections.Generic;
using ec.edu.monster.modelos;
using ec.edu.monster.utilidades;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class MovimientoDAO : IMovimientoDAO
    {
        private const string Consulta =
            "SELECT m.int_movinumero, m.dtt_movifecha, m.chr_tipocodigo, t.vch_tipodescripcion, t.vch_tipoaccion, " +
            "m.dec_moviimporte, m.chr_cuenreferencia, m.chr_emplcodigo " +
            "FROM movimiento m JOIN tipomovimiento t ON t.chr_tipocodigo = m.chr_tipocodigo " +
            "WHERE m.chr_cuencodigo = @cuenta ORDER BY m.int_movinumero";

        private const string Insercion =
            "INSERT INTO movimiento (chr_cuencodigo, int_movinumero, dtt_movifecha, chr_emplcodigo, chr_tipocodigo, " +
            "dec_moviimporte, chr_cuenreferencia) VALUES (@cuenta, @numero, @fecha, @empleado, @tipo, @importe, @referencia)";

        private readonly ProveedorConexion proveedor;

        public MovimientoDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public List<MovimientoDTO> ListarPorCuenta(string codigoCuenta)
        {
            List<MovimientoDTO> movimientos = new List<MovimientoDTO>();
            if (Textos.EstaVacio(codigoCuenta))
            {
                return movimientos;
            }
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(Consulta, conexion))
            {
                consulta.Parameters.AddWithValue("@cuenta", codigoCuenta);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        object fecha = lector["dtt_movifecha"];
                        movimientos.Add(new MovimientoDTO(
                            lector.GetString("chr_emplcodigo"),
                            fecha is DBNull ? null : ((DateTime)fecha).ToString(Codigos.FormatoFecha),
                            FormatoDinero.ADosDecimales(lector.GetDecimal("dec_moviimporte")),
                            lector.GetInt32("int_movinumero"),
                            lector.IsDBNull(lector.GetOrdinal("chr_cuenreferencia")) ? null : lector.GetString("chr_cuenreferencia"),
                            lector.GetString("vch_tipoaccion"),
                            lector.GetString("chr_tipocodigo"),
                            lector.GetString("vch_tipodescripcion")));
                    }
                }
            }
            return movimientos;
        }

        public void Insertar(ContextoTransaccional contexto, string codigoCuenta, MovimientoDTO movimiento)
        {
            using (MySqlCommand insercion = new MySqlCommand(Insercion,
                (MySqlConnection)contexto.Conexion, (MySqlTransaction)contexto.Transaccion))
            {
                insercion.Parameters.AddWithValue("@cuenta", codigoCuenta);
                insercion.Parameters.AddWithValue("@numero", movimiento.Numero);
                insercion.Parameters.AddWithValue("@fecha", movimiento.Fecha == null
                    ? (object)DBNull.Value : DateTime.ParseExact(movimiento.Fecha, Codigos.FormatoFecha, null));
                insercion.Parameters.AddWithValue("@empleado", movimiento.Empleado);
                insercion.Parameters.AddWithValue("@tipo", movimiento.TipoCodigo);
                insercion.Parameters.AddWithValue("@importe", FormatoDinero.ADosDecimales(movimiento.Importe));
                insercion.Parameters.AddWithValue("@referencia", (object)movimiento.Referencia ?? DBNull.Value);
                insercion.ExecuteNonQuery();
            }
        }
    }
}
