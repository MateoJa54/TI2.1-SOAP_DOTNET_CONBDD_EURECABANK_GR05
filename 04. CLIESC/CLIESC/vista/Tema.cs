using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Identidad visual de EURECABANK: paleta, tipografía y estilos. Un solo acento (el primario).
    public static class Tema
    {
        private static Color Hex(int rgb)
        {
            return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        }

        public static readonly Color Fondo = Hex(0xF3F7FB);
        public static readonly Color Superficie = Hex(0xFFFFFF);
        public static readonly Color Entrada = Hex(0xF7F9FC);
        public static readonly Color Borde = Hex(0xE4EAF2);
        public static readonly Color Navy = Hex(0x133049);
        public static readonly Color Texto = Hex(0x1F3142);
        public static readonly Color TextoSecundario = Hex(0x54677A);
        public static readonly Color TextoTenue = Hex(0x8B9AAB);
        public static readonly Color Primario = Hex(0x2563EB);
        public static readonly Color PrimarioOscuro = Hex(0x1769E0);
        public static readonly Color PrimarioSuave = Hex(0xE7EFFE);
        public static readonly Color Marca = Hex(0x0F8591);
        public static readonly Color Error = Hex(0xB8313A);
        public static readonly Color ErrorSuave = Hex(0xFBEBEC);
        public static readonly Color Exito = Hex(0x15803D);
        public static readonly Color ExitoSuave = Hex(0xE8F6EE);

        public const string Familia = "Segoe UI";
        public const float TamanoBase = 9.75f;
        public const int Radio = 12;
        public const int RadioTarjeta = 16;

        public static Font Fuente(float tamano, FontStyle estilo = FontStyle.Regular)
        {
            return new Font(Familia, tamano, estilo, GraphicsUnit.Point);
        }

        public static readonly Font Base = Fuente(TamanoBase);
        public static readonly Font Marca18 = Fuente(18f, FontStyle.Bold);
        public static readonly Font Titulo = Fuente(20f, FontStyle.Bold);
        public static readonly Font TarjetaTitulo = Fuente(12f, FontStyle.Bold);
        public static readonly Font FuenteEtiqueta = Fuente(8.5f, FontStyle.Bold);
        public static readonly Font Boton = Fuente(TamanoBase, FontStyle.Bold);
        public static readonly Font Pequena = Fuente(8.5f);
        public static readonly Font Valor = Fuente(14f, FontStyle.Bold);

        public static void Instalar()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        public static GraphicsPath RutaRedondeada(Rectangle area, int radio)
        {
            var ruta = new GraphicsPath();
            int diametro = radio * 2;
            if (diametro <= 0)
            {
                ruta.AddRectangle(area);
                return ruta;
            }
            ruta.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
            ruta.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
            ruta.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }

        public static Label Etiqueta(string texto, Font fuente, Color color)
        {
            return new Label
            {
                Text = texto,
                Font = fuente,
                ForeColor = color,
                AutoSize = true,
                BackColor = Color.Transparent
            };
        }

        // Etiqueta sobre un campo: en mayúsculas pequeñas, siempre encima del campo.
        public static Label EtiquetaCampo(string texto)
        {
            return Etiqueta(texto.ToUpperInvariant(), FuenteEtiqueta, TextoSecundario);
        }

        public static BotonRedondeado BotonPrimario(string texto)
        {
            return new BotonRedondeado
            {
                Dock = DockStyle.Fill,
                Text = texto,
                ColorFondo = Primario,
                ColorFondoHover = PrimarioOscuro,
                ColorFondoPresionado = PrimarioOscuro,
                ColorTexto = Color.White
            };
        }

        public static BotonRedondeado BotonSecundario(string texto)
        {
            return new BotonRedondeado
            {
                Dock = DockStyle.Fill,
                Text = texto,
                ColorFondo = Superficie,
                ColorFondoHover = Entrada,
                ColorFondoPresionado = Borde,
                ColorBorde = Borde,
                ColorTexto = Texto
            };
        }

        // Botón sobre la barra azul marino: fondo transparente y texto claro.
        public static BotonRedondeado BotonSobreNavy(string texto)
        {
            return new BotonRedondeado
            {
                Text = texto,
                ColorFondo = Color.Transparent,
                ColorFondoHover = Color.FromArgb(40, 255, 255, 255),
                ColorFondoPresionado = Color.FromArgb(70, 255, 255, 255),
                ColorBorde = Color.FromArgb(90, 255, 255, 255),
                ColorTexto = Color.White,
                ColorTextoDeshabilitado = TextoTenue,
                Size = new Size(150, 36)
            };
        }

        // Tabla de datos: encabezado tenue, filas claras y sin bordes laterales.
        public static void EstilizarTabla(DataGridView tabla)
        {
            tabla.AllowUserToAddRows = false;
            tabla.AllowUserToDeleteRows = false;
            tabla.AllowUserToResizeRows = false;
            tabla.ReadOnly = true;
            tabla.MultiSelect = false;
            tabla.RowHeadersVisible = false;
            tabla.BackgroundColor = Superficie;
            tabla.BorderStyle = BorderStyle.None;
            tabla.GridColor = Borde;
            tabla.Font = Base;
            tabla.EnableHeadersVisualStyles = false;
            tabla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            tabla.ColumnHeadersHeight = 34;
            tabla.ColumnHeadersDefaultCellStyle.BackColor = Entrada;
            tabla.ColumnHeadersDefaultCellStyle.ForeColor = TextoSecundario;
            tabla.ColumnHeadersDefaultCellStyle.Font = FuenteEtiqueta;
            tabla.ColumnHeadersDefaultCellStyle.SelectionBackColor = Entrada;
            tabla.DefaultCellStyle.BackColor = Superficie;
            tabla.DefaultCellStyle.ForeColor = Texto;
            tabla.DefaultCellStyle.SelectionBackColor = PrimarioSuave;
            tabla.DefaultCellStyle.SelectionForeColor = Texto;
            tabla.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            tabla.RowTemplate.Height = 32;
            tabla.AlternatingRowsDefaultCellStyle.BackColor = Superficie;
        }
    }
}
