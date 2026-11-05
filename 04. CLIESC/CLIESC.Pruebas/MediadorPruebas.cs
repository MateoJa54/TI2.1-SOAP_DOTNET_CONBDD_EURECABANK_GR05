using System;
using System.Globalization;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    [TestClass]
    public class MediadorPruebas
    {
        private ClienteFalso _cliente;
        private MediadorEscritorio _mediador;

        [TestInitialize]
        public void Preparar()
        {
            _cliente = new ClienteFalso();
            _mediador = new MediadorEscritorio(_cliente, "http://localhost:50775/WSEUREKABANK.svc");
        }

        private void IniciarSesion()
        {
            RespuestaAutenticacion respuesta = _mediador.Autenticacion.Iniciar("ana", "Cromero2026");
            Assert.IsTrue(_mediador.Sesion.EstaVigente, "La prueba necesita una sesión; respuesta: " + respuesta?.Codigo);
        }

        [TestMethod]
        public void Constructor_ClienteNuloLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MediadorEscritorio(null, "x"));
        }

        [TestMethod]
        public void Constructor_UrlNulaSeGuardaVacia()
        {
            Assert.AreEqual(string.Empty, new MediadorEscritorio(_cliente, null).Url);
        }

        [TestMethod]
        public void Constructor_GuardaLaUrlRecibida()
        {
            Assert.AreEqual("http://localhost:50775/WSEUREKABANK.svc", _mediador.Url);
        }

        [TestMethod]
        public void Constructor_CreaTodosLosControladoresYLaNavegacion()
        {
            Assert.IsNotNull(_mediador.Sesion);
            Assert.IsNotNull(_mediador.Autenticacion);
            Assert.IsNotNull(_mediador.Cuentas);
            Assert.IsNotNull(_mediador.Movimientos);
            Assert.IsNotNull(_mediador.Costos);
            Assert.IsNotNull(_mediador.Navegacion);
        }

        [TestMethod]
        public void Inicio_NoHaySesionYSeMuestraElAcceso()
        {
            Assert.IsFalse(_mediador.Sesion.EstaVigente);
            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
        }

        [TestMethod]
        public void SesionIniciada_SinSesionNoNavega()
        {
            Assert.IsFalse(_mediador.SesionIniciada());
            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
        }

        [TestMethod]
        public void SesionIniciada_ConSesionPasaAPrincipal()
        {
            IniciarSesion();

            Assert.IsTrue(_mediador.SesionIniciada());
            Assert.AreEqual(Pantalla.Principal, _mediador.Navegacion.Actual);
        }

        [TestMethod]
        public void Autenticar_CredencialesMalasNoAbrenLaPrincipal()
        {
            _cliente.ResponderTodoCon("AUTH-001", "Usuario o clave incorrectos");
            _cliente.RespuestaAutenticar.Token = null;
            _cliente.RespuestaAutenticar.Codigo = "AUTH-001";

            RespuestaAutenticacion respuesta = _mediador.Autenticacion.Iniciar("ana", "mala");

            Assert.AreEqual("AUTH-001", respuesta.Codigo);
            Assert.IsFalse(_mediador.SesionIniciada());
            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
        }

        [TestMethod]
        public void SesionCerrada_VuelveAlAccesoConMensajeNoError()
        {
            IniciarSesion();
            _mediador.SesionIniciada();

            _mediador.SesionCerrada();

            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
            Assert.AreEqual(Presentacion.SesionCerrada, _mediador.Navegacion.MensajeAcceso);
            Assert.IsFalse(_mediador.Navegacion.MensajeEsError);
        }

        [TestMethod]
        public void CerrarSesion_LlamaAlServidorYLimpiaLaSesion()
        {
            IniciarSesion();

            _mediador.Autenticacion.Cerrar();

            Assert.AreEqual(1, _cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual("tok-1", _cliente.Ultima.Token);
            Assert.IsFalse(_mediador.Sesion.EstaVigente);
        }

        [TestMethod]
        public void CerrarSinSesion_NoLlamaAlServidor()
        {
            _mediador.Autenticacion.Cerrar();
            Assert.AreEqual(0, _cliente.Cuantas("CerrarSesion"));
        }

        [TestMethod]
        public void SinSesion_ListarCuentasNoLlamaAlServidorYPideLogin()
        {
            RespuestaCuentas respuesta = _mediador.Cuentas.ListarPorCliente("1712345678");

            Assert.AreEqual(0, _cliente.Llamadas.Count);
            Assert.IsTrue(_mediador.RevisarRespuesta(respuesta));
            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
            Assert.IsTrue(_mediador.Navegacion.MensajeEsError);
        }

        [TestMethod]
        public void ConSesion_RespuestaCorrectaNoCambiaLaPantalla()
        {
            IniciarSesion();
            _mediador.SesionIniciada();

            RespuestaCuentas respuesta = _mediador.Cuentas.ListarPorCliente("1712345678");

            Assert.IsFalse(_mediador.RevisarRespuesta(respuesta));
            Assert.AreEqual(Pantalla.Principal, _mediador.Navegacion.Actual);
        }

        [TestMethod]
        [DataRow("AUTH-003")]
        [DataRow("AUTH-004")]
        public void ServidorRechazaLaSesion_LaSesionSeLimpiaYSeVuelveAlAcceso(string codigo)
        {
            IniciarSesion();
            _mediador.SesionIniciada();
            _cliente.ResponderTodoCon(codigo, "Sesión rechazada");

            RespuestaMovimientos respuesta = _mediador.Movimientos.Consultar("CTA-1");

            Assert.IsFalse(_mediador.Sesion.EstaVigente);
            Assert.IsTrue(_mediador.RevisarRespuesta(respuesta));
            Assert.AreEqual(Pantalla.Acceso, _mediador.Navegacion.Actual);
            Assert.AreEqual("[" + codigo + "] Sesión rechazada", _mediador.Navegacion.MensajeAcceso);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("-5")]
        [DataRow("0.001")]
        [DataRow("12.3456")]
        [DataRow("999999.99")]
        public void Depositar_MontoSeEnviaSinRedondear(string texto)
        {
            IniciarSesion();
            decimal monto = decimal.Parse(texto, CultureInfo.InvariantCulture);

            _mediador.Movimientos.Depositar("CTA-1", monto);

            Assert.AreEqual(monto, _cliente.Ultima.Monto);
            Assert.AreEqual("CTA-1", _cliente.Ultima.Cuenta);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("-1.5")]
        [DataRow("2.005")]
        public void Retirar_MontoSeEnviaSinRedondear(string texto)
        {
            IniciarSesion();
            decimal monto = decimal.Parse(texto, CultureInfo.InvariantCulture);

            _mediador.Movimientos.Retirar("CTA-2", monto);

            Assert.AreEqual("RealizarRetiro", _cliente.Ultima.Metodo);
            Assert.AreEqual(monto, _cliente.Ultima.Monto);
        }

        [TestMethod]
        public void Transferir_PasaOrigenDestinoYMonto()
        {
            IniciarSesion();

            _mediador.Movimientos.Transferir("CTA-1", "CTA-2", 15.75m);

            Assert.AreEqual("RealizarTransferencia", _cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-1", _cliente.Ultima.Cuenta);
            Assert.AreEqual("CTA-2", _cliente.Ultima.CuentaDestino);
            Assert.AreEqual(15.75m, _cliente.Ultima.Monto);
        }

        [TestMethod]
        public void Costos_ConSesionDevuelveLaRespuestaDelServidor()
        {
            IniciarSesion();

            RespuestaCostos respuesta = _mediador.Costos.Consultar("CTA-1");

            Assert.AreEqual(100m, respuesta.SaldoActual);
            Assert.AreEqual(5, respuesta.OperacionesSinCosto);
        }

        [TestMethod]
        public void Movimientos_ConSesionEnvianElTokenDeLaSesion()
        {
            IniciarSesion();

            _mediador.Movimientos.Consultar("CTA-1");

            Assert.AreEqual("tok-1", _cliente.Ultima.Token);
        }
    }
}
