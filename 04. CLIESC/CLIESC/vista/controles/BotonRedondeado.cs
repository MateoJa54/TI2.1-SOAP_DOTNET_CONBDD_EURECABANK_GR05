using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista.controles
{
    // Botón con esquinas redondeadas. Cambia de color al pasar el cursor y al presionar, sin animación.
    public class BotonRedondeado : Button
    {
        private Color _colorFondo = Tema.Primario;
        private Color _colorFondoHover = Tema.PrimarioOscuro;
        private Color _colorFondoPresionado = Tema.PrimarioOscuro;
        private Color _colorBorde = Color.Transparent;
        private Color _colorTexto = Color.White;
        private Color _colorTextoDeshabilitado = Tema.TextoTenue;
        private int _radio = Tema.Radio;
        private bool _encima;
        private bool _presionado;

        public BotonRedondeado()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Font = Tema.Boton;
            Cursor = Cursors.Hand;
            Height = 42;
        }

        public Color ColorFondo
        {
            get => _colorFondo;
            set { _colorFondo = value; Invalidate(); }
        }

        public Color ColorFondoHover
        {
            get => _colorFondoHover;
            set { _colorFondoHover = value; Invalidate(); }
        }

        public Color ColorFondoPresionado
        {
            get => _colorFondoPresionado;
            set { _colorFondoPresionado = value; Invalidate(); }
        }

        public Color ColorBorde
        {
            get => _colorBorde;
            set { _colorBorde = value; Invalidate(); }
        }

        public Color ColorTexto
        {
            get => _colorTexto;
            set { _colorTexto = value; Invalidate(); }
        }

        public Color ColorTextoDeshabilitado
        {
            get => _colorTextoDeshabilitado;
            set { _colorTextoDeshabilitado = value; Invalidate(); }
        }

        public int Radio
        {
            get => _radio;
            set { _radio = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var area = new Rectangle(0, 0, Width - 1, Height - 1);
            Color fondo = !Enabled
                ? Mezclar(_colorFondo, Tema.Borde, 0.55f)
                : _presionado ? _colorFondoPresionado
                : _encima ? _colorFondoHover : _colorFondo;

            using (var ruta = Tema.RutaRedondeada(area, _radio))
            {
                if (fondo.A > 0)
                {
                    using (var pincel = new SolidBrush(fondo))
                    {
                        g.FillPath(pincel, ruta);
                    }
                }
                if (_colorBorde.A > 0)
                {
                    using (var lapiz = new Pen(_colorBorde))
                    {
                        g.DrawPath(lapiz, ruta);
                    }
                }
            }

            Color colorTexto = Enabled ? _colorTexto : _colorTextoDeshabilitado;
            TextRenderer.DrawText(g, Text, Font, new Rectangle(0, 0, Width, Height), colorTexto,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);

            if (Focused && ShowFocusCues)
            {
                var foco = new Rectangle(3, 3, Width - 7, Height - 7);
                ControlPaint.DrawFocusRectangle(g, foco, colorTexto, fondo);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _encima = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _encima = false;
            _presionado = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            _presionado = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _presionado = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            Invalidate();
            base.OnEnabledChanged(e);
        }

        private static Color Mezclar(Color a, Color b, float cantidad)
        {
            int r = (int)(a.R * (1 - cantidad) + b.R * cantidad);
            int v = (int)(a.G * (1 - cantidad) + b.G * cantidad);
            int az = (int)(a.B * (1 - cantidad) + b.B * cantidad);
            return Color.FromArgb(a.A, r, v, az);
        }
    }
}
