using ec.edu.monster.web.util;

namespace ec.edu.monster.web.ws
{
    // Cliente WS compartido por todas las peticiones; no guarda estado de usuario.
    public static class ComposicionWeb
    {
        private static readonly IClienteEurekabank cliente = new ClienteWSEurekabank(DireccionServicio.Resolver(null));

        public static IClienteEurekabank Cliente
        {
            get { return cliente; }
        }
    }
}
