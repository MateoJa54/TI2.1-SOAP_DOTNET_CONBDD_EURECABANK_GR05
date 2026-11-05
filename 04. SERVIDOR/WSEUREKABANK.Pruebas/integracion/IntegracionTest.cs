using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ec.edu.monster.controladores;
using ec.edu.monster.datos;
using ec.edu.monster.modelos;
using ec.edu.monster.servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.integracion
{
    // Corren contra el esquema eurekabank_prueba (03. BDD\crear_bdd_prueba.ps1), nunca contra eurekabank.
    // Escriben saldos y logsession: para volver a la semilla se ejecuta otra vez crear_bdd_prueba.ps1.
    [TestClass]
    [TestCategory("Integracion")]
    public class IntegracionTest
    {
        private const string Esquema = "eurekabank_prueba";
        private ProveedorConexion proveedor;
        private RaizComposicion raiz;
        private string token;

        [TestInitialize]
        public void Preparar()
        {
            proveedor = new ProveedorConexion("127.0.0.1", 3306, Esquema, "eurekabank_app", "EurekaBank2026!");
            raiz = RaizComposicion.Crear(proveedor, TimeSpan.FromMinutes(30));
            token = raiz.Autenticacion.Autenticar("cromero", "Cromero2026").Token;
            Assert.IsNotNull(token, "no se pudo iniciar sesión: ¿existe el esquema " + Esquema + "?");
        }

        [TestMethod]
        public void EsteSuiteNuncaApuntaALaBaseReal()
        {
            Assert.AreEqual("eurekabank_prueba", proveedor.Esquema);
        }

        [TestMethod]
        public void LaBaseDePruebaTieneLasTablasYLaSemilla()
        {
            Assert.IsNotNull(new ClienteDAO(proveedor).BuscarPorDni("06914897"));
            Assert.AreEqual(1, new CuentaDAO(proveedor).ListarPorCliente("00001").Count(c => c.Codigo == "00200002"));
        }

        [TestMethod]
        public void ParametrosYCatalogos()
        {
            Assert.AreEqual(0.08m, new ParametroDAO(proveedor).Valor("001"));
            Assert.AreEqual(15m, new ParametroDAO(proveedor).Valor("002"));
            Assert.IsNull(new ParametroDAO(proveedor).Valor("999"));
            Assert.AreEqual("INGRESO", new TipoMovimientoDAO(proveedor).BuscarPorCodigo("003").Accion);
            Assert.IsNull(new TipoMovimientoDAO(proveedor).BuscarPorCodigo("999"));
            Assert.IsTrue(new MonedaDAO(proveedor).EstaSoportada("01"));
            Assert.IsFalse(new MonedaDAO(proveedor).EstaSoportada("99"));
            Assert.AreEqual(2.00m, new CostoDAO(proveedor).CostoMovimiento("01"));
            Assert.IsNotNull(new CostoDAO(proveedor).CostoMantenimiento("01"));
        }

        [TestMethod]
        public void LasConsultasSonParametrizadas_UnaInyeccionNoDevuelveFilas()
        {
            Assert.IsNull(new ClienteDAO(proveedor).BuscarPorDni("' OR '1'='1"));
            Assert.IsNull(new CuentaDAO(proveedor).BuscarPorCodigo("00200002' OR '1'='1"));
            Assert.IsNull(new UsuarioDAO(proveedor).BuscarPorNombre("cromero' --"));
        }

        [TestMethod]
        public void CuentaCanceladaYSusDatos()
        {
            CuentaDTO c = new CuentaDAO(proveedor).BuscarPorCodigo("00300001");
            Assert.AreEqual("CANCELADO", c.Estado);
            Assert.IsNotNull(c.FechaCreacion);
        }

        [TestMethod]
        public void DepositoReal_PersisteSaldoContadorYMovimiento()
        {
            CuentaDAO dao = new CuentaDAO(proveedor);
            CuentaDTO antes = dao.BuscarPorCodigo("00200001");
            int movimientosAntes = new MovimientoDAO(proveedor).ListarPorCuenta("00200001").Count;
            RespuestaMovimiento r = raiz.Movimientos.RealizarDeposito("00200001", 12.34m, token);
            Assert.AreEqual("200", r.Codigo);
            CuentaDTO despues = dao.BuscarPorCodigo("00200001");
            Assert.AreEqual(antes.Saldo + 12.34m - r.CostoAplicado, despues.Saldo);
            Assert.AreEqual(antes.OperacionesRealizadas + (r.CostoAplicado > 0 ? 2 : 1), despues.OperacionesRealizadas);
            Assert.AreEqual(movimientosAntes + (r.CostoAplicado > 0 ? 2 : 1), new MovimientoDAO(proveedor).ListarPorCuenta("00200001").Count);
            Assert.AreEqual(r.SaldoNuevo, despues.Saldo);
        }

        [TestMethod]
        public void TransferenciaReal_ConservaElDineroMenosItfYCargos()
        {
            CuentaDAO dao = new CuentaDAO(proveedor);
            decimal total = dao.BuscarPorCodigo("00200001").Saldo + dao.BuscarPorCodigo("00200002").Saldo;
            RespuestaTransferencia r = raiz.Movimientos.RealizarTransferencia("00200002", "00200001", 50m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.04m, r.ItfAplicado);
            decimal despues = dao.BuscarPorCodigo("00200001").Saldo + dao.BuscarPorCodigo("00200002").Saldo;
            decimal cargos = total - despues - r.ItfAplicado;
            Assert.IsTrue(cargos >= 0m && (cargos == 0m || cargos == 2m || cargos == 4m), "cargos=" + cargos);
            Assert.AreEqual(r.SaldoOrigen, dao.BuscarPorCodigo("00200002").Saldo);
            Assert.AreEqual(r.SaldoDestino, dao.BuscarPorCodigo("00200001").Saldo);
        }

        [TestMethod]
        public void RetiroConSaldoInsuficiente_NoEscribeNada()
        {
            CuentaDAO dao = new CuentaDAO(proveedor);
            CuentaDTO antes = dao.BuscarPorCodigo("00200001");
            RespuestaMovimiento r = raiz.Movimientos.RealizarRetiro("00200001", antes.Saldo + 1m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            CuentaDTO despues = dao.BuscarPorCodigo("00200001");
            Assert.AreEqual(antes.Saldo, despues.Saldo);
            Assert.AreEqual(antes.OperacionesRealizadas, despues.OperacionesRealizadas);
        }

        [TestMethod]
        public void Rollback_SiFallaUnaInsercionNoQuedaNadaParcial()
        {
            CuentaDAO dao = new CuentaDAO(proveedor);
            CuentaDTO antes = dao.BuscarPorCodigo("00200001");
            UnidadTrabajoMySql unidad = new UnidadTrabajoMySql(proveedor);
            MovimientoDAO movimientos = new MovimientoDAO(proveedor);
            Assert.ThrowsException<MySqlConnector.MySqlException>(() => unidad.Ejecutar<int>(contexto =>
            {
                dao.BloquearPorCodigo(contexto, "00200001");
                dao.ActualizarSaldoYContador(contexto, "00200001", antes.Saldo + 999m, antes.OperacionesRealizadas + 1);
                // tipo de movimiento inexistente: viola la clave foránea
                movimientos.Insertar(contexto, "00200001", new MovimientoDTO("0001", "2026-11-03", 1m,
                    antes.OperacionesRealizadas + 1, null, "INGRESO", "999", "x"));
                return 0;
            }));
            CuentaDTO despues = dao.BuscarPorCodigo("00200001");
            Assert.AreEqual(antes.Saldo, despues.Saldo);
            Assert.AreEqual(antes.OperacionesRealizadas, despues.OperacionesRealizadas);
        }

        [TestMethod]
        public void AutenticarYCerrarSesion_RegistranLogsession()
        {
            RespuestaAutenticacion a = raiz.Autenticacion.Autenticar("lcastro", "Castro2026");
            Assert.AreEqual("200", a.Codigo);
            Assert.AreEqual("200", raiz.Autenticacion.CerrarSesion(a.Token).Codigo);
            using (MySqlConnector.MySqlConnection c = proveedor.ObtenerConexion())
            using (MySqlConnector.MySqlCommand cmd = new MySqlConnector.MySqlCommand(
                "SELECT COUNT(*) FROM logsession WHERE chr_emplcodigo='0002' AND fec_salida IS NOT NULL", c))
            {
                Assert.IsTrue(Convert.ToInt32(cmd.ExecuteScalar()) >= 1);
            }
        }

        [TestMethod]
        public void UsuariosConHashSha1HeredadoOAnulados()
        {
            Assert.AreEqual("AUTH-001", raiz.Autenticacion.Autenticar("cvalencia", "lo que sea").Codigo);
            Assert.AreEqual("AUTH-002", raiz.Autenticacion.Autenticar("csarmiento", "Sarmiento2026").Codigo);
        }

        [TestMethod]
        public void ConcurrenciaReal_DosRetirosPorTodoElSaldo_SoloUnoProcede()
        {
            CuentaDAO dao = new CuentaDAO(proveedor);
            // Se deja la cuenta destino de prueba con un saldo conocido mediante un depósito y se retira justo ese total.
            raiz.Movimientos.RealizarDeposito("00100002", 500m, token);
            CuentaDTO c = dao.BuscarPorCodigo("00100002");
            decimal monto = c.Saldo - 1m;
            Task<RespuestaMovimiento>[] tareas =
            {
                Task.Run(() => raiz.Movimientos.RealizarRetiro("00100002", monto, token)),
                Task.Run(() => raiz.Movimientos.RealizarRetiro("00100002", monto, token))
            };
            Task.WaitAll(tareas);
            List<string> codigos = tareas.Select(t => t.Result.Codigo).OrderBy(x => x).ToList();
            CollectionAssert.AreEqual(new[] { "200", "SAL-001" }, codigos);
            Assert.IsTrue(dao.BuscarPorCodigo("00100002").Saldo >= 0m);
        }

        [TestMethod]
        public void ConcurrenciaReal_TransferenciasCruzadas_NoSeBloqueanEntreSi()
        {
            Task[] tareas = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
            {
                RespuestaTransferencia r = i % 2 == 0
                    ? raiz.Movimientos.RealizarTransferencia("00200002", "00200001", 1m, token)
                    : raiz.Movimientos.RealizarTransferencia("00200001", "00200002", 1m, token);
                Assert.AreEqual("200", r.Codigo);
            })).ToArray();
            Assert.IsTrue(Task.WaitAll(tareas, TimeSpan.FromSeconds(60)), "posible interbloqueo");
        }

        [TestMethod]
        public void LosNumerosDeMovimientoSonCorrelativosYSinHuecos()
        {
            List<MovimientoDTO> m = new MovimientoDAO(proveedor).ListarPorCuenta("00200002");
            CollectionAssert.AreEqual(Enumerable.Range(1, m.Count).ToList(), m.Select(x => x.Numero).ToList());
        }

        [TestMethod]
        public void LaFachadaCompleta_FuncionaConLaBaseReal()
        {
            WSEUREKABANK fachada = new WSEUREKABANK(raiz.Autenticacion, raiz.Cuentas, raiz.Movimientos, raiz.Costos);
            Assert.IsNotNull(fachada.ConsultarCuenta(new ec.edu.monster.controladores.mensajes.SolicitudConsultarCuenta("00200002", token)).Resultado);
        }
    }
}
