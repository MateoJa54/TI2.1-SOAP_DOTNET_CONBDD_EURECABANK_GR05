using System;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    // Un movimiento es una sola transacción (RB-02): se confirma completa o se deshace completa.
    public class UnidadTrabajoSql : IUnidadTrabajo
    {
        private readonly ProveedorConexion proveedor;

        public UnidadTrabajoSql(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public T Ejecutar<T>(Func<ContextoTransaccional, T> cuerpo)
        {
            using (SqlConnection conexion = proveedor.ObtenerConexion())
            using (SqlTransaction transaccion = conexion.BeginTransaction())
            {
                try
                {
                    T resultado = cuerpo(new ContextoTransaccional(conexion, transaccion));
                    transaccion.Commit();
                    return resultado;
                }
                catch
                {
                    try
                    {
                        transaccion.Rollback();
                    }
                    catch (Exception)
                    {
                        // la conexión ya se perdió: SQL Server deshace la transacción al cerrarla
                    }
                    throw;
                }
            }
        }
    }
}
