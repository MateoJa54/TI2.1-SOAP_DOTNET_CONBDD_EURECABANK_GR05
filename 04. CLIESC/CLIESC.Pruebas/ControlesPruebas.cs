using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.vista;
using ec.edu.monster.escritorio.vista.controles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    // Controles y disposición sin mostrar ventanas: solo se crean objetos y se leen sus propiedades.
    [TestClass]
    public class ControlesPruebas
    {
        [TestMethod]
        public void BotonRedondeado_ValoresPorDefecto()
        {
            var boton = new BotonRedondeado();
            Assert.AreEqual(12, boton.Radio);
            Assert.AreEqual(42, boton.Height);
            Assert.AreEqual(FlatStyle.Flat, boton.FlatStyle);
            Assert.AreEqual(Tema.Primario, boton.ColorFondo);
            Assert.AreEqual(Tema.PrimarioOscuro, boton.ColorFondoHover);
            Assert.AreEqual(Color.Transparent, boton.ColorBorde);
            Assert.AreEqual(Color.White, boton.ColorTexto);
        }

        [TestMethod]
        [DataRow(4)]
        [DataRow(12)]
        [DataRow(24)]
        public void BotonRedondeado_RadioSeGuarda(int radio)
        {
            var boton = new BotonRedondeado { Radio = radio };
            Assert.AreEqual(radio, boton.Radio);
        }

        [TestMethod]
        public void BotonRedondeado_ColoresSeGuardan()
        {
            var boton = new BotonRedondeado
            {
                ColorFondo = Tema.Navy,
                ColorFondoPresionado = Tema.Borde,
                ColorTextoDeshabilitado = Tema.TextoTenue
            };
            Assert.AreEqual(Tema.Navy, boton.ColorFondo);
            Assert.AreEqual(Tema.Borde, boton.ColorFondoPresionado);
            Assert.AreEqual(Tema.TextoTenue, boton.ColorTextoDeshabilitado);
        }

        [TestMethod]
        public void PanelTarjeta_ValoresPorDefecto()
        {
            var tarjeta = new PanelTarjeta();
            Assert.AreEqual(16, tarjeta.Radio);
            Assert.AreEqual(Tema.Superficie, tarjeta.Fondo);
            Assert.AreEqual(Tema.Borde, tarjeta.ColorBorde);
            Assert.AreEqual(Color.Transparent, tarjeta.BackColor);
        }

        [TestMethod]
        [DataRow(8)]
        [DataRow(16)]
        [DataRow(20)]
        public void PanelTarjeta_RadioSeGuarda(int radio)
        {
            var tarjeta = new PanelTarjeta { Radio = radio };
            Assert.AreEqual(radio, tarjeta.Radio);
        }

        [TestMethod]
        public void CampoTexto_ValoresPorDefecto()
        {
            var campo = new CampoTexto();
            Assert.AreEqual(44, campo.Height);
            Assert.AreEqual(DockStyle.Fill, campo.Dock);
            Assert.AreEqual(string.Empty, campo.Valor);
            Assert.IsFalse(campo.EsPassword);
            Assert.IsFalse(campo.SoloLectura);
        }

        [TestMethod]
        [DataRow("usuario1")]
        [DataRow("")]
        [DataRow("ñandú con espacios")]
        public void CampoTexto_ValorSeLeeComoSeEscribio(string valor)
        {
            var campo = new CampoTexto { Valor = valor };
            Assert.AreEqual(valor, campo.Valor);
            Assert.AreEqual(valor, campo.Caja.Text);
        }

        [TestMethod]
        public void CampoTexto_ValorNuloSeGuardaVacio()
        {
            var campo = new CampoTexto { Valor = null };
            Assert.AreEqual(string.Empty, campo.Valor);
        }

        [TestMethod]
        public void CampoTexto_ClaveUsaCaracterDeSistema()
        {
            var campo = new CampoTexto { EsPassword = true };
            Assert.IsTrue(campo.EsPassword);
            Assert.IsTrue(campo.Caja.UseSystemPasswordChar);
        }

        [TestMethod]
        [DataRow(10)]
        [DataRow(60)]
        public void CampoTexto_LongitudMaximaSeAplicaAlTextBox(int maximo)
        {
            var campo = new CampoTexto { MaximoCaracteres = maximo };
            Assert.AreEqual(maximo, campo.Caja.MaxLength);
        }

        [TestMethod]
        public void CampoTexto_SoloLecturaSeAplicaAlTextBox()
        {
            var campo = new CampoTexto { SoloLectura = true };
            Assert.IsTrue(campo.Caja.ReadOnly);
            campo.SoloLectura = false;
            Assert.IsFalse(campo.Caja.ReadOnly);
        }

        [TestMethod]
        public void BotonPestana_ActivaCambiaSuEstado()
        {
            var pestana = new BotonPestana();
            Assert.IsFalse(pestana.Activa);
            pestana.Activa = true;
            Assert.IsTrue(pestana.Activa);
        }

        [TestMethod]
        public void BotonPestana_TamanoYCursor()
        {
            var pestana = new BotonPestana();
            Assert.AreEqual(new Size(150, 46), pestana.Size);
            Assert.AreEqual(Cursors.Hand, pestana.Cursor);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(5)]
        public void Filas_CreaTantasFilasComoAlturas(int cantidad)
        {
            int[] alturas = new int[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                alturas[i] = 10 + i;
            }
            TableLayoutPanel tabla = Disenio.Filas(alturas);
            Assert.AreEqual(cantidad, tabla.RowCount);
            Assert.AreEqual(cantidad, tabla.RowStyles.Count);
        }

        [TestMethod]
        [DataRow(18)]
        [DataRow(44)]
        [DataRow(120)]
        public void Filas_AlturaFijaSeRespeta(int altura)
        {
            TableLayoutPanel tabla = Disenio.Filas(altura, 0);
            Assert.AreEqual(SizeType.Absolute, tabla.RowStyles[0].SizeType);
            Assert.AreEqual((float)altura, tabla.RowStyles[0].Height);
        }

        [TestMethod]
        public void Filas_CeroOcupaElResto()
        {
            TableLayoutPanel tabla = Disenio.Filas(20, 0, 30);
            Assert.AreEqual(SizeType.Percent, tabla.RowStyles[1].SizeType);
            Assert.AreEqual(100f, tabla.RowStyles[1].Height);
        }

        [TestMethod]
        [DataRow(80)]
        [DataRow(140)]
        public void Lado_LaColumnaDerechaTieneAnchoFijo(int anchoDerecha)
        {
            TableLayoutPanel tabla = Disenio.Lado(anchoDerecha, new TextBox(), new Button());
            Assert.AreEqual(2, tabla.ColumnCount);
            Assert.AreEqual(SizeType.Absolute, tabla.ColumnStyles[1].SizeType);
            Assert.AreEqual((float)anchoDerecha, tabla.ColumnStyles[1].Width);
        }

        [TestMethod]
        public void Lado_UbicaLosControlesEnLaFila()
        {
            var izquierda = new TextBox();
            var derecha = new Button();
            TableLayoutPanel tabla = Disenio.Lado(120, izquierda, derecha);
            Assert.AreEqual(0, tabla.GetColumn(izquierda));
            Assert.AreEqual(1, tabla.GetColumn(derecha));
            Assert.AreEqual(DockStyle.Fill, izquierda.Dock);
            Assert.AreEqual(DockStyle.Fill, derecha.Dock);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(8)]
        [DataRow(28)]
        public void Tarjeta_AplicaElRellenoPedido(int relleno)
        {
            PanelTarjeta tarjeta = Disenio.Tarjeta(new Panel(), relleno);
            Assert.AreEqual(relleno, tarjeta.Padding.Left);
            Assert.AreEqual(DockStyle.Fill, tarjeta.Dock);
            Assert.AreEqual(1, tarjeta.Controls.Count);
        }

        [TestMethod]
        public void Tarjeta_RellenoPorDefectoEs22()
        {
            PanelTarjeta tarjeta = Disenio.Tarjeta(new Panel());
            Assert.AreEqual(22, tarjeta.Padding.Left);
        }

        [TestMethod]
        [DataRow("Cuenta")]
        [DataRow("Saldo")]
        [DataRow("Estado")]
        public void Tabla_UnaColumnaPorEncabezado(string encabezado)
        {
            DataGridView tabla = Disenio.Tabla(encabezado, "Otro");
            Assert.AreEqual(2, tabla.Columns.Count);
            Assert.AreEqual(encabezado, tabla.Columns[0].HeaderText);
            Assert.AreEqual(DataGridViewColumnSortMode.NotSortable, tabla.Columns[0].SortMode);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(4)]
        public void Llenar_ReemplazaLasFilas(int cantidad)
        {
            DataGridView tabla = Disenio.Tabla("A", "B");
            Disenio.Llenar(tabla, new[] { new[] { "1", "2" } });
            string[][] filas = new string[cantidad][];
            for (int i = 0; i < cantidad; i++)
            {
                filas[i] = new[] { "f" + i, "v" + i };
            }

            Disenio.Llenar(tabla, filas);

            Assert.AreEqual(cantidad, tabla.Rows.Count);
            if (cantidad > 0)
            {
                Assert.AreEqual("f0", tabla.Rows[0].Cells[0].Value);
            }
        }

        [TestMethod]
        public void Estado_EmpiezaVacioConColorSecundario()
        {
            Label etiqueta = Disenio.Estado();
            Assert.AreEqual(" ", etiqueta.Text);
            Assert.AreEqual(Tema.TextoSecundario, etiqueta.ForeColor);
            Assert.IsFalse(etiqueta.AutoSize);
        }

        [TestMethod]
        [DataRow("Consultando...", false)]
        [DataRow("[MON-001] El monto debe ser mayor que cero y con máximo dos decimales", true)]
        [DataRow("Sin resultados", false)]
        public void PintarEstado_TextoYColor(string texto, bool error)
        {
            Label etiqueta = Disenio.Estado();
            Disenio.PintarEstado(etiqueta, texto, error);
            Assert.AreEqual(texto, etiqueta.Text);
            Assert.AreEqual(error ? Tema.Error : Tema.TextoSecundario, etiqueta.ForeColor);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void PintarEstado_TextoVacioSeMuestraComoEspacio(string texto)
        {
            Label etiqueta = Disenio.Estado();
            Disenio.PintarEstado(etiqueta, texto, false);
            Assert.AreEqual(" ", etiqueta.Text);
        }
    }
}
