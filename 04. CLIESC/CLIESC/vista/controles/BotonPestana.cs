using System;
using System.Drawing;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista.controles
{
    // Pestaña tipo subrayado: el texto cambia de color y una línea azul marca la pestaña activa.
    public class BotonPestana : Control
    {
        private bool _activa;
        private bool _encima;

        public BotonPestana()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            Font = Tema.Fuente(Tema.TamanoBase, FontStyle.Bold);
            Cursor = Cursors.Hand;
            Size = new Size(150, 46);
        }

        public bool Activa
        {
            get => _activa;
            set { _activa = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            if (_encima && !_activa)
            {
                using (var pincel = new SolidBrush(Tema.PrimarioSuave))
                {
                    g.FillRectangle(pincel, ClientRectangle);
                }
            }

            Color color = _activa ? Tema.Primario : Tema.TextoSecundario;
            TextRenderer.DrawText(g, Text, Font,
                new Rectangle(0, 0, Width, Height - 2), color,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            if (_activa)
            {
                using (var lapiz = new Pen(Tema.Primario, 3f))
                {
                    g.DrawLine(lapiz, 10, Height - 2, Width - 10, Height - 2);
                }
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
            Invalidate();
            base.OnMouseLeave(e);
        }
    }
}
