using System;
using System.Collections.Generic;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.seguridad;
using ec.edu.monster.utilidades;

namespace ec.edu.monster.servicios
{
    public class ServicioCuentas
    {
        public const string EstadoActivo = "ACTIVO";

        private readonly IClienteDAO clienteDAO;
        private readonly ICuentaDAO cuentaDAO;
        private readonly TokenServicio tokenServicio;

        public ServicioCuentas(IClienteDAO clienteDAO, ICuentaDAO cuentaDAO, TokenServicio tokenServicio)
        {
            if (clienteDAO == null) throw new ArgumentNullException("clienteDAO");
            if (cuentaDAO == null) throw new ArgumentNullException("cuentaDAO");
            if (tokenServicio == null) throw new ArgumentNullException("tokenServicio");
            this.clienteDAO = clienteDAO;
            this.cuentaDAO = cuentaDAO;
            this.tokenServicio = tokenServicio;
        }

        public RespuestaCuentas ListarCuentasPorCliente(string cedula, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.CuentasFallidas(acceso);
            }
            if (Textos.EstaVacio(cedula))
            {
                return FabricaRespuestas.CuentasFallidas(CodigoRespuesta.Dat001);
            }
            DatosCliente cliente = clienteDAO.BuscarPorDni(cedula.Trim());
            if (cliente == null)
            {
                return FabricaRespuestas.CuentasFallidas(CodigoRespuesta.Cli001);
            }
            List<CuentaDTO> cuentas = cuentaDAO.ListarPorCliente(cliente.Codigo);
            return FabricaRespuestas.CuentasExitosas(cedula.Trim(), cliente.Nombre, cuentas);
        }

        public RespuestaCuenta ConsultarCuenta(string cuenta, string token)
        {
            CodigoRespuesta acceso = tokenServicio.ValidarToken(token);
            if (!acceso.Exitoso)
            {
                return FabricaRespuestas.CuentaFallida(acceso);
            }
            if (Textos.EstaVacio(cuenta))
            {
                return FabricaRespuestas.CuentaFallida(CodigoRespuesta.Dat001);
            }
            CuentaDTO encontrada = cuentaDAO.BuscarPorCodigo(cuenta.Trim());
            if (encontrada == null)
            {
                return FabricaRespuestas.CuentaFallida(CodigoRespuesta.Cta001);
            }
            if (encontrada.Estado != EstadoActivo)
            {
                return FabricaRespuestas.CuentaFallida(CodigoRespuesta.Cta002);
            }
            return FabricaRespuestas.CuentaExitosa(encontrada);
        }
    }
}
