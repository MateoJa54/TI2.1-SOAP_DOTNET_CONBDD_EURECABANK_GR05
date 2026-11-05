using System;
using System.Collections.Concurrent;
using System.Linq;
using ec.edu.monster.modelos;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.seguridad
{
    // RB-14: UUID aleatorio, vigencia de 30 minutos, registro en memoria.
    public class TokenServicio
    {
        public static readonly TimeSpan VigenciaPorDefecto = TimeSpan.FromMinutes(30);

        private readonly TimeSpan vigencia;
        private readonly Func<DateTime> reloj;
        private readonly ConcurrentDictionary<string, SesionToken> sesiones = new ConcurrentDictionary<string, SesionToken>();

        public TokenServicio() : this(VigenciaPorDefecto, () => DateTime.UtcNow)
        {
        }

        public TokenServicio(TimeSpan vigencia, Func<DateTime> reloj)
        {
            if (vigencia <= TimeSpan.Zero)
            {
                throw new ArgumentException("La vigencia debe ser positiva: " + vigencia, "vigencia");
            }
            if (reloj == null)
            {
                throw new ArgumentNullException("reloj", "El reloj es obligatorio");
            }
            this.vigencia = vigencia;
            this.reloj = reloj;
        }

        public TimeSpan Vigencia
        {
            get { return vigencia; }
        }

        public int CantidadSesiones
        {
            get { return sesiones.Count; }
        }

        public string GenerarToken(string empleado)
        {
            LimpiarMuertas();
            string token = Guid.NewGuid().ToString();
            sesiones[token] = new SesionToken(token, empleado, null, reloj().Add(vigencia));
            return token;
        }

        public void AsociarLogSession(string token, int? idSesionLog)
        {
            SesionToken actual;
            if (token != null && sesiones.TryGetValue(token, out actual))
            {
                sesiones[token] = new SesionToken(actual.Token, actual.Empleado, idSesionLog, actual.Expiracion);
            }
        }

        public DateTime? ObtenerExpiracion(string token)
        {
            SesionToken sesion = ObtenerSesion(token);
            return sesion == null ? (DateTime?)null : sesion.Expiracion;
        }

        public SesionToken ObtenerSesion(string token)
        {
            SesionToken sesion;
            if (token == null || !sesiones.TryGetValue(token, out sesion))
            {
                return null;
            }
            return sesion;
        }

        // Un token vencido se conserva para poder cerrar su fila de logsession (contrato 6.4).
        public CodigoRespuesta ValidarToken(string token)
        {
            if (Textos.EstaVacio(token))
            {
                return CodigoRespuesta.Dat001;
            }
            SesionToken sesion = ObtenerSesion(token);
            if (sesion == null)
            {
                return CodigoRespuesta.Auth003;
            }
            if (reloj() >= sesion.Expiracion)
            {
                return CodigoRespuesta.Auth004;
            }
            return CodigoRespuesta.Ok200;
        }

        public void Cerrar(string token)
        {
            SesionToken descartada;
            if (token != null)
            {
                sesiones.TryRemove(token, out descartada);
            }
        }

        private void LimpiarMuertas()
        {
            DateTime limite = reloj() - vigencia;
            foreach (string clave in sesiones.Where(par => par.Value.Expiracion < limite).Select(par => par.Key).ToList())
            {
                SesionToken descartada;
                sesiones.TryRemove(clave, out descartada);
            }
        }
    }
}
