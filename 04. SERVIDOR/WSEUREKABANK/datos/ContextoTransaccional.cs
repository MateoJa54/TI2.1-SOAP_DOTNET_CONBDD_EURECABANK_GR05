using System.Data.Common;

namespace ec.edu.monster.datos
{
    public class ContextoTransaccional
    {
        public ContextoTransaccional(DbConnection conexion, DbTransaction transaccion)
        {
            Conexion = conexion;
            Transaccion = transaccion;
        }

        public DbConnection Conexion { get; private set; }
        public DbTransaction Transaccion { get; private set; }
    }
}
