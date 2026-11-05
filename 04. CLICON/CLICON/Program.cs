using System;
using System.Text;
using ec.edu.monster.consola.controlador;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using ec.edu.monster.consola.vista;
using ec.edu.monster.consola.ws;

namespace ec.edu.monster.consola
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            string url = DireccionServicio.Resolver(args.Length > 0 ? args[0] : null);
            Consola consola = new Consola();
            consola.Linea("EURECABANK - cliente de consola (servicio: " + url + ")");
            Ejecutar(consola, new ClienteWSEurekabank(url));
            return 0;
        }

        public static void Ejecutar(Consola consola, IClienteEurekabank cliente)
        {
            SesionUsuario sesion = new SesionUsuario();
            VistaAutenticacion autenticacion = new VistaAutenticacion(consola, new ControladorAutenticacion(cliente, sesion), sesion);
            VistaMenu menu = new VistaMenu(consola, sesion, autenticacion,
                new VistaCuentas(consola, new ControladorCuentas(cliente, sesion)),
                new VistaMovimientos(consola, new ControladorMovimientos(cliente, sesion)),
                new VistaCostos(consola, new ControladorCostos(cliente, sesion)));
            while (autenticacion.IniciarSesion())
            {
                menu.Ejecutar();
            }
            consola.Linea("Hasta luego.");
        }
    }
}
