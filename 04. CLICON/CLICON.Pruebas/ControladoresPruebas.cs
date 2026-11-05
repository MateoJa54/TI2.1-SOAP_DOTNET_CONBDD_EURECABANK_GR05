using System;
using System.Globalization;
using ec.edu.monster.consola.controlador;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.consola.pruebas
{
    [TestClass]
    public class ControladorAutenticacionPruebas
    {
        [TestMethod]
        public void Constructor_ClienteNuloLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorAutenticacion(null, new SesionUsuario()));
        }

        [TestMethod]
        public void Constructor_SesionNulaLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorAutenticacion(new ClienteFalso(), null));
        }

        [TestMethod]
        [DataRow("Cromero2026")]
        [DataRow("clave-secreta")]
        [DataRow("Ñandú#2026")]
        [DataRow("1234")]
        [DataRow("a")]
        [DataRow("  con espacios  ")]
        [DataRow("")]
        [DataRow("ClaveMuyLargaQueNoDebeViajarEnClaroNunca2026!!")]
        public void Iniciar_EnviaElHashNuncaLaClaveEnClaro(string clave)
        {
            ClienteFalso cliente = new ClienteFalso();
            ControladorAutenticacion controlador = new ControladorAutenticacion(cliente, new SesionUsuario());

            controlador.Iniciar("ana", clave);

            Assert.AreEqual(1, cliente.Llamadas.Count);
            Assert.AreEqual("ana", cliente.Ultima.Usuario);
            Assert.AreEqual(HashClave.Sha256(clave), cliente.Ultima.ClaveHash);
            Assert.AreNotEqual(clave, cliente.Ultima.ClaveHash);
        }

        [TestMethod]
        public void Iniciar_ClaveDeEjemploSeEnviaComoHashConocido()
        {
            ClienteFalso cliente = new ClienteFalso();
            new ControladorAutenticacion(cliente, new SesionUsuario()).Iniciar("ana", "Cromero2026");
            Assert.AreEqual("891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4", cliente.Ultima.ClaveHash);
        }

        [TestMethod]
        [DataRow("200", "tok-1", true)]
        [DataRow("200", "t", true)]
        [DataRow("200", "", false)]
        [DataRow("200", null, false)]
        [DataRow("401", "tok-1", false)]
        [DataRow("AUTH-001", "tok-1", false)]
        [DataRow("AUTH-002", "tok-1", false)]
        [DataRow("500", "tok-1", false)]
        [DataRow("SER-001", "tok-1", false)]
        [DataRow("", "tok-1", false)]
        public void Iniciar_GuardaSesionSoloConCodigo200YToken(string codigo, string token, bool esperadoVigente)
        {
            ClienteFalso cliente = new ClienteFalso();
            cliente.RespuestaAutenticar.Codigo = codigo;
            cliente.RespuestaAutenticar.Token = token;
            SesionUsuario sesion = new SesionUsuario();

            new ControladorAutenticacion(cliente, sesion).Iniciar("ana", "Cromero2026");

            Assert.AreEqual(esperadoVigente, sesion.EstaVigente);
        }

        [TestMethod]
        public void Iniciar_DevuelveLaRespuestaDelCliente()
        {
            ClienteFalso cliente = new ClienteFalso();
            cliente.RespuestaAutenticar.Codigo = "AUTH-001";
            cliente.RespuestaAutenticar.Mensaje = "Usuario o clave incorrectos";

            RespuestaAutenticacion r = new ControladorAutenticacion(cliente, new SesionUsuario()).Iniciar("ana", "x");

            Assert.AreEqual("AUTH-001", r.Codigo);
            Assert.AreEqual("Usuario o clave incorrectos", r.Mensaje);
        }

        [TestMethod]
        public void Iniciar_LoginCorrectoCopiaDatosALaSesion()
        {
            ClienteFalso cliente = new ClienteFalso();
            SesionUsuario sesion = new SesionUsuario();

            new ControladorAutenticacion(cliente, sesion).Iniciar("ana", "Cromero2026");

            Assert.AreEqual("tok-1", sesion.Token);
            Assert.AreEqual("E-001", sesion.Empleado);
            Assert.AreEqual("Ana Pérez", sesion.NombreEmpleado);
            Assert.AreEqual("Cajero", sesion.Rol);
        }

        [TestMethod]
        public void Iniciar_FalloNoTocaUnaSesionYaVigente()
        {
            ClienteFalso cliente = new ClienteFalso();
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "previo" });
            cliente.RespuestaAutenticar.Codigo = "AUTH-001";
            cliente.RespuestaAutenticar.Token = null;

            new ControladorAutenticacion(cliente, sesion).Iniciar("ana", "x");

            Assert.AreEqual("previo", sesion.Token);
        }

        [TestMethod]
        public void Cerrar_SinSesionNoLlamaAlClienteYDevuelveOk()
        {
            ClienteFalso cliente = new ClienteFalso();
            RespuestaBase r = new ControladorAutenticacion(cliente, new SesionUsuario()).Cerrar();

            Assert.AreEqual(0, cliente.Llamadas.Count);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("OK", r.Estado);
        }

        [TestMethod]
        public void Cerrar_ConSesionEnviaElTokenAlCliente()
        {
            ClienteFalso cliente = new ClienteFalso();
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "tok-cierre" });

            new ControladorAutenticacion(cliente, sesion).Cerrar();

            Assert.AreEqual(1, cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual("tok-cierre", cliente.Ultima.Token);
        }

        [TestMethod]
        public void Cerrar_DevuelveLaRespuestaDelCliente()
        {
            ClienteFalso cliente = new ClienteFalso();
            cliente.RespuestaCerrar = new RespuestaBase("ERROR", "SER-001", "Error interno");
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "tok" });

            RespuestaBase r = new ControladorAutenticacion(cliente, sesion).Cerrar();

            Assert.AreEqual("SER-001", r.Codigo);
            Assert.AreEqual("Error interno", r.Mensaje);
        }

        [TestMethod]
        [DataRow("200")]
        [DataRow("SER-001")]
        [DataRow("AUTH-003")]
        [DataRow("AUTH-004")]
        [DataRow("500")]
        [DataRow("")]
        public void Cerrar_SiemprePoneLaSesionEnCero(string codigoDelServidor)
        {
            ClienteFalso cliente = new ClienteFalso();
            cliente.RespuestaCerrar.Codigo = codigoDelServidor;
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "tok" });

            new ControladorAutenticacion(cliente, sesion).Cerrar();

            Assert.IsFalse(sesion.EstaVigente);
            Assert.IsNull(sesion.Token);
        }

        [TestMethod]
        public void Cerrar_RespuestaNulaDelClienteIgualmenteLimpiaLaSesion()
        {
            ClienteFalso cliente = new ClienteFalso();
            cliente.RespuestaCerrar = null;
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "tok" });

            RespuestaBase r = new ControladorAutenticacion(cliente, sesion).Cerrar();

            Assert.IsNull(r);
            Assert.IsFalse(sesion.EstaVigente);
        }
    }

    [TestClass]
    public class ControladoresProtegidosPruebas
    {
        [TestMethod]
        public void Constructores_ClienteNuloLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorCuentas(null, new SesionUsuario()));
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorMovimientos(null, new SesionUsuario()));
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorCostos(null, new SesionUsuario()));
        }

        [TestMethod]
        public void Constructores_SesionNulaLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorCuentas(new ClienteFalso(), null));
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorMovimientos(new ClienteFalso(), null));
            Assert.ThrowsException<ArgumentNullException>(() => new ControladorCostos(new ClienteFalso(), null));
        }

        [TestMethod]
        public void HaySesionActiva_FalsoSinSesionYVerdaderoConSesion()
        {
            Ambiente amb = new Ambiente();
            Assert.IsFalse(amb.Cuentas.HaySesionActiva);
            amb.IniciarSesionDirecta("tok-1");
            Assert.IsTrue(amb.Cuentas.HaySesionActiva);
            Assert.IsTrue(amb.Movimientos.HaySesionActiva);
            Assert.IsTrue(amb.Costos.HaySesionActiva);
        }

        [TestMethod]
        public void ConstantesDeCodigos_SonLasDelContrato()
        {
            Assert.AreEqual("AUTH-003", ControladorProtegido.CodigoTokenInvalido);
            Assert.AreEqual("AUTH-004", ControladorProtegido.CodigoTokenExpirado);
            Assert.AreEqual("El token de sesión no es válido", ControladorProtegido.MensajeTokenInvalido);
        }

        [TestMethod]
        [DataRow("Cuentas.Listar")]
        [DataRow("Cuentas.Consultar")]
        [DataRow("Movimientos.Consultar")]
        [DataRow("Movimientos.Depositar")]
        [DataRow("Movimientos.Retirar")]
        [DataRow("Movimientos.Transferir")]
        [DataRow("Costos.Consultar")]
        public void SinSesion_NoLlamaAlClienteYDevuelveAuth003(string operacion)
        {
            Ambiente amb = new Ambiente();

            RespuestaBase r = amb.Invocar(operacion);

            Assert.AreEqual(0, amb.Cliente.Llamadas.Count);
            Assert.AreEqual("AUTH-003", r.Codigo);
            Assert.AreEqual("El token de sesión no es válido", r.Mensaje);
            Assert.AreEqual("ERROR", r.Estado);
        }

        [TestMethod]
        [DataRow("Cuentas.Listar", "ListarCuentasPorCliente", "1712345678", null, null, null)]
        [DataRow("Cuentas.Consultar", "ConsultarCuenta", null, "CTA-1", null, null)]
        [DataRow("Movimientos.Consultar", "ConsultarMovimientos", null, "CTA-1", null, null)]
        [DataRow("Movimientos.Depositar", "RealizarDeposito", null, "CTA-1", "25.5", null)]
        [DataRow("Movimientos.Retirar", "RealizarRetiro", null, "CTA-1", "25.5", null)]
        [DataRow("Movimientos.Transferir", "RealizarTransferencia", null, "CTA-1", "25.5", "CTA-2")]
        [DataRow("Costos.Consultar", "ConsultarCostosMantenimiento", null, "CTA-1", null, null)]
        public void ConSesion_PasaTokenYDatosTalCual(string operacion, string metodo, string cedula, string cuenta, string monto, string destino)
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-xyz");

            amb.Invocar(operacion);

            Llamada l = amb.Cliente.Ultima;
            Assert.AreEqual(metodo, l.Metodo);
            Assert.AreEqual("tok-xyz", l.Token);
            Assert.AreEqual(cedula, l.Cedula);
            Assert.AreEqual(cuenta, l.Cuenta);
            Assert.AreEqual(destino, l.CuentaDestino);
            if (monto == null)
            {
                Assert.IsNull(l.Monto);
            }
            else
            {
                Assert.AreEqual(decimal.Parse(monto, CultureInfo.InvariantCulture), l.Monto.Value);
            }
        }

        [TestMethod]
        [DataRow("Cuentas.Listar", "AUTH-003")]
        [DataRow("Cuentas.Listar", "AUTH-004")]
        [DataRow("Cuentas.Consultar", "AUTH-003")]
        [DataRow("Cuentas.Consultar", "AUTH-004")]
        [DataRow("Movimientos.Consultar", "AUTH-003")]
        [DataRow("Movimientos.Consultar", "AUTH-004")]
        [DataRow("Movimientos.Depositar", "AUTH-003")]
        [DataRow("Movimientos.Depositar", "AUTH-004")]
        [DataRow("Movimientos.Retirar", "AUTH-003")]
        [DataRow("Movimientos.Retirar", "AUTH-004")]
        [DataRow("Movimientos.Transferir", "AUTH-003")]
        [DataRow("Movimientos.Transferir", "AUTH-004")]
        [DataRow("Costos.Consultar", "AUTH-003")]
        [DataRow("Costos.Consultar", "AUTH-004")]
        public void RespuestaAuth003o004_LimpiaLaSesion(string operacion, string codigo)
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");
            amb.Cliente.ResponderTodoCon(codigo, "Motivo del servidor");

            RespuestaBase r = amb.Invocar(operacion);

            Assert.AreEqual(codigo, r.Codigo);
            Assert.IsFalse(amb.Sesion.EstaVigente);
        }

        [TestMethod]
        [DataRow("Cuentas.Listar", "200")]
        [DataRow("Cuentas.Listar", "SER-001")]
        [DataRow("Cuentas.Listar", "AUTH-001")]
        [DataRow("Cuentas.Listar", "CTA-404")]
        [DataRow("Cuentas.Consultar", "200")]
        [DataRow("Cuentas.Consultar", "SER-001")]
        [DataRow("Cuentas.Consultar", "AUTH-001")]
        [DataRow("Cuentas.Consultar", "CTA-404")]
        [DataRow("Movimientos.Consultar", "200")]
        [DataRow("Movimientos.Consultar", "SER-001")]
        [DataRow("Movimientos.Consultar", "AUTH-001")]
        [DataRow("Movimientos.Consultar", "CTA-404")]
        [DataRow("Movimientos.Depositar", "200")]
        [DataRow("Movimientos.Depositar", "SER-001")]
        [DataRow("Movimientos.Depositar", "AUTH-001")]
        [DataRow("Movimientos.Depositar", "CTA-404")]
        [DataRow("Movimientos.Retirar", "200")]
        [DataRow("Movimientos.Retirar", "SER-001")]
        [DataRow("Movimientos.Retirar", "AUTH-001")]
        [DataRow("Movimientos.Retirar", "CTA-404")]
        [DataRow("Movimientos.Transferir", "200")]
        [DataRow("Movimientos.Transferir", "SER-001")]
        [DataRow("Movimientos.Transferir", "AUTH-001")]
        [DataRow("Movimientos.Transferir", "CTA-404")]
        [DataRow("Costos.Consultar", "200")]
        [DataRow("Costos.Consultar", "SER-001")]
        [DataRow("Costos.Consultar", "AUTH-001")]
        [DataRow("Costos.Consultar", "CTA-404")]
        public void OtroCodigo_ConservaLaSesion(string operacion, string codigo)
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");
            amb.Cliente.ResponderTodoCon(codigo, "Mensaje");

            RespuestaBase r = amb.Invocar(operacion);

            Assert.AreEqual(codigo, r.Codigo);
            Assert.IsTrue(amb.Sesion.EstaVigente);
            Assert.AreEqual("tok-1", amb.Sesion.Token);
        }

        [TestMethod]
        public void RespuestaNulaDelCliente_DevuelveNuloYConservaLaSesion()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");
            amb.Cliente.RespuestaListar = null;

            RespuestaCuentas r = amb.Cuentas.ListarPorCliente("1712345678");

            Assert.IsNull(r);
            Assert.IsTrue(amb.Sesion.EstaVigente);
        }

        [TestMethod]
        public void ListarPorCliente_DevuelveLasCuentasDelCliente()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaCuentas r = amb.Cuentas.ListarPorCliente("1712345678");

            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Juan Ruiz", r.NombreCliente);
        }

        [TestMethod]
        public void Consultar_CuentaDevuelveElDetalleDelCliente()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaCuenta r = amb.Cuentas.Consultar("CTA-1");

            Assert.IsNotNull(r.Cuenta);
            Assert.AreEqual("CTA-1", r.Cuenta.Codigo);
            Assert.AreEqual(100m, r.Cuenta.Saldo);
        }

        [TestMethod]
        public void Movimientos_ConsultarDevuelveLaRespuestaDelCliente()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaMovimientos r = amb.Movimientos.Consultar("CTA-1");

            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("CTA-1", r.Cuenta);
        }

        [TestMethod]
        public void Movimientos_DepositarDevuelveElMovimientoYSaldo()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaMovimiento r = amb.Movimientos.Depositar("CTA-1", 25.5m);

            Assert.AreEqual(7, r.Movimiento.Numero);
            Assert.AreEqual(125.5m, r.SaldoNuevo);
        }

        [TestMethod]
        public void Movimientos_RetirarDevuelveElMovimientoYSaldo()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaMovimiento r = amb.Movimientos.Retirar("CTA-1", 25.5m);

            Assert.AreEqual(8, r.Movimiento.Numero);
            Assert.AreEqual(90m, r.SaldoNuevo);
        }

        [TestMethod]
        public void Movimientos_TransferirDevuelveAmbasCuentas()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaTransferencia r = amb.Movimientos.Transferir("CTA-1", "CTA-2", 25.5m);

            Assert.AreEqual("CTA-1", amb.Cliente.Ultima.Cuenta);
            Assert.AreEqual("CTA-2", amb.Cliente.Ultima.CuentaDestino);
            Assert.AreEqual(9, r.NumeroOrigen);
            Assert.AreEqual(10, r.NumeroDestino);
        }

        [TestMethod]
        public void Costos_ConsultarDevuelveLosCostos()
        {
            Ambiente amb = new Ambiente();
            amb.IniciarSesionDirecta("tok-1");

            RespuestaCostos r = amb.Costos.Consultar("CTA-1");

            Assert.AreEqual(2m, r.CostoMantenimiento);
            Assert.AreEqual(5, r.OperacionesSinCosto);
        }
    }
}
