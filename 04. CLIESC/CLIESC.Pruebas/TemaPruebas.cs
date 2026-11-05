using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ec.edu.monster.escritorio.vista;
using ec.edu.monster.escritorio.vista.controles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    [TestClass]
    public class TemaPruebas
    {
        private static Color ColorDe(string nombre)
        {
            return (Color)typeof(Tema).GetField(nombre, BindingFlags.Public | BindingFlags.Static).GetValue(null);
        }

        [TestMethod]
        [DataRow("Fondo", 0xF3F7FB)]
        [DataRow("Superficie", 0xFFFFFF)]
        [DataRow("Entrada", 0xF7F9FC)]
        [DataRow("Borde", 0xE4EAF2)]
        [DataRow("Navy", 0x133049)]
        [DataRow("Texto", 0x1F3142)]
        [DataRow("TextoSecundario", 0x54677A)]
        [DataRow("TextoTenue", 0x8B9AAB)]
        [DataRow("Primario", 0x2563EB)]
        [DataRow("PrimarioOscuro", 0x1769E0)]
        [DataRow("PrimarioSuave", 0xE7EFFE)]
        [DataRow("Marca", 0x0F8591)]
        [DataRow("Error", 0xB8313A)]
        [DataRow("ErrorSuave", 0xFBEBEC)]
        public void Paleta_CoincideConElHexadecimalPedido(string nombre, int rgb)
        {
            Assert.AreEqual(0xFF000000 | (uint)rgb, (uint)ColorDe(nombre).ToArgb());
        }

        [TestMethod]
        public void Radios_SonDoceYDieciseis()
        {
            Assert.AreEqual(12, Tema.Radio);
            Assert.AreEqual(16, Tema.RadioTarjeta);
        }

        [TestMethod]
        public void Fuente_EsSegoeUIA975()
        {
            Assert.AreEqual("Segoe UI", Tema.Familia);
            Assert.AreEqual(9.75f, Tema.TamanoBase);
            Assert.AreEqual(9.75f, Tema.Base.Size);
            Assert.AreEqual("Segoe UI", Tema.Base.FontFamily.Name);
        }

        [TestMethod]
        [DataRow(10f)]
        [DataRow(12f)]
        [DataRow(20f)]
        public void Fuente_CreaFuentesDelTamanoPedido(float tamano)
        {
            using (Font fuente = Tema.Fuente(tamano, FontStyle.Bold))
            {
                Assert.AreEqual(tamano, fuente.Size);
                Assert.IsTrue(fuente.Bold);
            }
        }

        [TestMethod]
        public void EtiquetaCampo_VaEnMayusculasYColorSecundario()
        {
            Label etiqueta = Tema.EtiquetaCampo("cédula");
            Assert.AreEqual("CÉDULA", etiqueta.Text);
            Assert.AreEqual(Tema.TextoSecundario, etiqueta.ForeColor);
        }

        [TestMethod]
        public void RutaRedondeada_UsaElAreaPedida()
        {
            using (var ruta = Tema.RutaRedondeada(new Rectangle(0, 0, 100, 40), 12))
            {
                RectangleF limites = ruta.GetBounds();
                Assert.AreEqual(100f, limites.Width, 0.5f);
                Assert.AreEqual(40f, limites.Height, 0.5f);
            }
        }

        [TestMethod]
        public void RutaRedondeada_RadioCeroEsRectangulo()
        {
            using (var ruta = Tema.RutaRedondeada(new Rectangle(0, 0, 30, 20), 0))
            {
                Assert.IsTrue(ruta.PointCount >= 4);
            }
        }

        [TestMethod]
        public void BotonPrimario_TieneColoresDelPrimario()
        {
            BotonRedondeado boton = Tema.BotonPrimario("Ingresar");
            Assert.AreEqual("Ingresar", boton.Text);
            Assert.AreEqual(Tema.Primario, boton.ColorFondo);
            Assert.AreEqual(Tema.PrimarioOscuro, boton.ColorFondoHover);
            Assert.AreEqual(Color.White, boton.ColorTexto);
            Assert.AreEqual(DockStyle.Fill, boton.Dock);
        }

        [TestMethod]
        public void BotonSecundario_TieneBordeYTextoOscuro()
        {
            BotonRedondeado boton = Tema.BotonSecundario("Consultar");
            Assert.AreEqual(Tema.Superficie, boton.ColorFondo);
            Assert.AreEqual(Tema.Borde, boton.ColorBorde);
            Assert.AreEqual(Tema.Texto, boton.ColorTexto);
        }

        [TestMethod]
        public void BotonSobreNavy_EsTransparenteConTextoClaro()
        {
            BotonRedondeado boton = Tema.BotonSobreNavy("Cerrar sesión");
            Assert.AreEqual(Color.Transparent, boton.ColorFondo);
            Assert.AreEqual(Color.White, boton.ColorTexto);
        }

        [TestMethod]
        public void EstilizarTabla_ApagaEdicionYOrdenamiento()
        {
            var tabla = new DataGridView();
            Tema.EstilizarTabla(tabla);
            Assert.IsTrue(tabla.ReadOnly);
            Assert.IsFalse(tabla.AllowUserToAddRows);
            Assert.IsFalse(tabla.RowHeadersVisible);
            Assert.AreEqual(Tema.Superficie, tabla.BackgroundColor);
        }

        [TestMethod]
        public void Recursos_LaMarcaMonsterVieneIncrustada()
        {
            using (Image marca = Recursos.CargarMarca())
            {
                Assert.IsNotNull(marca);
                Assert.IsTrue(marca.Width > 0 && marca.Height > 0);
            }
            Assert.AreEqual("ec.edu.monster.escritorio.recursos.Monster.png", Recursos.Marca);
        }
    }
}
