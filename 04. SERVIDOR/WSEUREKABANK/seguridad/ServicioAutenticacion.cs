using System;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.servicios;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.seguridad
{
    // Orden del contrato 6.2: DAT-001, AUTH-001, AUTH-002. La clave se compara en tiempo constante (RB-13).
    public class ServicioAutenticacion
    {
        public const string IpDesconocida = "NONE";
        public const string HostDesconocido = "NONE";
        public const string EstadoActivo = "ACTIVO";

        private readonly IUsuarioDAO usuarioDAO;
        private readonly ILogSessionDAO logSessionDAO;
        private readonly TokenServicio tokenServicio;

        public ServicioAutenticacion(IUsuarioDAO usuarioDAO, ILogSessionDAO logSessionDAO, TokenServicio tokenServicio)
        {
            if (usuarioDAO == null) throw new ArgumentNullException("usuarioDAO");
            if (logSessionDAO == null) throw new ArgumentNullException("logSessionDAO");
            if (tokenServicio == null) throw new ArgumentNullException("tokenServicio");
            this.usuarioDAO = usuarioDAO;
            this.logSessionDAO = logSessionDAO;
            this.tokenServicio = tokenServicio;
        }

        public RespuestaAutenticacion Autenticar(string usuario, string clave)
        {
            if (Textos.EstaVacio(usuario) || Textos.EstaVacio(clave))
            {
                return FabricaRespuestas.AutenticacionFallida(CodigoRespuesta.Dat001);
            }
            DatosUsuario encontrado = usuarioDAO.BuscarPorNombre(usuario.Trim());
            if (encontrado == null || !HashClave.Coincide(clave, encontrado.Hash))
            {
                return FabricaRespuestas.AutenticacionFallida(CodigoRespuesta.Auth001);
            }
            if (encontrado.Estado != EstadoActivo)
            {
                return FabricaRespuestas.AutenticacionFallida(CodigoRespuesta.Auth002);
            }
            string token = tokenServicio.GenerarToken(encontrado.Codigo);
            int? idSesion = logSessionDAO.RegistrarIngreso(encontrado.Codigo, IpDesconocida, HostDesconocido);
            tokenServicio.AsociarLogSession(token, idSesion);
            DateTime? expiracion = tokenServicio.ObtenerExpiracion(token);
            if (!expiracion.HasValue)
            {
                throw new InvalidOperationException("El token recién generado no existe");
            }
            return FabricaRespuestas.AutenticacionExitosa(token, encontrado.Codigo, encontrado.NombreEmpleado,
                expiracion.Value.ToString(Codigos.FormatoFechaHora), encontrado.Rol);
        }

        public RespuestaBase CerrarSesion(string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (acceso == CodigoRespuesta.Dat001 || acceso == CodigoRespuesta.Auth003)
            {
                return FabricaRespuestas.CierreSesionFallido(acceso);
            }
            SesionToken sesion = tokenServicio.ObtenerSesion(token);
            if (sesion != null)
            {
                logSessionDAO.RegistrarSalida(sesion.IdSesionLog);
            }
            tokenServicio.Cerrar(token);
            if (acceso == CodigoRespuesta.Auth004)
            {
                return FabricaRespuestas.CierreSesionFallido(CodigoRespuesta.Auth004);
            }
            return FabricaRespuestas.CierreSesionExitoso();
        }
    }
}
