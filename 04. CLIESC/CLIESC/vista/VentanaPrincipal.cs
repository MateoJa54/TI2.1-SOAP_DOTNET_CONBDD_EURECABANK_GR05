using System;
using System.Drawing;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista
{
    // Ventana única: muestra el acceso o la pantalla principal según la navegación del mediador.
    public sealed class VentanaPrincipal : Form
    {
        private readonly IMediadorEscritorio _mediador;
        private readonly VistaAcceso _acceso;
        private readonly VistaPrincipal _principal;

        public VentanaPrincipal(IMediadorEscritorio mediador)
        {
            _mediador = mediador ?? throw new ArgumentNullException(nameof(mediador));

            Text = "EURECABANK - Cliente de escritorio";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(980, 680);
            ClientSize = new Size(1040, 720);
            BackColor = Tema.Fondo;
            Font = Tema.Base;

            _acceso = new VistaAcceso(_mediador);
            _principal = new VistaPrincipal(_mediador);

            var contenedor = new Panel { Dock = DockStyle.Fill, BackColor = Tema.Fondo };
            contenedor.Controls.Add(_principal);
            contenedor.Controls.Add(_acceso);
            Controls.Add(contenedor);

            _mediador.Navegacion.Cambio += (s, e) => MostrarPantalla();
            MostrarPantalla();
        }

        private void MostrarPantalla()
        {
            Navegacion navegacion = _mediador.Navegacion;
            bool principal = navegacion.Actual == Pantalla.Principal;
            _principal.Visible = principal;
            _acceso.Visible = !principal;
            if (principal)
            {
                _principal.AlMostrar();
                _principal.BringToFront();
            }
            else
            {
                _acceso.AlMostrar(navegacion.MensajeAcceso, navegacion.MensajeEsError);
                _acceso.BringToFront();
            }
        }
    }
}
