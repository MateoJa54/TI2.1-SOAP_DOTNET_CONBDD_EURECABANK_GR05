using System;
using System.Security.Cryptography;
using System.Text;

namespace ec.edu.monster.utilidades
{
    public static class HashClave
    {
        public const int LongitudHash = 64;

        public static string Sha256(string texto)
        {
            if (texto == null)
            {
                throw new ArgumentNullException("texto", "El texto a hashear es obligatorio");
            }
            using (SHA256 sha = SHA256.Create())
            {
                return AHexadecimal(sha.ComputeHash(Encoding.UTF8.GetBytes(texto)));
            }
        }

        public static bool Coincide(string claveRecibida, string hashEsperado)
        {
            if (claveRecibida == null || hashEsperado == null)
            {
                return false;
            }
            string recibido = claveRecibida.Trim();
            string candidato = EsHashSha256(recibido) ? recibido : Sha256(claveRecibida);
            return IgualesEnTiempoConstante(Encoding.UTF8.GetBytes(candidato), Encoding.UTF8.GetBytes(hashEsperado));
        }

        // RB-13: CryptographicOperations.FixedTimeEquals no existe en .NET Framework 4.8.
        public static bool IgualesEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            int diferencia = a.Length ^ b.Length;
            int largo = Math.Max(a.Length, b.Length);
            for (int i = 0; i < largo; i++)
            {
                byte x = i < a.Length ? a[i] : (byte)0;
                byte y = i < b.Length ? b[i] : (byte)0;
                diferencia |= x ^ y;
            }
            return diferencia == 0;
        }

        public static bool EsHashSha256(string texto)
        {
            if (texto == null || texto.Length != LongitudHash)
            {
                return false;
            }
            foreach (char c in texto)
            {
                bool hexadecimal = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!hexadecimal)
                {
                    return false;
                }
            }
            return true;
        }

        public static string AHexadecimal(byte[] bytes)
        {
            StringBuilder texto = new StringBuilder(bytes.Length * 2);
            foreach (byte valor in bytes)
            {
                texto.Append(valor.ToString("x2"));
            }
            return texto.ToString();
        }
    }
}
