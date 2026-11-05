using System;
using ec.edu.monster.datos;
using ec.edu.monster.seguridad;
using ec.edu.monster.servicios;

namespace ec.edu.monster.controladores
{
    public sealed class RaizComposicion
    {
        public const string VariableMinutosToken = "EUREKABANK_TOKEN_MINUTOS";
        public const int MinutosTokenPorDefecto = 30;

        private RaizComposicion()
        {
        }

        public TokenServicio Tokens { get; private set; }
        public ServicioAutenticacion Autenticacion { get; private set; }
        public ServicioCuentas Cuentas { get; private set; }
        public ServicioMovimientos Movimientos { get; private set; }
        public ServicioCostos Costos { get; private set; }

        public static RaizComposicion DelSistema()
        {
            return Crear(new ProveedorConexion(), VigenciaToken());
        }

        public static RaizComposicion Crear(ProveedorConexion proveedor, TimeSpan vigencia)
        {
            TokenServicio tokens = new TokenServicio(vigencia, () => DateTime.UtcNow);
            ClienteDAO clienteDAO = new ClienteDAO(proveedor);
            CuentaDAO cuentaDAO = new CuentaDAO(proveedor);
            MovimientoDAO movimientoDAO = new MovimientoDAO(proveedor);
            ParametroDAO parametroDAO = new ParametroDAO(proveedor);
            TipoMovimientoDAO tipoMovimientoDAO = new TipoMovimientoDAO(proveedor);
            MonedaDAO monedaDAO = new MonedaDAO(proveedor);
            CostoDAO costoDAO = new CostoDAO(proveedor);
            UnidadTrabajoSql unidadTrabajo = new UnidadTrabajoSql(proveedor);
            return new RaizComposicion
            {
                Tokens = tokens,
                Autenticacion = new ServicioAutenticacion(new UsuarioDAO(proveedor), new LogSessionDAO(proveedor), tokens),
                Cuentas = new ServicioCuentas(clienteDAO, cuentaDAO, tokens),
                Movimientos = new ServicioMovimientos(cuentaDAO, movimientoDAO, tipoMovimientoDAO, parametroDAO,
                    costoDAO, monedaDAO, unidadTrabajo, tokens),
                Costos = new ServicioCostos(cuentaDAO, costoDAO, monedaDAO, parametroDAO, tokens)
            };
        }

        public static TimeSpan VigenciaToken()
        {
            int minutos;
            string valor = ProveedorConexion.Leer(VariableMinutosToken, MinutosTokenPorDefecto.ToString());
            if (!int.TryParse(valor, out minutos) || minutos <= 0)
            {
                minutos = MinutosTokenPorDefecto;
            }
            return TimeSpan.FromMinutes(minutos);
        }
    }
}
