using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using ec.edu.monster.controladores;
using ec.edu.monster.controladores.mensajes;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.pruebas.Apoyo;
using ec.edu.monster.seguridad;
using ec.edu.monster.servicios;
using ec.edu.monster.utilidades;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.controladores
{
    [TestClass]
    public class FachadaTest
    {
        private Entorno entorno;
        private string token;

        [TestInitialize]
        public void Preparar()
        {
            entorno = new Entorno();
            token = entorno.Entrar();
        }

        private static string Codigo(Retorno r)
        {
            return r.Campos.Single(c => c.LocalName == "codigo").InnerText;
        }

        [TestMethod]
        public void Autenticar_Delega()
        {
            Retorno r = entorno.Fachada.Autenticar(new SolicitudAutenticar("cromero", "Cromero2026")).Resultado;
            Assert.AreEqual("200", Codigo(r));
        }

        [TestMethod]
        public void CerrarSesion_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.CerrarSesion(new SolicitudCerrarSesion(token)).Resultado));
        }

        [TestMethod]
        public void ListarCuentas_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("06914897", token)).Resultado));
        }

        [TestMethod]
        public void ConsultarCuenta_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.ConsultarCuenta(new SolicitudConsultarCuenta("00200002", token)).Resultado));
        }

        [TestMethod]
        public void ConsultarMovimientos_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.ConsultarMovimientos(new SolicitudConsultarMovimientos("00200002", token)).Resultado));
        }

        [TestMethod]
        public void Deposito_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.RealizarDeposito(new SolicitudRealizarDeposito("00200002", 10m, token)).Resultado));
        }

        [TestMethod]
        public void Retiro_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.RealizarRetiro(new SolicitudRealizarRetiro("00200002", 10m, token)).Resultado));
        }

        [TestMethod]
        public void Transferencia_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.RealizarTransferencia(new SolicitudRealizarTransferencia("00200002", "00200001", 10m, token)).Resultado));
        }

        [TestMethod]
        public void Costos_Delega()
        {
            Assert.AreEqual("200", Codigo(entorno.Fachada.ConsultarCostosMantenimiento(new SolicitudConsultarCostosMantenimiento("00200002", token)).Resultado));
        }

        [TestMethod]
        public void LasNueveOperacionesDelContratoExisten()
        {
            string[] esperadas =
            {
                "autenticar", "cerrarSesion", "listarCuentasPorCliente", "consultarCuenta", "consultarMovimientos",
                "realizarDeposito", "realizarRetiro", "realizarTransferencia", "consultarCostosMantenimiento"
            };
            string[] reales = typeof(IWSEUREKABANK).GetMethods()
                .Select(m => ((System.ServiceModel.OperationContractAttribute)m.GetCustomAttributes(typeof(System.ServiceModel.OperationContractAttribute), false)[0]).Name)
                .ToArray();
            CollectionAssert.AreEquivalent(esperadas, reales);
        }

        [TestMethod]
        public void ElContratoUsaElEspacioDeNombresYNombreDelServicio()
        {
            System.ServiceModel.ServiceContractAttribute atributo = (System.ServiceModel.ServiceContractAttribute)
                typeof(IWSEUREKABANK).GetCustomAttributes(typeof(System.ServiceModel.ServiceContractAttribute), false)[0];
            Assert.AreEqual("http://ws.monster.edu.ec/", atributo.Namespace);
            Assert.AreEqual("WSEUREKABANK", atributo.Name);
        }

        // Una excepción inesperada nunca debe llegar al cliente: responde ERR-001 con su mensaje literal.
        private WSEUREKABANK FachadaQueFalla()
        {
            ExplotaEnTodo falla = new ExplotaEnTodo();
            TokenServicio tokens = entorno.Tokens;
            return new WSEUREKABANK(
                new ServicioAutenticacion(falla, entorno.Base, tokens),
                new ServicioCuentas(falla, falla, tokens),
                new ServicioMovimientos(falla, falla, falla, falla, falla, falla, falla, tokens),
                new ServicioCostos(falla, falla, falla, falla, tokens));
        }

        private static void ComprobarErr001(Retorno r, int camposEsperadosMinimos)
        {
            Assert.AreEqual("ERR-001", Codigo(r));
            Assert.AreEqual("ERROR", r.Campos.Single(c => c.LocalName == "estado").InnerText);
            Assert.AreEqual("Ocurrió un error interno, intente nuevamente", r.Campos.Single(c => c.LocalName == "mensaje").InnerText);
            Assert.IsTrue(r.Campos.Length >= camposEsperadosMinimos);
        }

        [TestMethod]
        public void Autenticar_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().Autenticar(new SolicitudAutenticar("cromero", "x")).Resultado, 3);
        }

        [TestMethod]
        public void ListarCuentas_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("06914897", token)).Resultado, 3);
        }

        [TestMethod]
        public void ConsultarCuenta_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().ConsultarCuenta(new SolicitudConsultarCuenta("00200002", token)).Resultado, 3);
        }

        [TestMethod]
        public void ConsultarMovimientos_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().ConsultarMovimientos(new SolicitudConsultarMovimientos("00200002", token)).Resultado, 3);
        }

        [TestMethod]
        public void Deposito_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().RealizarDeposito(new SolicitudRealizarDeposito("00200002", 5m, token)).Resultado, 6);
        }

        [TestMethod]
        public void Retiro_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().RealizarRetiro(new SolicitudRealizarRetiro("00200002", 5m, token)).Resultado, 6);
        }

        [TestMethod]
        public void Transferencia_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().RealizarTransferencia(new SolicitudRealizarTransferencia("00200002", "00200001", 5m, token)).Resultado, 8);
        }

        [TestMethod]
        public void Costos_ConFalloInesperado_EsErr001()
        {
            ComprobarErr001(FachadaQueFalla().ConsultarCostosMantenimiento(new SolicitudConsultarCostosMantenimiento("00200002", token)).Resultado, 9);
        }

        [TestMethod]
        public void CerrarSesion_ConFalloInesperado_EsErr001()
        {
            string t = entorno.Tokens.GenerarToken("0001");
            ExplotaEnTodo falla = new ExplotaEnTodo();
            WSEUREKABANK fachada = new WSEUREKABANK(
                new ServicioAutenticacion(entorno.Base, falla, entorno.Tokens),
                new ServicioCuentas(entorno.Base, entorno.Base, entorno.Tokens),
                new ServicioMovimientos(entorno.Base, entorno.Base, entorno.Base, entorno.Base, entorno.Base, entorno.Base, entorno.Base, entorno.Tokens),
                new ServicioCostos(entorno.Base, entorno.Base, entorno.Base, entorno.Base, entorno.Tokens));
            ComprobarErr001(fachada.CerrarSesion(new SolicitudCerrarSesion(t)).Resultado, 3);
        }

        [TestMethod]
        public void Constructor_RechazaNulos()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new WSEUREKABANK(null, entorno.Cuentas, entorno.Movimientos, entorno.Costos));
            Assert.ThrowsException<ArgumentNullException>(() => new WSEUREKABANK(entorno.Autenticacion, null, entorno.Movimientos, entorno.Costos));
            Assert.ThrowsException<ArgumentNullException>(() => new WSEUREKABANK(entorno.Autenticacion, entorno.Cuentas, null, entorno.Costos));
            Assert.ThrowsException<ArgumentNullException>(() => new WSEUREKABANK(entorno.Autenticacion, entorno.Cuentas, entorno.Movimientos, null));
        }

        // ---- Forma del XML contra el XSD congelado ----

        private static XmlSchemaSet CargarEsquema()
        {
            string carpeta = AppDomain.CurrentDomain.BaseDirectory;
            while (carpeta != null && !File.Exists(Path.Combine(carpeta, "contrato", "WSEUREKABANK_schema.xsd")))
            {
                carpeta = Path.GetDirectoryName(carpeta);
            }
            Assert.IsNotNull(carpeta, "no se encontró contrato\\WSEUREKABANK_schema.xsd");
            XmlSchemaSet conjunto = new XmlSchemaSet();
            conjunto.Add("http://ws.monster.edu.ec/", Path.Combine(carpeta, "contrato", "WSEUREKABANK_schema.xsd"));
            conjunto.Compile();
            return conjunto;
        }

        private static void ValidarContraEsquema(string operacion, Retorno r)
        {
            XNamespace ws = "http://ws.monster.edu.ec/";
            XElement retorno = new XElement("return", r.Campos.Select(c => XElement.Parse(c.OuterXml)));
            XElement envoltura = new XElement(ws + (operacion + "Response"), retorno);
            XDocument documento = new XDocument(envoltura);
            List<string> errores = new List<string>();
            documento.Validate(CargarEsquema(), (s, e) => errores.Add(e.Message));
            Assert.AreEqual(0, errores.Count, operacion + ": " + string.Join(" | ", errores));
        }

        [TestMethod]
        public void RespuestaDeAutenticacion_CumpleElXsdCongelado()
        {
            ValidarContraEsquema("autenticar", entorno.Fachada.Autenticar(new SolicitudAutenticar("cromero", "Cromero2026")).Resultado);
        }

        [TestMethod]
        public void RespuestaDeAutenticacionFallida_CumpleElXsd()
        {
            ValidarContraEsquema("autenticar", entorno.Fachada.Autenticar(new SolicitudAutenticar("x", "y")).Resultado);
        }

        [TestMethod]
        public void RespuestaDeCierreDeSesion_CumpleElXsd()
        {
            ValidarContraEsquema("cerrarSesion", entorno.Fachada.CerrarSesion(new SolicitudCerrarSesion(token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeListaDeCuentas_CumpleElXsd()
        {
            ValidarContraEsquema("listarCuentasPorCliente", entorno.Fachada.ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("06914897", token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeListaDeCuentasConError_CumpleElXsd()
        {
            ValidarContraEsquema("listarCuentasPorCliente", entorno.Fachada.ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("1", token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeCuenta_CumpleElXsd()
        {
            ValidarContraEsquema("consultarCuenta", entorno.Fachada.ConsultarCuenta(new SolicitudConsultarCuenta("00200002", token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeMovimientos_CumpleElXsd()
        {
            entorno.Fachada.RealizarDeposito(new SolicitudRealizarDeposito("00200002", 10m, token));
            ValidarContraEsquema("consultarMovimientos", entorno.Fachada.ConsultarMovimientos(new SolicitudConsultarMovimientos("00200002", token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeDeposito_CumpleElXsd()
        {
            ValidarContraEsquema("realizarDeposito", entorno.Fachada.RealizarDeposito(new SolicitudRealizarDeposito("00200002", 10m, token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeDepositoConError_CumpleElXsd()
        {
            ValidarContraEsquema("realizarDeposito", entorno.Fachada.RealizarDeposito(new SolicitudRealizarDeposito("00200002", -1m, token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeRetiro_CumpleElXsd()
        {
            ValidarContraEsquema("realizarRetiro", entorno.Fachada.RealizarRetiro(new SolicitudRealizarRetiro("00200002", 10m, token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeTransferencia_CumpleElXsd()
        {
            ValidarContraEsquema("realizarTransferencia", entorno.Fachada.RealizarTransferencia(new SolicitudRealizarTransferencia("00200002", "00200001", 100m, token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeTransferenciaConError_CumpleElXsd()
        {
            ValidarContraEsquema("realizarTransferencia", entorno.Fachada.RealizarTransferencia(new SolicitudRealizarTransferencia("1", "2", 100m, token)).Resultado);
        }

        [TestMethod]
        public void RespuestaDeCostos_CumpleElXsd()
        {
            ValidarContraEsquema("consultarCostosMantenimiento", entorno.Fachada.ConsultarCostosMantenimiento(new SolicitudConsultarCostosMantenimiento("00200002", token)).Resultado);
        }

        [TestMethod]
        public void LosCamposDelSobreVanEnElOrdenDelContrato()
        {
            Retorno r = entorno.Fachada.Autenticar(new SolicitudAutenticar("cromero", "Cromero2026")).Resultado;
            CollectionAssert.AreEqual(
                new[] { "codigo", "estado", "mensaje", "empleado", "expiracion", "nombreEmpleado", "rol", "token" },
                r.Campos.Select(c => c.LocalName).ToArray());
        }

        [TestMethod]
        public void LosCamposDeUnaCuentaVanEnOrdenAlfabetico()
        {
            Retorno r = entorno.Fachada.ConsultarCuenta(new SolicitudConsultarCuenta("00200002", token)).Resultado;
            XElement cuenta = XElement.Parse(r.Campos.Single(c => c.LocalName == "cuenta").OuterXml);
            CollectionAssert.AreEqual(
                new[] { "codigo", "codigoCliente", "codigoSucursal", "descripcionMoneda", "estado", "fechaCreacion", "moneda", "nombreCliente", "operacionesRealizadas", "saldo" },
                cuenta.Elements().Select(e => e.Name.LocalName).ToArray());
        }

        [TestMethod]
        public void LosHijosDeReturnNoLlevanEspacioDeNombres()
        {
            Retorno r = entorno.Fachada.Autenticar(new SolicitudAutenticar("cromero", "Cromero2026")).Resultado;
            Assert.IsTrue(r.Campos.All(c => c.NamespaceURI == ""));
        }

        [TestMethod]
        public void LosImportesViajanConDosDecimales()
        {
            Retorno r = entorno.Fachada.RealizarDeposito(new SolicitudRealizarDeposito("00200002", 10m, token)).Resultado;
            Assert.AreEqual("5010.00", r.Campos.Single(c => c.LocalName == "saldoNuevo").InnerText);
            Assert.AreEqual("0.00", r.Campos.Single(c => c.LocalName == "itfAplicado").InnerText);
        }

        [TestMethod]
        public void UnaListaVaciaNoGeneraElementos()
        {
            Retorno r = entorno.Fachada.ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("00000000", token)).Resultado;
            Assert.IsFalse(r.Campos.Any(c => c.LocalName == "cuentas"));
        }

        [TestMethod]
        public void CadaCuentaEsUnElementoRepetido()
        {
            Retorno r = entorno.Fachada.ListarCuentasPorCliente(new SolicitudListarCuentasPorCliente("06914897", token)).Resultado;
            Assert.AreEqual(4, r.Campos.Count(c => c.LocalName == "cuentas"));
        }

        [TestMethod]
        public void LosTextosConTildesSobrevivenALaSerializacion()
        {
            Retorno r = entorno.Fachada.Autenticar(new SolicitudAutenticar("nadie", "x")).Resultado;
            Assert.AreEqual("El usuario o la contraseña son incorrectos", r.Campos.Single(c => c.LocalName == "mensaje").InnerText);
        }

        private class ExplotaEnTodo : IClienteDAO, ICuentaDAO, IMovimientoDAO, IUsuarioDAO, ILogSessionDAO,
            IParametroDAO, ITipoMovimientoDAO, IMonedaDAO, ICostoDAO, IUnidadTrabajo
        {
            private static Exception Falla()
            {
                return new InvalidOperationException("falla de la base de datos simulada");
            }

            public DatosCliente BuscarPorDni(string dni) { throw Falla(); }
            public List<CuentaDTO> ListarPorCliente(string codigoCliente) { throw Falla(); }
            public CuentaDTO BuscarPorCodigo(string codigo) { throw Falla(); }
            public CuentaDTO BloquearPorCodigo(ContextoTransaccional contexto, string codigo) { throw Falla(); }
            public void ActualizarSaldoYContador(ContextoTransaccional contexto, string codigo, decimal saldo, int contador) { throw Falla(); }
            public List<MovimientoDTO> ListarPorCuenta(string codigoCuenta) { throw Falla(); }
            public void Insertar(ContextoTransaccional contexto, string codigoCuenta, MovimientoDTO movimiento) { throw Falla(); }
            public DatosUsuario BuscarPorNombre(string nombreUsuario) { throw Falla(); }
            public int? RegistrarIngreso(string empleado, string ip, string hostname) { throw Falla(); }
            public void RegistrarSalida(int? idSesion) { throw Falla(); }
            public decimal? Valor(string codigo) { throw Falla(); }
            DatosTipoMovimiento ITipoMovimientoDAO.BuscarPorCodigo(string codigo) { throw Falla(); }
            public bool EstaSoportada(string codigoMoneda) { throw Falla(); }
            public DatosCostoMantenimiento CostoMantenimiento(string moneda) { throw Falla(); }
            public decimal? CostoMovimiento(string moneda) { throw Falla(); }
            public decimal? InteresMensual(string moneda) { throw Falla(); }
            public T Ejecutar<T>(Func<ContextoTransaccional, T> cuerpo) { throw Falla(); }
        }
    }
}
