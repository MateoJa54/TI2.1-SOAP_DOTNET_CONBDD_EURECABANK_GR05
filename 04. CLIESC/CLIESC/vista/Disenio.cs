using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ec.edu.monster.escritorio.vista.controles;

namespace ec.edu.monster.escritorio.vista
{
    // Piezas de disposición compartidas por las pantallas: filas, tarjetas, tablas y mensajes de estado.
    public static class Disenio
    {
        // Cada altura en píxeles; 0 ocupa el espacio restante.
        public static TableLayoutPanel Filas(params int[] alturas)
        {
            var tabla = Transparente(alturas.Length, 1);
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            foreach (int altura in alturas)
            {
                tabla.RowStyles.Add(altura > 0
                    ? new RowStyle(SizeType.Absolute, altura)
                    : new RowStyle(SizeType.Percent, 100f));
            }
            return tabla;
        }

        // Dos controles en una misma fila: el de la izquierda ocupa el espacio y el de la derecha un ancho fijo.
        public static TableLayoutPanel Lado(int anchoDerecha, Control izquierda, Control derecha)
        {
            var tabla = Transparente(1, 2);
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, anchoDerecha));
            tabla.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            izquierda.Dock = DockStyle.Fill;
            derecha.Dock = DockStyle.Fill;
            tabla.Controls.Add(izquierda, 0, 0);
            tabla.Controls.Add(derecha, 1, 0);
            return tabla;
        }

        public static PanelTarjeta Tarjeta(Control contenido, int relleno = 22)
        {
            var tarjeta = new PanelTarjeta { Dock = DockStyle.Fill, Padding = new Padding(relleno) };
            contenido.Dock = DockStyle.Fill;
            tarjeta.Controls.Add(contenido);
            return tarjeta;
        }

        public static DataGridView Tabla(params string[] encabezados)
        {
            var tabla = new DataGridView { Dock = DockStyle.Fill };
            Tema.EstilizarTabla(tabla);
            foreach (string encabezado in encabezados)
            {
                tabla.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = encabezado,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
            }
            return tabla;
        }

        public static void Llenar(DataGridView tabla, IEnumerable<string[]> filas)
        {
            tabla.Rows.Clear();
            foreach (string[] fila in filas)
            {
                tabla.Rows.Add(fila);
            }
        }

        public static Label Estado()
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = Tema.Base,
                ForeColor = Tema.TextoSecundario,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = " "
            };
        }

        public static void PintarEstado(Label etiqueta, string texto, bool error)
        {
            etiqueta.Text = string.IsNullOrEmpty(texto) ? " " : texto;
            etiqueta.ForeColor = error ? Tema.Error : Tema.TextoSecundario;
        }

        private static TableLayoutPanel Transparente(int filas, int columnas)
        {
            return new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = filas,
                ColumnCount = columnas,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
        }
    }
}
