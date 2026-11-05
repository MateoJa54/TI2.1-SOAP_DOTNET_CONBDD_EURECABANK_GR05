using ec.edu.monster.escritorio.controlador;
using ec.edu.monster.escritorio.modelo;

namespace ec.edu.monster.escritorio.vista
{
    // Lo que las pantallas necesitan del resto de la aplicación, sin conocer la ventana ni el servidor.
    public interface IMediadorEscritorio
    {
        SesionUsuario Sesion { get; }

        ControladorAutenticacion Autenticacion { get; }

        ControladorCuentas Cuentas { get; }

        ControladorMovimientos Movimientos { get; }

        ControladorCostos Costos { get; }

        Navegacion Navegacion { get; }

        string Url { get; }

        // Si hay sesión vigente, pasa a la pantalla principal y devuelve true.
        bool SesionIniciada();

        // Pasa a la pantalla de acceso con el mensaje de cierre.
        void SesionCerrada();

        // Si la respuesta es AUTH-003 o AUTH-004 vuelve al acceso y devuelve true.
        bool RevisarRespuesta(RespuestaBase respuesta);
    }
}
