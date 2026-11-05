namespace ec.edu.monster.utilidades
{
    public static class Textos
    {
        public static bool EstaVacio(string texto)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return true;
            }
            foreach (char c in texto)
            {
                if (!char.IsWhiteSpace(c) && c != ' ')
                {
                    return false;
                }
            }
            return true;
        }

        public static string Recortar(string texto)
        {
            return texto == null ? null : texto.Trim();
        }
    }
}
