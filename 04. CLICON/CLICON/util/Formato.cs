using System;
using System.Globalization;

namespace ec.edu.monster.consola.util
{
    public static class Formato
    {
        public const string CodigoMontoInvalido = "MON-001";
        public const string MensajeMontoInvalido = "El monto debe ser mayor que cero y con máximo dos decimales";

        public static string Monto(decimal valor)
        {
            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture);
        }

        public static string Texto(string valor)
        {
            return valor ?? "";
        }

        public static string Resultado(string codigo, string mensaje)
        {
            return "[" + Texto(codigo) + "] " + Texto(mensaje);
        }

        // Lo único que el cliente comprueba es que el texto sea un número; cero, negativos y más de dos
        // decimales se envían tal cual y los decide el servidor.
        public static bool TryLeerMonto(string texto, out decimal monto)
        {
            monto = 0m;
            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }
            string limpio = texto.Trim();
            if (limpio.IndexOf('.') < 0)
            {
                limpio = limpio.Replace(',', '.');
            }
            return decimal.TryParse(limpio, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out monto);
        }
    }
}
