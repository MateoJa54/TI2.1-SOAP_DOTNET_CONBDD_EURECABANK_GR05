using System;
using System.Drawing;
using System.Windows.Forms;

namespace ec.edu.monster.escritorio.vista
{
    // Base de las cuatro pestañas de la pantalla principal: fondo, mediador y el tratamiento común de fallos.
    public abstract class VistaModulo : UserControl
    {
        protected readonly IMediadorEscritorio Mediador;

        protected VistaModulo(IMediadorEscritorio mediador)
        {
            Mediador = mediador ?? throw new ArgumentNullException(nameof(mediador));
            Dock = DockStyle.Fill;
            BackColor = Tema.Fondo;
            Padding = new Padding(24, 16, 24, 16);
        }

        // Habilita o deshabilita los botones mientras una llamada está en curso.
        protected abstract void Ocupar(bool ocupado);

        // Vacía los datos mostrados (al cerrar sesión).
        public abstract void Limpiar();

        // Una excepción no prevista: el detalle va al registro; la pantalla solo muestra el mensaje general.
        protected void Fallo(Exception causa, string contexto, Label etiqueta)
        {
            Ocupar(false);
            Disenio.PintarEstado(etiqueta, Presentacion.FalloInesperado(causa, contexto), true);
        }
    }
}
