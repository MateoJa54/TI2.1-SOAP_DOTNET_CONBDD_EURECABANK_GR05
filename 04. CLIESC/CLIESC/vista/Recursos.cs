using System.Drawing;
using System.IO;
using System.Reflection;

namespace ec.edu.monster.escritorio.vista
{
    // Imagen incrustada en el ensamblado (recursos\Monster.png).
    public static class Recursos
    {
        public const string Marca = "ec.edu.monster.escritorio.recursos.Monster.png";

        // Devuelve una copia independiente del flujo; si el recurso falta, devuelve null.
        public static Image CargarMarca()
        {
            using (Stream flujo = Assembly.GetExecutingAssembly().GetManifestResourceStream(Marca))
            {
                if (flujo == null)
                {
                    return null;
                }
                using (Image original = Image.FromStream(flujo))
                {
                    return new Bitmap(original);
                }
            }
        }
    }
}
