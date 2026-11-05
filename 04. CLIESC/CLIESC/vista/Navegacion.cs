using System;
using ec.edu.monster.escritorio.modelo;

namespace ec.edu.monster.escritorio.vista
{
    public enum Pantalla
    {
        Acceso,
        Principal
    }

    // Decide qué pantalla está visible. No muestra nada: la ventana escucha Cambio.
    public sealed class Navegacion
    {
        public Pantalla Actual { get; private set; } = Pantalla.Acceso;

        // Mensaje que la pantalla de acceso muestra al volver (por ejemplo, la sesión expiró).
        public string MensajeAcceso { get; private set; } = string.Empty;

        public bool MensajeEsError { get; private set; }

        public event EventHandler Cambio;

        public void IrAPrincipal()
        {
            Actual = Pantalla.Principal;
            MensajeAcceso = string.Empty;
            MensajeEsError = false;
            Notificar();
        }

        public void IrAAcceso(string mensaje, bool esError = false)
        {
            Actual = Pantalla.Acceso;
            MensajeAcceso = mensaje ?? string.Empty;
            MensajeEsError = esError;
            Notificar();
        }

        // AUTH-003 y AUTH-004 significan que el servidor ya no acepta la sesión: se vuelve al acceso con el mensaje.
        public bool RevisarRespuesta(RespuestaBase respuesta)
        {
            if (!Presentacion.EsSesionInvalida(respuesta))
            {
                return false;
            }
            IrAAcceso(Presentacion.Mensaje(respuesta), esError: true);
            return true;
        }

        private void Notificar()
        {
            Cambio?.Invoke(this, EventArgs.Empty);
        }
    }
}
