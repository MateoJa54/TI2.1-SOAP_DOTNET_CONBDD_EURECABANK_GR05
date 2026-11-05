using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ec.edu.monster.datos
{
    // SQL Server (MSSQLSERVER) con autenticación de Windows: el servidor no guarda ninguna clave.
    public class ProveedorConexion
    {
        public const string VariableServidor = "EUREKABANK_DB_SERVIDOR";
        public const string VariableEsquema = "EUREKABANK_DB_ESQUEMA";

        public const string ServidorPorDefecto = "localhost";
        public const string EsquemaPorDefecto = "eurekabank";

        private readonly string cadenaConexion;

        public ProveedorConexion()
            : this(Leer(VariableServidor, ServidorPorDefecto), Leer(VariableEsquema, EsquemaPorDefecto))
        {
        }

        public ProveedorConexion(string servidor, string esquema)
        {
            Servidor = servidor;
            Esquema = esquema;
            SqlConnectionStringBuilder constructor = new SqlConnectionStringBuilder
            {
                DataSource = servidor,
                InitialCatalog = esquema,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 5,
                MaxPoolSize = 50
            };
            cadenaConexion = constructor.ConnectionString;
        }

        public string Servidor { get; private set; }
        public string Esquema { get; private set; }

        public SqlConnection ObtenerConexion()
        {
            SqlConnection conexion = new SqlConnection(cadenaConexion);
            conexion.Open();
            return conexion;
        }

        public static string Leer(string nombre, string porDefecto)
        {
            string valor = Environment.GetEnvironmentVariable(nombre);
            if (valor != null)
            {
                return valor.Trim();
            }
            valor = ConfigurationManager.AppSettings[nombre];
            if (string.IsNullOrWhiteSpace(valor))
            {
                valor = porDefecto;
            }
            return valor == null ? null : valor.Trim();
        }
    }
}
