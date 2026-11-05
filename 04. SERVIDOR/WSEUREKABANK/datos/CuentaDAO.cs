using System;
using System.Collections.Generic;
using ec.edu.monster.modelos;
using ec.edu.monster.utilidades;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class CuentaDAO : ICuentaDAO
    {
        private const string SelectBase =
            "SELECT c.chr_cuencodigo, c.chr_monecodigo, m.vch_monedescripcion, c.dec_cuensaldo, " +
            "c.vch_cuenestado, c.chr_cliecodigo, " +
            "CONCAT(cl.vch_clienombre, ' ', cl.vch_cliepaterno, ' ', cl.vch_cliematerno) AS vch_clienombrecompleto, " +
            "c.chr_sucucodigo, c.dtt_cuenfechacreacion, c.chr_cuenclave, c.int_cuencontmov " +
            "FROM cuenta c " +
            "JOIN cliente cl ON cl.chr_cliecodigo = c.chr_cliecodigo " +
            "JOIN moneda m ON m.chr_monecodigo = c.chr_monecodigo ";

        private const string Actualizacion =
            "UPDATE cuenta SET dec_cuensaldo = @saldo, int_cuencontmov = @contador WHERE chr_cuencodigo = @codigo";

        private readonly ProveedorConexion proveedor;

        public CuentaDAO(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public List<CuentaDTO> ListarPorCliente(string codigoCliente)
        {
            List<CuentaDTO> cuentas = new List<CuentaDTO>();
            if (Textos.EstaVacio(codigoCliente))
            {
                return cuentas;
            }
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(SelectBase + "WHERE c.chr_cliecodigo = @cliente ORDER BY c.chr_cuencodigo", conexion))
            {
                consulta.Parameters.AddWithValue("@cliente", codigoCliente);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    while (lector.Read())
                    {
                        cuentas.Add(Leer(lector));
                    }
                }
            }
            return cuentas;
        }

        public CuentaDTO BuscarPorCodigo(string codigo)
        {
            if (Textos.EstaVacio(codigo))
            {
                return null;
            }
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlCommand consulta = new MySqlCommand(SelectBase + "WHERE c.chr_cuencodigo = @codigo", conexion))
            {
                consulta.Parameters.AddWithValue("@codigo", codigo);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read() ? Leer(lector) : null;
                }
            }
        }

        public CuentaDTO BloquearPorCodigo(ContextoTransaccional contexto, string codigo)
        {
            if (Textos.EstaVacio(codigo))
            {
                return null;
            }
            using (MySqlCommand consulta = new MySqlCommand(SelectBase + "WHERE c.chr_cuencodigo = @codigo FOR UPDATE",
                (MySqlConnection)contexto.Conexion, (MySqlTransaction)contexto.Transaccion))
            {
                consulta.Parameters.AddWithValue("@codigo", codigo);
                using (MySqlDataReader lector = consulta.ExecuteReader())
                {
                    return lector.Read() ? Leer(lector) : null;
                }
            }
        }

        public void ActualizarSaldoYContador(ContextoTransaccional contexto, string codigo, decimal saldo, int contador)
        {
            using (MySqlCommand actualizacion = new MySqlCommand(Actualizacion,
                (MySqlConnection)contexto.Conexion, (MySqlTransaction)contexto.Transaccion))
            {
                actualizacion.Parameters.AddWithValue("@saldo", FormatoDinero.ADosDecimales(saldo));
                actualizacion.Parameters.AddWithValue("@contador", contador);
                actualizacion.Parameters.AddWithValue("@codigo", codigo);
                if (actualizacion.ExecuteNonQuery() != 1)
                {
                    throw new InvalidOperationException("La cuenta " + codigo + " no existe y no se pudo actualizar");
                }
            }
        }

        private static CuentaDTO Leer(MySqlDataReader lector)
        {
            object fecha = lector["dtt_cuenfechacreacion"];
            return new CuentaDTO(
                clave: lector.IsDBNull(lector.GetOrdinal("chr_cuenclave")) ? null : lector.GetString("chr_cuenclave"),
                codigo: lector.GetString("chr_cuencodigo"),
                codigoCliente: lector.GetString("chr_cliecodigo"),
                codigoSucursal: lector.GetString("chr_sucucodigo"),
                descripcionMoneda: lector.GetString("vch_monedescripcion"),
                estado: lector.GetString("vch_cuenestado"),
                fechaCreacion: fecha is DBNull ? null : ((DateTime)fecha).ToString(Codigos.FormatoFecha),
                moneda: lector.GetString("chr_monecodigo"),
                nombreCliente: lector.GetString("vch_clienombrecompleto"),
                operacionesRealizadas: lector.GetInt32("int_cuencontmov"),
                saldo: FormatoDinero.ADosDecimales(lector.GetDecimal("dec_cuensaldo")));
        }
    }
}
