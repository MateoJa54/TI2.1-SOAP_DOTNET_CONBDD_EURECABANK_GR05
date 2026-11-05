using System;
using System.Configuration;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.web.pruebas
{
    [TestClass]
    public class HashClavePruebas
    {
        [TestMethod]
        public void Sha256_ClaveDeEjemplo_DevuelveElHashEsperado()
        {
            Assert.AreEqual("891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4", HashClave.Sha256("Cromero2026"));
        }

        [TestMethod]
        [DataRow("abc", "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
        [DataRow("", "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [DataRow("Cromero2026", "891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4")]
        public void Sha256_TextosConocidos(string texto, string esperado)
        {
            Assert.AreEqual(esperado, HashClave.Sha256(texto));
        }

        [TestMethod]
        [DataRow("Cromero2026")]
        [DataRow("abc")]
        [DataRow("ñandú")]
        [DataRow("clave con espacios")]
        public void Sha256_DevuelveSesentaYCuatroHexMinusculas(string texto)
        {
            string hash = HashClave.Sha256(texto);
            Assert.AreEqual(64, hash.Length);
            foreach (char c in hash)
            {
                Assert.IsTrue((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'), "Carácter no hexadecimal: " + c);
            }
        }

        [TestMethod]
        public void Sha256_DistingueMayusculasYMinusculas()
        {
            Assert.AreNotEqual(HashClave.Sha256("Cromero2026"), HashClave.Sha256("cromero2026"));
        }

        [TestMethod]
        public void Sha256_EsDeterminista()
        {
            Assert.AreEqual(HashClave.Sha256("Cromero2026"), HashClave.Sha256("Cromero2026"));
        }

        [TestMethod]
        public void Sha256_NuloEquivaleAlHashDeCadenaVacia()
        {
            Assert.AreEqual(HashClave.Sha256(""), HashClave.Sha256(null));
        }
    }

    [TestClass]
    public class DireccionServicioPruebas
    {
        private string envPrevia;

        [TestInitialize]
        public void Preparar()
        {
            envPrevia = Environment.GetEnvironmentVariable(DireccionServicio.VariableEntorno);
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, null);
        }

        [TestCleanup]
        public void Limpiar()
        {
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, envPrevia);
        }

        [TestMethod]
        public void Constantes_TienenLosValoresDelContrato()
        {
            Assert.AreEqual("WS_URL", DireccionServicio.VariableEntorno);
            Assert.AreEqual("ws.url", DireccionServicio.ClaveConfiguracion);
            Assert.AreEqual("http://localhost:50775/WSEUREKABANK.svc", DireccionServicio.PorDefecto);
        }

        [TestMethod]
        [DataRow("http://arg/svc")]
        [DataRow("  http://arg/svc  ")]
        [DataRow("http://otra-maquina:8080/WSEUREKABANK.svc")]
        [DataRow("https://servidor.local/WSEUREKABANK.svc")]
        public void Resolver_ArgumentoGanaSobreVariableDeEntorno(string argumento)
        {
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, "http://desde-variable/svc");
            Assert.AreEqual(argumento.Trim(), DireccionServicio.Resolver(argumento));
        }

        [TestMethod]
        [DataRow("http://arg/svc")]
        [DataRow("  http://arg/svc  ")]
        [DataRow("http://10.0.0.5/WSEUREKABANK.svc")]
        public void Resolver_ArgumentoGanaSobreAppConfig(string argumento)
        {
            Assert.AreEqual(argumento.Trim(), DireccionServicio.Resolver(argumento));
        }

        [TestMethod]
        [DataRow("http://desde-variable/svc")]
        [DataRow("  http://desde-variable/svc  ")]
        [DataRow("http://127.0.0.1:9000/WSEUREKABANK.svc")]
        public void Resolver_SinArgumentoUsaVariableDeEntorno(string variable)
        {
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, variable);
            Assert.AreEqual(variable.Trim(), DireccionServicio.Resolver(null));
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Resolver_ArgumentoVacioCaeEnLaVariableDeEntorno(string argumento)
        {
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, "http://desde-variable/svc");
            Assert.AreEqual("http://desde-variable/svc", DireccionServicio.Resolver(argumento));
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Resolver_VariableVaciaSeIgnoraYUsaAppConfigOPorDefecto(string variable)
        {
            Environment.SetEnvironmentVariable(DireccionServicio.VariableEntorno, variable);
            Assert.AreEqual(EsperadoSinArgumentoNiVariable(), DireccionServicio.Resolver(null));
        }

        [TestMethod]
        public void Resolver_SinNadaDevuelveAppConfigOValorPorDefecto()
        {
            Assert.AreEqual(EsperadoSinArgumentoNiVariable(), DireccionServicio.Resolver(null));
        }

        [TestMethod]
        public void Resolver_SinNadaNoDevuelveVacio()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(DireccionServicio.Resolver(null)));
        }

        [TestMethod]
        [DataRow((string)null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Resolver_ArgumentoYVariableVaciosDevuelveAppConfigOPorDefecto(string argumento)
        {
            Assert.AreEqual(EsperadoSinArgumentoNiVariable(), DireccionServicio.Resolver(argumento));
        }

        // El valor de App.config solo se ve si el proceso de pruebas lo carga; si no existe, vale el valor por defecto.
        private static string EsperadoSinArgumentoNiVariable()
        {
            string configurada = ConfigurationManager.AppSettings[DireccionServicio.ClaveConfiguracion];
            return string.IsNullOrWhiteSpace(configurada) ? DireccionServicio.PorDefecto : configurada.Trim();
        }
    }

    [TestClass]
    public class SesionUsuarioPruebas
    {
        [TestMethod]
        public void SesionNueva_NoEstaVigente()
        {
            Assert.IsFalse(new SesionUsuario().EstaVigente);
        }

        [TestMethod]
        public void Iniciar_CopiaLosDatosDeLaRespuesta()
        {
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion
            {
                Codigo = "200",
                Token = "tok-9",
                Empleado = "E-009",
                NombreEmpleado = "Luis Mena",
                Rol = "Supervisor",
                Expiracion = "2026-11-04T23:00:00"
            });
            Assert.IsTrue(sesion.EstaVigente);
            Assert.AreEqual("tok-9", sesion.Token);
            Assert.AreEqual("E-009", sesion.Empleado);
            Assert.AreEqual("Luis Mena", sesion.NombreEmpleado);
            Assert.AreEqual("Supervisor", sesion.Rol);
            Assert.AreEqual("2026-11-04T23:00:00", sesion.Expiracion);
        }

        [TestMethod]
        public void Iniciar_RespuestaNulaLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new SesionUsuario().Iniciar(null));
        }

        [TestMethod]
        public void Limpiar_DejaTodosLosCamposVacios()
        {
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "tok", Empleado = "E", NombreEmpleado = "N", Rol = "R", Expiracion = "X" });
            sesion.Limpiar();
            Assert.IsFalse(sesion.EstaVigente);
            Assert.IsNull(sesion.Token);
            Assert.IsNull(sesion.Empleado);
            Assert.IsNull(sesion.NombreEmpleado);
            Assert.IsNull(sesion.Rol);
            Assert.IsNull(sesion.Expiracion);
        }

        [TestMethod]
        public void Limpiar_SobreSesionVaciaNoFalla()
        {
            SesionUsuario sesion = new SesionUsuario();
            sesion.Limpiar();
            Assert.IsFalse(sesion.EstaVigente);
        }

        [TestMethod]
        [DataRow((string)null, false)]
        [DataRow("", false)]
        [DataRow("tok", true)]
        [DataRow("a", true)]
        [DataRow("token-largo-de-prueba-0123456789", true)]
        public void EstaVigente_DependeDelToken(string token, bool esperado)
        {
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = token });
            Assert.AreEqual(esperado, sesion.EstaVigente);
        }

        [TestMethod]
        public void Iniciar_SegundaVezReemplazaLosDatos()
        {
            SesionUsuario sesion = new SesionUsuario();
            sesion.Iniciar(new RespuestaAutenticacion { Token = "uno", NombreEmpleado = "Primero" });
            sesion.Iniciar(new RespuestaAutenticacion { Token = "dos", NombreEmpleado = "Segundo" });
            Assert.AreEqual("dos", sesion.Token);
            Assert.AreEqual("Segundo", sesion.NombreEmpleado);
        }
    }
}
