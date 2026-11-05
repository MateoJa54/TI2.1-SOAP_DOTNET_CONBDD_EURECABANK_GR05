using System;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.seguridad;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.servicios
{
    // RB-10 a RB-12: el servicio solo proyecta los costos; el cobro lo hace un proceso batch externo.
    public class ServicioCostos
    {
        public const string ParametroOperacionesSinCosto = "002";

        private readonly ICuentaDAO cuentaDAO;
        private readonly ICostoDAO costoDAO;
        private readonly IMonedaDAO monedaDAO;
        private readonly IParametroDAO parametroDAO;
        private readonly TokenServicio tokenServicio;

        public ServicioCostos(ICuentaDAO cuentaDAO, ICostoDAO costoDAO, IMonedaDAO monedaDAO,
            IParametroDAO parametroDAO, TokenServicio tokenServicio)
        {
            if (cuentaDAO == null) throw new ArgumentNullException("cuentaDAO");
            if (costoDAO == null) throw new ArgumentNullException("costoDAO");
            if (monedaDAO == null) throw new ArgumentNullException("monedaDAO");
            if (parametroDAO == null) throw new ArgumentNullException("parametroDAO");
            if (tokenServicio == null) throw new ArgumentNullException("tokenServicio");
            this.cuentaDAO = cuentaDAO;
            this.costoDAO = costoDAO;
            this.monedaDAO = monedaDAO;
            this.parametroDAO = parametroDAO;
            this.tokenServicio = tokenServicio;
        }

        public RespuestaCostos ConsultarCostosMantenimiento(string cuenta, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.CostosFallidos(acceso);
            }
            if (Textos.EstaVacio(cuenta))
            {
                return FabricaRespuestas.CostosFallidos(CodigoRespuesta.Dat001);
            }
            CuentaDTO datos = cuentaDAO.BuscarPorCodigo(cuenta.Trim());
            if (datos == null)
            {
                return FabricaRespuestas.CostosFallidos(CodigoRespuesta.Cta001);
            }
            if (datos.Estado != ServicioCuentas.EstadoActivo)
            {
                return FabricaRespuestas.CostosFallidos(CodigoRespuesta.Cta002);
            }
            if (!monedaDAO.EstaSoportada(datos.Moneda))
            {
                return FabricaRespuestas.CostosFallidos(CodigoRespuesta.Mon002);
            }
            decimal saldo = FormatoDinero.ADosDecimales(datos.Saldo);
            decimal limite = FormatoDinero.Cero;
            decimal costoMantenimiento = FormatoDinero.Cero;
            DatosCostoMantenimiento mantenimiento = costoDAO.CostoMantenimiento(datos.Moneda);
            if (mantenimiento != null)
            {
                limite = FormatoDinero.ADosDecimales(mantenimiento.MontoMaximo);
                if (saldo <= limite)
                {
                    costoMantenimiento = FormatoDinero.ADosDecimales(mantenimiento.Importe);
                }
            }
            decimal? costoPorMovimiento = costoDAO.CostoMovimiento(datos.Moneda);
            decimal costoMovimiento = costoPorMovimiento.HasValue
                ? FormatoDinero.ADosDecimales(costoPorMovimiento.Value) : FormatoDinero.Cero;
            decimal? porcentajeInteres = costoDAO.InteresMensual(datos.Moneda);
            decimal interes = porcentajeInteres.HasValue
                ? FormatoDinero.Porcentaje(saldo, porcentajeInteres.Value) : FormatoDinero.Cero;
            decimal? sinCosto = parametroDAO.Valor(ParametroOperacionesSinCosto);
            int operacionesSinCosto = sinCosto.HasValue ? (int)sinCosto.Value : 0;
            return FabricaRespuestas.CostosExitosos(saldo, costoMantenimiento, costoMovimiento, interes, limite,
                operacionesSinCosto);
        }
    }
}
