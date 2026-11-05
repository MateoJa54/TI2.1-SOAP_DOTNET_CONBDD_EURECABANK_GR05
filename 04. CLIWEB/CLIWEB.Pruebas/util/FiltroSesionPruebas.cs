using System.Linq;
using System.Web.Mvc;
using ec.edu.monster.web;
using ec.edu.monster.web.controlador;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using ec.edu.monster.web.pruebas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.web.pruebas
{
    [TestClass]
    public class FiltroSesionPruebas
    {
        private SesionFalsa sesion;

        [TestInitialize]
        public void Preparar()
        {
            sesion = new SesionFalsa();
        }

        private AuthorizationContext Contexto(string controlador, string accion, string ruta)
        {
            ContextoFalso contexto = new ContextoFalso(sesion, ruta, "GET");
            CuentaController ancla = new CuentaController(new ClienteFalso());
            PreparacionMvc.Asociar(ancla, contexto, controlador, accion);
            ReflectedControllerDescriptor descriptorControlador = new ReflectedControllerDescriptor(typeof(CuentaController));
            ActionDescriptor descriptorAccion = new ReflectedActionDescriptor(typeof(CuentaController).GetMethod("Logout"), "Logout", descriptorControlador);
            return new AuthorizationContext(ancla.ControllerContext, descriptorAccion);
        }

        private void Autorizar(string controlador, string accion, string ruta, out AuthorizationContext contexto)
        {
            contexto = Contexto(controlador, accion, ruta);
            new FiltroSesion().OnAuthorization(contexto);
        }

        [TestMethod]
        public void SinSesion_Cuentas_RedirigeAlLogin()
        {
            AuthorizationContext contexto;
            Autorizar("Cuentas", "Index", "~/Cuentas", out contexto);
            AsercionesWeb.AssertRedirige(contexto.Result, "Cuenta", "Login");
        }

        [TestMethod]
        [DataRow("Movimientos", "Index")]
        [DataRow("Movimientos", "Consultar")]
        [DataRow("Operaciones", "Index")]
        [DataRow("Operaciones", "Ejecutar")]
        [DataRow("Costos", "Index")]
        [DataRow("Costos", "Consultar")]
        [DataRow("Cuentas", "Listar")]
        [DataRow("Cuentas", "Consultar")]
        [DataRow("Cuenta", "Logout")]
        public void SinSesion_CualquierAccionProtegida_RedirigeAlLogin(string controlador, string accion)
        {
            AuthorizationContext contexto;
            Autorizar(controlador, accion, "~/" + controlador + "/" + accion, out contexto);
            AsercionesWeb.AssertRedirige(contexto.Result, "Cuenta", "Login");
        }

        [TestMethod]
        public void SinSesion_Login_PasaSinRedirigir()
        {
            AuthorizationContext contexto;
            Autorizar("Cuenta", "Login", "~/Cuenta/Login", out contexto);
            Assert.IsNull(contexto.Result);
        }

        [TestMethod]
        public void SinSesion_RecursosDeContent_PasanSinRedirigir()
        {
            AuthorizationContext contexto;
            Autorizar(null, null, "~/Content/site.css", out contexto);
            Assert.IsNull(contexto.Result);
            Autorizar(null, null, "~/Content/img/marca.svg", out contexto);
            Assert.IsNull(contexto.Result);
        }

        [TestMethod]
        public void SinSesion_PaginaDeError_PasaSinRedirigir()
        {
            AuthorizationContext contexto;
            Autorizar("Error", "Index", "~/Error", out contexto);
            Assert.IsNull(contexto.Result);
        }

        [TestMethod]
        public void ConSesionVigente_PasaTodasLasAcciones()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            AuthorizationContext contexto;
            Autorizar("Cuentas", "Index", "~/Cuentas", out contexto);
            Assert.IsNull(contexto.Result);
            Autorizar("Operaciones", "Ejecutar", "~/Operaciones/Ejecutar", out contexto);
            Assert.IsNull(contexto.Result);
        }

        [TestMethod]
        public void SesionNoVigente_RedirigeAlLogin()
        {
            SesionUsuario usuario = PreparacionMvc.IniciarSesion(sesion, "tok-1");
            usuario.Limpiar();
            AuthorizationContext contexto;
            Autorizar("Cuentas", "Index", "~/Cuentas", out contexto);
            AsercionesWeb.AssertRedirige(contexto.Result, "Cuenta", "Login");
        }

        [TestMethod]
        public void SinSesionNiRutaDeControlador_RedirigeAlLogin()
        {
            AuthorizationContext contexto;
            Autorizar(null, null, "~/", out contexto);
            AsercionesWeb.AssertRedirige(contexto.Result, "Cuenta", "Login");
        }

        [TestMethod]
        [DataRow("Cuenta", "Login", "~/Cuenta/Login", true)]
        [DataRow("cuenta", "login", "~/cuenta/login", true)]
        [DataRow("Cuenta", "Logout", "~/Cuenta/Logout", false)]
        [DataRow("Cuentas", "Index", "~/Cuentas", false)]
        [DataRow("Error", "Index", "~/Error", true)]
        [DataRow(null, null, "~/Content/site.css", true)]
        [DataRow(null, null, "/Content/site.css", true)]
        [DataRow(null, null, "~/Cuentas", false)]
        [DataRow(null, null, "~/Contenido/site.css", false)]
        public void EsPublica_SoloAccesoErrorYContent(string controlador, string accion, string ruta, bool esperado)
        {
            Assert.AreEqual(esperado, FiltroSesion.EsPublica(controlador, accion, ruta));
        }

        [TestMethod]
        public void FilterConfig_RegistraElFiltroDeSesionComoGlobal()
        {
            GlobalFilterCollection filtros = new GlobalFilterCollection();
            FilterConfig.RegisterGlobalFilters(filtros);
            Assert.IsTrue(filtros.Any(f => f.Instance is FiltroSesion));
            Assert.IsTrue(filtros.Any(f => f.Instance is HandleErrorAttribute));
        }
    }

    [TestClass]
    public class PresentacionPruebas
    {
        [TestMethod]
        [DataRow("OK", "200", "Depósito realizado", "Depósito realizado")]
        [DataRow("OK", "200", "", "")]
        [DataRow("OK", "200", null, "")]
        public void Mensaje_ExitoSeMuestraLiteral(string estado, string codigo, string mensaje, string esperado)
        {
            Assert.AreEqual(esperado, Presentacion.Mensaje(new RespuestaBase(estado, codigo, mensaje)));
        }

        [TestMethod]
        [DataRow("ERROR", "AUTH-001", "Usuario o clave incorrectos", "[AUTH-001] Usuario o clave incorrectos")]
        [DataRow("ERROR", "DAT-001", "Los datos de la petición están incompletos o son incorrectos", "[DAT-001] Los datos de la petición están incompletos o son incorrectos")]
        [DataRow("OK", "201", "Creado", "[201] Creado")]
        [DataRow("ERROR", "MON-001", "El monto debe ser mayor que cero y con máximo dos decimales", "[MON-001] El monto debe ser mayor que cero y con máximo dos decimales")]
        public void Mensaje_ErrorLlevaCodigoEntreCorchetes(string estado, string codigo, string mensaje, string esperado)
        {
            Assert.AreEqual(esperado, Presentacion.Mensaje(new RespuestaBase(estado, codigo, mensaje)));
        }

        [TestMethod]
        public void Mensaje_Nulo_DevuelveElErrorDeServicioSinDetalle()
        {
            string texto = Presentacion.Mensaje(null);
            Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", texto);
            Assert.IsFalse(texto.Contains("http"));
        }

        [TestMethod]
        [DataRow("OK", "200", true)]
        [DataRow("OK", "201", false)]
        [DataRow("ERROR", "200", false)]
        [DataRow("", "", false)]
        public void EsExito_RequiereEstadoOkYCodigo200(string estado, string codigo, bool esperado)
        {
            Assert.AreEqual(esperado, Presentacion.EsExito(new RespuestaBase(estado, codigo, "m")));
        }

        [TestMethod]
        public void EsExito_Nulo_EsFalso()
        {
            Assert.IsFalse(Presentacion.EsExito(null));
        }

        [TestMethod]
        [DataRow(null, true)]
        [DataRow("", true)]
        [DataRow("tok", false)]
        public void SesionWeb_HaySesion_DependeDeLaSesionVigente(string token, bool sinSesion)
        {
            SesionFalsa sesion = new SesionFalsa();
            SesionUsuario usuario = new SesionUsuario();
            usuario.Iniciar(new RespuestaAutenticacion { Token = token });
            sesion[SesionWeb.Clave] = usuario;
            Assert.AreEqual(!sinSesion, SesionWeb.HaySesion(sesion));
        }

        [TestMethod]
        public void SesionWeb_ObtenerConSesionNula_DevuelveNulo()
        {
            Assert.IsNull(SesionWeb.Obtener(null));
            Assert.IsFalse(SesionWeb.HaySesion(null));
        }

        [TestMethod]
        public void SesionWeb_ObtenerOCrear_GuardaUnaSesionPorSesionHttp()
        {
            SesionFalsa primera = new SesionFalsa();
            SesionFalsa segunda = new SesionFalsa();
            SesionUsuario a = SesionWeb.ObtenerOCrear(primera);
            SesionUsuario b = SesionWeb.ObtenerOCrear(segunda);
            Assert.AreNotSame(a, b);
            Assert.AreSame(a, SesionWeb.ObtenerOCrear(primera));
        }
    }
}
