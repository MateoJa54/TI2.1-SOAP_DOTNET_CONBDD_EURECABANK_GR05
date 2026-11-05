using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista.controles
{
    // Campo de texto con fondo redondeado. El TextBox real vive dentro para conservar la edición nativa.
    // La etiqueta va en un Label aparte, encima del campo.
    public class CampoTexto : UserControl
    {
        private readonly TextBox _caja;
        private bool _enfocado;

        public CampoTexto()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            BackColor = Tema.Superficie;
            Dock = DockStyle.Fill;
            Height = 44;
            Font = Tema.Base;

            _caja = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Tema.Entrada,
                ForeColor = Tema.Texto,
                Font = Tema.Base
            };
            _caja.Enter += (s, e) => CambiarFoco(true);
            _caja.Leave += (s, e) => CambiarFoco(false);
            _caja.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    EnterPresionado?.Invoke(this, EventArgs.Empty);
                }
            };
            Controls.Add(_caja);
        }

        public event EventHandler EnterPresionado;

        public TextBox Caja => _caja;

        public string Valor
        {
            get => _caja.Text;
            set => _caja.Text = value ?? string.Empty;
        }

        public bool EsPassword
        {
            get => _caja.UseSystemPasswordChar;
            set => _caja.UseSystemPasswordChar = value;
        }

        public bool SoloLectura
        {
            get => _caja.ReadOnly;
            set => _caja.ReadOnly = value;
        }

        public int MaximoCaracteres
        {
            get => _caja.MaxLength;
            set => _caja.MaxLength = value;
        }

        public void Enfocar()
        {
            _caja.Focus();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AjustarCaja();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var area = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var ruta = Tema.RutaRedondeada(area, Tema.Radio))
            using (var pincel = new SolidBrush(_enfocado ? Tema.Superficie : Tema.Entrada))
            using (var lapiz = new Pen(_enfocado ? Tema.Primario : Tema.Borde))
            {
                g.FillPath(pincel, ruta);
                g.DrawPath(lapiz, ruta);
            }
        }

        private void CambiarFoco(bool enfocado)
        {
            _enfocado = enfocado;
            _caja.BackColor = enfocado ? Tema.Superficie : Tema.Entrada;
            Invalidate();
        }

        private void AjustarCaja()
        {
            if (_caja == null)
            {
                return;
            }
            const int margen = 14;
            int alto = _caja.PreferredHeight;
            _caja.SetBounds(margen, (Height - alto) / 2, Math.Max(0, Width - 2 * margen), alto);
        }
    }
}
