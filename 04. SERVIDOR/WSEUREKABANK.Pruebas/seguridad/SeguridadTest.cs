using System;
using ec.edu.monster.modelos;
using ec.edu.monster.pruebas.Apoyo;
using ec.edu.monster.seguridad;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.seguridad
{
    [TestClass]
    public class TokenServicioTest
    {
        private RelojManual reloj;
        private TokenServicio tokens;

        [TestInitialize]
        public void Preparar()
        {
            reloj = new RelojManual();
            tokens = new TokenServicio(TimeSpan.FromMinutes(30), reloj.Leer);
        }

        [TestMethod]
        public void VigenciaPorDefecto_EsTreintaMinutos()
        {
            Assert.AreEqual(TimeSpan.FromMinutes(30), TokenServicio.VigenciaPorDefecto);
            Assert.AreEqual(TimeSpan.FromMinutes(30), new TokenServicio().Vigencia);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-5)]
        public void VigenciaNoPositiva_Falla(int minutos)
        {
            Assert.ThrowsException<ArgumentException>(() => new TokenServicio(TimeSpan.FromMinutes(minutos), reloj.Leer));
        }

        [TestMethod]
        public void RelojNulo_Falla()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new TokenServicio(TimeSpan.FromMinutes(1), null));
        }

        [TestMethod]
        public void GenerarToken_EsUnGuid()
        {
            string token = tokens.GenerarToken("0001");
            Guid analizado;
            Assert.IsTrue(Guid.TryParse(token, out analizado));
        }

        [TestMethod]
        public void GenerarToken_DosVecesDaTokensDistintos()
        {
            Assert.AreNotEqual(tokens.GenerarToken("0001"), tokens.GenerarToken("0001"));
        }

        [TestMethod]
        public void ValidarToken_RecienGenerado_EsValido()
        {
            Assert.AreSame(CodigoRespuesta.Ok200, tokens.ValidarToken(tokens.GenerarToken("0001")));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void ValidarToken_Vacio_EsDat001(string token)
        {
            Assert.AreSame(CodigoRespuesta.Dat001, tokens.ValidarToken(token));
        }

        [TestMethod]
        public void ValidarToken_Inexistente_EsAuth003()
        {
            Assert.AreSame(CodigoRespuesta.Auth003, tokens.ValidarToken(Guid.NewGuid().ToString()));
        }

        [TestMethod]
        public void ValidarToken_UnMinutoAntesDeVencer_EsValido()
        {
            string token = tokens.GenerarToken("0001");
            reloj.Avanzar(TimeSpan.FromMinutes(29) + TimeSpan.FromSeconds(59));
            Assert.AreSame(CodigoRespuesta.Ok200, tokens.ValidarToken(token));
        }

        [TestMethod]
        public void ValidarToken_EnElInstanteDeVencer_EsAuth004()
        {
            string token = tokens.GenerarToken("0001");
            reloj.Avanzar(TimeSpan.FromMinutes(30));
            Assert.AreSame(CodigoRespuesta.Auth004, tokens.ValidarToken(token));
        }

        [TestMethod]
        public void ValidarToken_Vencido_SeConservaParaCerrarSesion()
        {
            string token = tokens.GenerarToken("0001");
            reloj.Avanzar(TimeSpan.FromMinutes(31));
            Assert.AreSame(CodigoRespuesta.Auth004, tokens.ValidarToken(token));
            Assert.IsNotNull(tokens.ObtenerSesion(token));
        }

        [TestMethod]
        public void Cerrar_DescartaElToken()
        {
            string token = tokens.GenerarToken("0001");
            tokens.Cerrar(token);
            Assert.AreSame(CodigoRespuesta.Auth003, tokens.ValidarToken(token));
            Assert.AreEqual(0, tokens.CantidadSesiones);
        }

        [TestMethod]
        public void Cerrar_ConNuloOInexistente_NoFalla()
        {
            tokens.Cerrar(null);
            tokens.Cerrar("no existe");
        }

        [TestMethod]
        public void ObtenerSesion_GuardaElEmpleado()
        {
            string token = tokens.GenerarToken("0007");
            Assert.AreEqual("0007", tokens.ObtenerSesion(token).Empleado);
            Assert.IsNull(tokens.ObtenerSesion(null));
            Assert.IsNull(tokens.ObtenerSesion("x"));
        }

        [TestMethod]
        public void ObtenerExpiracion_EsAhoraMasVigencia()
        {
            string token = tokens.GenerarToken("0001");
            Assert.AreEqual(reloj.Ahora.AddMinutes(30), tokens.ObtenerExpiracion(token));
            Assert.IsNull(tokens.ObtenerExpiracion("x"));
            Assert.IsNull(tokens.ObtenerExpiracion(null));
        }

        [TestMethod]
        public void AsociarLogSession_ConservaLaExpiracion()
        {
            string token = tokens.GenerarToken("0001");
            DateTime expiracion = tokens.ObtenerExpiracion(token).Value;
            tokens.AsociarLogSession(token, 42);
            Assert.AreEqual(42, tokens.ObtenerSesion(token).IdSesionLog);
            Assert.AreEqual(expiracion, tokens.ObtenerExpiracion(token).Value);
            tokens.AsociarLogSession("inexistente", 1);
            tokens.AsociarLogSession(null, 1);
        }

        [TestMethod]
        public void GenerarToken_LimpiaLosQueLlevanUnaVigenciaEnteraVencidos()
        {
            string viejo = tokens.GenerarToken("0001");
            reloj.Avanzar(TimeSpan.FromMinutes(61));
            tokens.GenerarToken("0002");
            Assert.IsNull(tokens.ObtenerSesion(viejo));
            Assert.AreEqual(1, tokens.CantidadSesiones);
        }

        [TestMethod]
        public void GenerarToken_ConservaLosRecienVencidos()
        {
            string viejo = tokens.GenerarToken("0001");
            reloj.Avanzar(TimeSpan.FromMinutes(40));
            tokens.GenerarToken("0002");
            Assert.IsNotNull(tokens.ObtenerSesion(viejo));
        }

        [TestMethod]
        public void VariasHebras_GenerandoTokens_NoPierdenSesiones()
        {
            System.Threading.Tasks.Parallel.For(0, 200, i => tokens.GenerarToken("000" + (i % 9)));
            Assert.AreEqual(200, tokens.CantidadSesiones);
        }
    }

    [TestClass]
    public class ServicioAutenticacionTest
    {
        private Entorno entorno;

        [TestInitialize]
        public void Preparar()
        {
            entorno = new Entorno();
        }

        [TestMethod]
        public void ClaveEnClaro_Autentica()
        {
            RespuestaAutenticacion r = entorno.Autenticacion.Autenticar("cromero", "Cromero2026");
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("OK", r.Estado);
            Assert.AreEqual("Autenticación exitosa", r.Mensaje);
            Assert.AreEqual("0001", r.Empleado);
            Assert.AreEqual("Carlos Romero Pachas", r.NombreEmpleado);
            Assert.AreEqual("Procesos", r.Rol);
            Assert.IsNotNull(r.Token);
        }

        [TestMethod]
        public void HashSha256_Autentica()
        {
            RespuestaAutenticacion r = entorno.Autenticacion.Autenticar("cromero",
                "891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4");
            Assert.AreEqual("200", r.Codigo);
        }

        [TestMethod]
        public void Exito_FormatoDeExpiracionIso8601SinZona()
        {
            RespuestaAutenticacion r = entorno.Autenticacion.Autenticar("cromero", "Cromero2026");
            Assert.AreEqual("2026-11-03T10:30:00", r.Expiracion);
        }

        [TestMethod]
        public void Exito_RegistraUnaFilaEnLogSession()
        {
            entorno.Autenticacion.Autenticar("cromero", "Cromero2026");
            Assert.AreEqual(1, entorno.Base.Ingresos.Count);
            Assert.AreEqual("0001", entorno.Base.Ingresos[0]);
        }

        [TestMethod]
        public void Exito_SinRolEnLaBase_DevuelveRolNulo()
        {
            RespuestaAutenticacion r = entorno.Autenticacion.Autenticar("sinrol", "Pachas2026");
            Assert.AreEqual("200", r.Codigo);
            Assert.IsNull(r.Rol);
        }

        [TestMethod]
        public void Usuario_ConEspaciosAlrededor_SeRecorta()
        {
            Assert.AreEqual("200", entorno.Autenticacion.Autenticar("  cromero  ", "Cromero2026").Codigo);
        }

        [TestMethod]
        [DataRow(null, "x")]
        [DataRow("", "x")]
        [DataRow("   ", "x")]
        [DataRow("cromero", null)]
        [DataRow("cromero", "")]
        [DataRow("cromero", "  ")]
        [DataRow(null, null)]
        public void DatosVacios_EsDat001_SinTocarLaBase(string usuario, string clave)
        {
            RespuestaAutenticacion r = entorno.Autenticacion.Autenticar(usuario, clave);
            Assert.AreEqual("DAT-001", r.Codigo);
            Assert.AreEqual("ERROR", r.Estado);
            Assert.IsNull(r.Token);
            Assert.AreEqual(0, entorno.Base.Ingresos.Count);
        }

        [TestMethod]
        public void UsuarioInexistente_EsAuth001()
        {
            Assert.AreEqual("AUTH-001", entorno.Autenticacion.Autenticar("nadie", "Cromero2026").Codigo);
        }

        [TestMethod]
        public void ClaveIncorrecta_EsAuth001ConElMismoMensajeQueUsuarioInexistente()
        {
            RespuestaAutenticacion a = entorno.Autenticacion.Autenticar("cromero", "mal");
            RespuestaAutenticacion b = entorno.Autenticacion.Autenticar("nadie", "mal");
            Assert.AreEqual("AUTH-001", a.Codigo);
            Assert.AreEqual(a.Mensaje, b.Mensaje);
        }

        [TestMethod]
        public void UsuarioAnulado_ConClaveCorrecta_EsAuth002()
        {
            Assert.AreEqual("AUTH-002", entorno.Autenticacion.Autenticar("inactivo", "Sarmiento2026").Codigo);
        }

        [TestMethod]
        public void UsuarioAnulado_ConClaveIncorrecta_EsAuth001()
        {
            Assert.AreEqual("AUTH-001", entorno.Autenticacion.Autenticar("inactivo", "mal").Codigo);
        }

        [TestMethod]
        public void Error_NoRegistraSesion()
        {
            entorno.Autenticacion.Autenticar("cromero", "mal");
            entorno.Autenticacion.Autenticar("inactivo", "Sarmiento2026");
            Assert.AreEqual(0, entorno.Base.Ingresos.Count);
            Assert.AreEqual(0, entorno.Tokens.CantidadSesiones);
        }

        [TestMethod]
        public void DosLogins_DanTokensDistintos()
        {
            string a = entorno.Autenticacion.Autenticar("cromero", "Cromero2026").Token;
            string b = entorno.Autenticacion.Autenticar("cromero", "Cromero2026").Token;
            Assert.AreNotEqual(a, b);
            Assert.AreEqual(2, entorno.Base.Ingresos.Count);
        }

        [TestMethod]
        public void CerrarSesion_Exito_MarcaSalidaYDescartaElToken()
        {
            string token = entorno.Entrar();
            RespuestaBase r = entorno.Autenticacion.CerrarSesion(token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Sesión cerrada correctamente", r.Mensaje);
            Assert.AreEqual(1, entorno.Base.Salidas.Count);
            Assert.AreEqual(1, entorno.Base.Salidas[0]);
            Assert.AreEqual("AUTH-003", entorno.Cuentas.ConsultarCuenta("00200002", token).Codigo);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("  ")]
        public void CerrarSesion_SinToken_EsDat001(string token)
        {
            Assert.AreEqual("DAT-001", entorno.Autenticacion.CerrarSesion(token).Codigo);
        }

        [TestMethod]
        public void CerrarSesion_TokenInexistente_EsAuth003()
        {
            RespuestaBase r = entorno.Autenticacion.CerrarSesion(Guid.NewGuid().ToString());
            Assert.AreEqual("AUTH-003", r.Codigo);
            Assert.AreEqual(0, entorno.Base.Salidas.Count);
        }

        [TestMethod]
        public void CerrarSesion_TokenVencido_EsAuth004PeroCierraLaFila()
        {
            string token = entorno.Entrar();
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(45));
            RespuestaBase r = entorno.Autenticacion.CerrarSesion(token);
            Assert.AreEqual("AUTH-004", r.Codigo);
            Assert.AreEqual(1, entorno.Base.Salidas.Count);
            Assert.AreEqual("AUTH-003", entorno.Autenticacion.CerrarSesion(token).Codigo);
        }

        [TestMethod]
        public void CerrarSesion_DosVeces_LaSegundaEsAuth003()
        {
            string token = entorno.Entrar();
            entorno.Autenticacion.CerrarSesion(token);
            Assert.AreEqual("AUTH-003", entorno.Autenticacion.CerrarSesion(token).Codigo);
        }

        [TestMethod]
        public void Constructor_RechazaNulos()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioAutenticacion(null, entorno.Base, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioAutenticacion(entorno.Base, null, entorno.Tokens));
            Assert.ThrowsException<ArgumentNullException>(() => new ServicioAutenticacion(entorno.Base, entorno.Base, null));
        }
    }
}
