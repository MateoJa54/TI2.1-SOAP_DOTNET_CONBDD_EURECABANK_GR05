using System;
using ec.edu.monster.modelos;
using ec.edu.monster.pruebas.Apoyo;
using ec.edu.monster.servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.servicios
{
    [TestClass]
    public class ServicioCuentasTest
    {
        private Entorno entorno;
        private string token;

        [TestInitialize]
        public void Preparar()
        {
            entorno = new Entorno();
            token = entorno.Entrar();
        }

        [TestMethod]
        public void Listar_DevuelveTodasLasCuentasIncluidasLasNoActivas()
        {
            RespuestaCuentas r = entorno.Cuentas.ListarCuentasPorCliente("06914897", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Cuentas consultadas correctamente", r.Mensaje);
            Assert.AreEqual("06914897", r.Cedula);
            Assert.AreEqual("Ricardo Gomez Torres", r.NombreCliente);
            Assert.AreEqual(4, r.Cuentas.Count);
            Assert.IsTrue(r.Cuentas.Exists(c => c.Estado == "CANCELADO"));
        }

        [TestMethod]
        public void Listar_ClienteSinCuentas_DevuelveListaVacia()
        {
            RespuestaCuentas r = entorno.Cuentas.ListarCuentasPorCliente("00000000", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0, r.Cuentas.Count);
        }

        [TestMethod]
        public void Listar_RecortaLaCedula()
        {
            Assert.AreEqual("06914897", entorno.Cuentas.ListarCuentasPorCliente(" 06914897 ", token).Cedula);
        }

        [TestMethod]
        public void Listar_ClienteInexistente_EsCli001()
        {
            RespuestaCuentas r = entorno.Cuentas.ListarCuentasPorCliente("99999999", token);
            Assert.AreEqual("CLI-001", r.Codigo);
            Assert.AreEqual("El cliente solicitado no existe", r.Mensaje);
            Assert.AreEqual(0, r.Cuentas.Count);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Listar_CedulaVacia_EsDat001(string cedula)
        {
            Assert.AreEqual("DAT-001", entorno.Cuentas.ListarCuentasPorCliente(cedula, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("", "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void Listar_TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, entorno.Cuentas.ListarCuentasPorCliente("06914897", tokenMalo).Codigo);
        }

        [TestMethod]
        public void Listar_TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(31));
            Assert.AreEqual("AUTH-004", entorno.Cuentas.ListarCuentasPorCliente("06914897", token).Codigo);
        }

        [TestMethod]
        public void Listar_ElTokenGanaSobreLaCedulaVacia()
        {
            Assert.AreEqual("AUTH-003", entorno.Cuentas.ListarCuentasPorCliente("", "falso").Codigo);
        }

        [TestMethod]
        public void Consultar_CuentaActiva()
        {
            RespuestaCuenta r = entorno.Cuentas.ConsultarCuenta("00200002", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Cuenta consultada correctamente", r.Mensaje);
            Assert.AreEqual("00200002", r.Cuenta.Codigo);
            Assert.AreEqual(5000m, r.Cuenta.Saldo);
            Assert.AreEqual("01", r.Cuenta.Moneda);
        }

        [TestMethod]
        public void Consultar_Inexistente_EsCta001()
        {
            Assert.AreEqual("CTA-001", entorno.Cuentas.ConsultarCuenta("11111111", token).Codigo);
        }

        [TestMethod]
        public void Consultar_Cancelada_EsCta002()
        {
            RespuestaCuenta r = entorno.Cuentas.ConsultarCuenta("00300001", token);
            Assert.AreEqual("CTA-002", r.Codigo);
            Assert.AreEqual("La cuenta no se encuentra en estado ACTIVO", r.Mensaje);
            Assert.IsNull(r.Cuenta);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void Consultar_CuentaVacia_EsDat001(string cuenta)
        {
            Assert.AreEqual("DAT-001", entorno.Cuentas.ConsultarCuenta(cuenta, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void Consultar_TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, entorno.Cuentas.ConsultarCuenta("00200002", tokenMalo).Codigo);
        }

        [TestMethod]
        public void Consultar_TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromHours(1));
            Assert.AreEqual("AUTH-004", entorno.Cuentas.ConsultarCuenta("00200002", token).Codigo);
        }

        [TestMethod]
        public void Constructor_RechazaNulos()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCuentas(null, entorno.Base, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCuentas(entorno.Base, null, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCuentas(entorno.Base, entorno.Base, null));
        }
    }

    [TestClass]
    public class ServicioCostosTest
    {
        private Entorno entorno;
        private string token;

        [TestInitialize]
        public void Preparar()
        {
            entorno = new Entorno();
            token = entorno.Entrar();
        }

        [TestMethod]
        public void SaldoPorEncimaDelLimite_NoCobraMantenimiento()
        {
            RespuestaCostos r = entorno.Costos.ConsultarCostosMantenimiento("00200002", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Costos de mantenimiento consultados correctamente", r.Mensaje);
            Assert.AreEqual(5000.00m, r.SaldoActual);
            Assert.AreEqual(0.00m, r.CostoMantenimiento);
            Assert.AreEqual(2.00m, r.CostoMovimiento);
            Assert.AreEqual(35.00m, r.InteresProyectado);
            Assert.AreEqual(3500.00m, r.LimiteMantenimiento);
            Assert.AreEqual(15, r.OperacionesSinCosto);
        }

        [TestMethod]
        public void SaldoPorDebajoDelLimite_CobraMantenimiento()
        {
            RespuestaCostos r = entorno.Costos.ConsultarCostosMantenimiento("00200001", token);
            Assert.AreEqual(7.00m, r.CostoMantenimiento);
            Assert.AreEqual(0.70m, r.InteresProyectado);
        }

        [TestMethod]
        public void SaldoIgualAlLimite_CobraMantenimiento()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 3500m;
            Assert.AreEqual(7.00m, entorno.Costos.ConsultarCostosMantenimiento("00200001", token).CostoMantenimiento);
        }

        [TestMethod]
        public void SaldoUnCentavoPorEncimaDelLimite_NoCobra()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 3500.01m;
            Assert.AreEqual(0.00m, entorno.Costos.ConsultarCostosMantenimiento("00200001", token).CostoMantenimiento);
        }

        [TestMethod]
        public void CuentaEnDolares_UsaLosCostosDeSuMoneda()
        {
            RespuestaCostos r = entorno.Costos.ConsultarCostosMantenimiento("00100002", token);
            Assert.AreEqual(2.50m, r.CostoMantenimiento);
            Assert.AreEqual(0.60m, r.CostoMovimiento);
            Assert.AreEqual(4.00m, r.InteresProyectado);
            Assert.AreEqual(1000.00m, r.LimiteMantenimiento);
        }

        [TestMethod]
        public void Interes_RedondeaHalfUp()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 0.5m;
            Assert.AreEqual(0.00m, entorno.Costos.ConsultarCostosMantenimiento("00200001", token).InteresProyectado);
            entorno.Base.Cuentas["00200001"].Saldo = 0.72m;
            Assert.AreEqual(0.01m, entorno.Costos.ConsultarCostosMantenimiento("00200001", token).InteresProyectado);
        }

        [TestMethod]
        public void SinParametroDeOperaciones_DevuelveCero()
        {
            entorno.Base.Parametros.Remove("002");
            Assert.AreEqual(0, entorno.Costos.ConsultarCostosMantenimiento("00200002", token).OperacionesSinCosto);
        }

        [TestMethod]
        public void SinFilasDeCosto_DevuelveCeros()
        {
            entorno.Base.Mantenimientos.Clear();
            entorno.Base.CostosMovimiento.Clear();
            entorno.Base.Intereses.Clear();
            RespuestaCostos r = entorno.Costos.ConsultarCostosMantenimiento("00200002", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.CostoMantenimiento);
            Assert.AreEqual(0.00m, r.CostoMovimiento);
            Assert.AreEqual(0.00m, r.InteresProyectado);
            Assert.AreEqual(0.00m, r.LimiteMantenimiento);
        }

        [TestMethod]
        public void CuentaInexistente_EsCta001()
        {
            Assert.AreEqual("CTA-001", entorno.Costos.ConsultarCostosMantenimiento("11111111", token).Codigo);
        }

        [TestMethod]
        public void CuentaCancelada_EsCta002()
        {
            Assert.AreEqual("CTA-002", entorno.Costos.ConsultarCostosMantenimiento("00300001", token).Codigo);
        }

        [TestMethod]
        public void MonedaNoSoportada_EsMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("MON-002", entorno.Costos.ConsultarCostosMantenimiento("00200002", token).Codigo);
        }

        [TestMethod]
        public void MonedaNoSoportada_PierdeContraCuentaCancelada()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("CTA-002", entorno.Costos.ConsultarCostosMantenimiento("00300001", token).Codigo);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(" ")]
        public void CuentaVacia_EsDat001(string cuenta)
        {
            Assert.AreEqual("DAT-001", entorno.Costos.ConsultarCostosMantenimiento(cuenta, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, entorno.Costos.ConsultarCostosMantenimiento("00200002", tokenMalo).Codigo);
        }

        [TestMethod]
        public void TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(30));
            Assert.AreEqual("AUTH-004", entorno.Costos.ConsultarCostosMantenimiento("00200002", token).Codigo);
        }

        [TestMethod]
        public void NoEscribeNada()
        {
            entorno.Costos.ConsultarCostosMantenimiento("00200001", token);
            Assert.AreEqual(0, entorno.Base.EjecucionesDeUnidad);
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
        }

        [TestMethod]
        public void Constructor_RechazaNulos()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCostos(null, entorno.Base, entorno.Base, entorno.Base, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCostos(entorno.Base, null, entorno.Base, entorno.Base, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCostos(entorno.Base, entorno.Base, null, entorno.Base, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCostos(entorno.Base, entorno.Base, entorno.Base, null, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioCostos(entorno.Base, entorno.Base, entorno.Base, entorno.Base, null));
        }
    }
}
