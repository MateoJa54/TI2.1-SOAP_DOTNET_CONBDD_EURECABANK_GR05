using System;

namespace ec.edu.monster.seguridad
{
    public sealed class SesionToken
    {
        public SesionToken(string token, string empleado, int? idSesionLog, DateTime expiracion)
        {
            Token = token;
            Empleado = empleado;
            IdSesionLog = idSesionLog;
            Expiracion = expiracion;
        }

        public string Token { get; private set; }
        public string Empleado { get; private set; }
        public int? IdSesionLog { get; private set; }
        public DateTime Expiracion { get; private set; }
    }
}
