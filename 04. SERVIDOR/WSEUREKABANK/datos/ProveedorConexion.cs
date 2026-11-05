using System;
using System.Configuration;
using MySqlConnector;

namespace ec.edu.monster.datos
{
    public class ProveedorConexion
    {
        public const string VariableServidor = "EUREKABANK_DB_SERVIDOR";
        public const string VariablePuerto = "EUREKABANK_DB_PUERTO";
        public const string VariableEsquema = "EUREKABANK_DB_ESQUEMA";
        public const string VariableUsuario = "EUREKABANK_DB_USUARIO";
        public const string VariableClave = "EUREKABANK_DB_CLAVE";

        public const string ServidorPorDefecto = "127.0.0.1";
        public const string PuertoPorDefecto = "3306";
        public const string EsquemaPorDefecto = "eurekabank";
        public const string UsuarioPorDefecto = "eurekabank_app";
        public const string ClavePorDefecto = "EurekaBank2026!";

        private readonly string cadenaConexion;

        public ProveedorConexion()
            : this(Leer(VariableServidor, ServidorPorDefecto), int.Parse(Leer(VariablePuerto, PuertoPorDefecto)),
                  Leer(VariableEsquema, EsquemaPorDefecto), Leer(VariableUsuario, UsuarioPorDefecto),
                  Leer(VariableClave, ClavePorDefecto))
        {
        }

        public ProveedorConexion(string servidor, int puerto, string esquema, string usuario, string clave)
        {
            Servidor = servidor;
            Puerto = puerto;
            Esquema = esquema;
            Usuario = usuario;
            MySqlConnectionStringBuilder constructor = new MySqlConnectionStringBuilder
            {
                Server = servidor,
                Port = (uint)puerto,
                Database = esquema,
                UserID = usuario,
                Password = clave,
                SslMode = MySqlSslMode.None,
                AllowPublicKeyRetrieval = true,
                ConnectionTimeout = 5,
                DefaultCommandTimeout = 15,
                CharacterSet = "utf8mb4",
                MaximumPoolSize = 50
            };
            cadenaConexion = constructor.ConnectionString;
        }

        public string Servidor { get; private set; }
        public int Puerto { get; private set; }
        public string Esquema { get; private set; }
        public string Usuario { get; private set; }

        public MySqlConnection ObtenerConexion()
        {
            MySqlConnection conexion = new MySqlConnection(cadenaConexion);
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
