using System;
using System.Configuration;

namespace ec.edu.monster.escritorio.util
{
    public static class DireccionServicio
    {
        public const string VariableEntorno = "WS_URL";
        public const string ClaveConfiguracion = "ws.url";
        public const string PorDefecto = "http://localhost:50775/WSEUREKABANK.svc";

        // Argumento, variable WS_URL, App.config y valor por defecto, en ese orden.
        public static string Resolver(string argumento)
        {
            if (!string.IsNullOrWhiteSpace(argumento))
            {
                return argumento.Trim();
            }
            string variable = Environment.GetEnvironmentVariable(VariableEntorno);
            if (!string.IsNullOrWhiteSpace(variable))
            {
                return variable.Trim();
            }
            string configurada = ConfigurationManager.AppSettings[ClaveConfiguracion];
            return string.IsNullOrWhiteSpace(configurada) ? PorDefecto : configurada.Trim();
        }
    }
}
