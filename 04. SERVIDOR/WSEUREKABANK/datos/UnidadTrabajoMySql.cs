using System;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class UnidadTrabajoMySql : IUnidadTrabajo
    {
        private readonly ProveedorConexion proveedor;

        public UnidadTrabajoMySql(ProveedorConexion proveedor)
        {
            this.proveedor = proveedor;
        }

        public T Ejecutar<T>(Func<ContextoTransaccional, T> cuerpo)
        {
            using (MySqlConnection conexion = proveedor.ObtenerConexion())
            using (MySqlTransaction transaccion = conexion.BeginTransaction())
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
                        // la conexión ya se perdió: el servidor deshace la transacción al cerrarla
                    }
                    throw;
                }
            }
        }
    }
}
