using System;
using ec.edu.monster.escritorio.controlador;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.ws;

namespace ec.edu.monster.escritorio.vista
{
    // Mediador sin ventanas: guarda la sesión, los controladores y la navegación.
    // Las pantallas lo usan; la ventana principal escucha Navegacion.Cambio.
    public sealed class MediadorEscritorio : IMediadorEscritorio
    {
        public MediadorEscritorio(IClienteEurekabank cliente, string url)
        {
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));
            Url = url ?? string.Empty;
            Sesion = new SesionUsuario();
            Navegacion = new Navegacion();
            Autenticacion = new ControladorAutenticacion(cliente, Sesion);
            Cuentas = new ControladorCuentas(cliente, Sesion);
            Movimientos = new ControladorMovimientos(cliente, Sesion);
            Costos = new ControladorCostos(cliente, Sesion);
        }

        public SesionUsuario Sesion { get; }

        public ControladorAutenticacion Autenticacion { get; }

        public ControladorCuentas Cuentas { get; }

        public ControladorMovimientos Movimientos { get; }

        public ControladorCostos Costos { get; }

        public Navegacion Navegacion { get; }

        public string Url { get; }

        public bool SesionIniciada()
        {
            if (!Sesion.EstaVigente)
            {
                return false;
            }
            Navegacion.IrAPrincipal();
            return true;
        }

        public void SesionCerrada()
        {
            Navegacion.IrAAcceso(Presentacion.SesionCerrada);
        }

        public bool RevisarRespuesta(RespuestaBase respuesta)
        {
            return Navegacion.RevisarRespuesta(respuesta);
        }
    }
}
