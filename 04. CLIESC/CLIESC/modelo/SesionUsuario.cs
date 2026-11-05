using System;

namespace ec.edu.monster.escritorio.modelo
{
    public class SesionUsuario
    {
        public string Token { get; private set; }
        public string Empleado { get; private set; }
        public string NombreEmpleado { get; private set; }
        public string Rol { get; private set; }
        public string Expiracion { get; private set; }

        public bool EstaVigente
        {
            get { return !string.IsNullOrEmpty(Token); }
        }

        public void Iniciar(RespuestaAutenticacion respuesta)
        {
            if (respuesta == null)
            {
                throw new ArgumentNullException("respuesta");
            }
            Token = respuesta.Token;
            Empleado = respuesta.Empleado;
            NombreEmpleado = respuesta.NombreEmpleado;
            Rol = respuesta.Rol;
            Expiracion = respuesta.Expiracion;
        }

        public void Limpiar()
        {
            Token = null;
            Empleado = null;
            NombreEmpleado = null;
            Rol = null;
            Expiracion = null;
        }
    }
}
