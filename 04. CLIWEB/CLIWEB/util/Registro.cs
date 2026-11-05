using System;
using System.IO;

namespace ec.edu.monster.web.util
{
    // El detalle técnico de un fallo (URL, causa) va a este archivo y nunca a la pantalla.
    public static class Registro
    {
        public static string Ruta { get; set; } = Path.Combine(Path.GetTempPath(), "eurekabank-cliweb.log");

        public static void Error(string contexto, Exception causa)
        {
            try
            {
                File.AppendAllText(Ruta, DateTime.Now.ToString("s") + " " + contexto + " :: " + causa + Environment.NewLine);
            }
            catch (Exception)
            {
                // sin archivo de registro la aplicación sigue funcionando
            }
        }
    }
}
