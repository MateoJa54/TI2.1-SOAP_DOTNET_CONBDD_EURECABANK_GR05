using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista.controles
{
    // Tarjeta con esquinas redondeadas (16 px), fondo blanco y borde sutil. Sobre ella se apoyan los formularios.
    public class PanelTarjeta : Panel
    {
        private int _radio = Tema.RadioTarjeta;
        private Color _fondo = Tema.Superficie;
        private Color _borde = Tema.Borde;

        public PanelTarjeta()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw, true);
            BackColor = Color.Transparent;
        }

        public Color Fondo
        {
            get => _fondo;
            set { _fondo = value; Invalidate(); }
        }

        public Color ColorBorde
        {
            get => _borde;
            set { _borde = value; Invalidate(); }
        }

        public int Radio
        {
            get => _radio;
            set { _radio = value; Invalidate(); }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var area = new Rectangle(0, 0, Width - 1, Height - 1);
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }
            using (var ruta = Tema.RutaRedondeada(area, _radio))
            {
                using (var pincel = new SolidBrush(_fondo))
                {
                    g.FillPath(pincel, ruta);
                }
                using (var lapiz = new Pen(_borde))
                {
                    g.DrawPath(lapiz, ruta);
                }
            }
        }
    }
}
