using System;
using System.Windows.Forms;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.vista;
using ec.edu.monster.escritorio.ws;

namespace ec.edu.monster.escritorio
{
    // Punto de arranque del cliente de escritorio. Resuelve la dirección del
    // servicio, crea el mediador y abre la ventana principal.
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Tema.Instalar();
            string url = DireccionServicio.Resolver(args.Length > 0 ? args[0] : null);
            var mediador = new MediadorEscritorio(new ClienteWSEurekabank(url), url);
            Application.Run(new VentanaPrincipal(mediador));
        }
    }
}
