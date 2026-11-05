using System.Security.Cryptography;
using System.Text;

namespace ec.edu.monster.escritorio.util
{
    public static class HashClave
    {
        public static string Sha256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                StringBuilder resultado = new StringBuilder(64);
                foreach (byte b in sha.ComputeHash(Encoding.UTF8.GetBytes(texto ?? "")))
                {
                    resultado.Append(b.ToString("x2"));
                }
                return resultado.ToString();
            }
        }
    }
}
