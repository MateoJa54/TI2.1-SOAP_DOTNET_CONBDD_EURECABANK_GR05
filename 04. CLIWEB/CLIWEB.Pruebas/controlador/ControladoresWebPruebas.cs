using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using ec.edu.monster.web.controlador;
using ec.edu.monster.web.modelo;
using ec.edu.monster.web.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.web.pruebas
{
    // Comprobaciones compartidas por los controladores MVC.
    public static class AsercionesWeb
    {
        public static void AssertRedirige(ActionResult resultado, string controlador, string accion)
        {
            RedirectToRouteResult redireccion = resultado as RedirectToRouteResult;
            Assert.IsNotNull(redireccion, "Se esperaba una redirección y llegó " + (resultado == null ? "nulo" : resultado.GetType().Name));
            Assert.AreEqual(controlador, redireccion.RouteValues["controller"] as string);
            Assert.AreEqual(accion, redireccion.RouteValues["action"] as string);
        }

        public static string Aviso(ActionResult resultado)
        {
            RedirectToRouteResult redireccion = resultado as RedirectToRouteResult;
            Assert.IsNotNull(redireccion);
            return redireccion.RouteValues["aviso"] as string;
        }

        public static T Modelo<T>(ActionResult resultado) where T : class
        {
            ViewResult vista = resultado as ViewResult;
            Assert.IsNotNull(vista, "Se esperaba una vista y llegó " + (resultado == null ? "nulo" : resultado.GetType().Name));
            T modelo = vista.Model as T;
            Assert.IsNotNull(modelo);
            return modelo;
        }
    }

    [TestClass]
    public class CuentaControllerPruebas
    {
        private ClienteFalso cliente;
        private SesionFalsa sesion;
        private CuentaController controlador;

        [TestInitialize]
        public void Preparar()
        {
            PreparacionMvc.RegistrarRutas();
            cliente = new ClienteFalso();
            sesion = new SesionFalsa();
            controlador = new CuentaController(cliente);
            PreparacionMvc.Asociar(controlador, new ContextoFalso(sesion, "~/Cuenta/Login", "POST"), "Cuenta", "Login");
        }

        private SesionUsuario SesionGuardada()
        {
            return sesion[SesionWeb.Clave] as SesionUsuario;
        }

        [TestMethod]
        public void LoginGet_SinSesion_MuestraElFormularioConElAviso()
        {
            LoginVista vista = AsercionesWeb.Modelo<LoginVista>(controlador.Login("[AUTH-004] El token de sesión no es válido"));
            Assert.AreEqual("[AUTH-004] El token de sesión no es válido", vista.Aviso);
            Assert.IsNull(vista.Error);
        }

        [TestMethod]
        public void LoginGet_ConSesionVigente_RedirigeAlIndiceDeCuentas()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            AsercionesWeb.AssertRedirige(controlador.Login(null), "Cuentas", "Index");
        }

        [TestMethod]
        public void LoginPost_Correcto_GuardaLaSesionVigente()
        {
            controlador.Login("ana", "Cromero2026");
            SesionUsuario guardada = SesionGuardada();
            Assert.IsNotNull(guardada);
            Assert.IsTrue(guardada.EstaVigente);
            Assert.AreEqual("tok-1", guardada.Token);
            Assert.AreEqual("Ana Pérez", guardada.NombreEmpleado);
        }

        [TestMethod]
        public void LoginPost_Correcto_RedirigeAlIndiceDeCuentas()
        {
            AsercionesWeb.AssertRedirige(controlador.Login("ana", "Cromero2026"), "Cuentas", "Index");
        }

        [TestMethod]
        public void LoginPost_EnviaUsuarioYHashDeLaClave()
        {
            controlador.Login("ana", "Cromero2026");
            Assert.AreEqual("ana", cliente.Ultima.Usuario);
            Assert.AreEqual(HashClave.Sha256("Cromero2026"), cliente.Ultima.ClaveHash);
        }

        [TestMethod]
        public void LoginPost_NuevoLogin_InvalidaLaSesionPrevia()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-viejo");
            SesionUsuario anterior = SesionGuardada();

            controlador.Login("ana", "Cromero2026");

            Assert.AreEqual(1, cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual("tok-viejo", cliente.Llamadas.First(l => l.Metodo == "CerrarSesion").Token);
            Assert.AreEqual(1, sesion.Limpiezas);
            Assert.AreNotSame(anterior, SesionGuardada());
            Assert.AreEqual("tok-1", SesionGuardada().Token);
        }

        [TestMethod]
        public void LoginPost_SinSesionPrevia_NoLlamaCerrarSesion()
        {
            controlador.Login("ana", "Cromero2026");
            Assert.AreEqual(0, cliente.Cuantas("CerrarSesion"));
        }

        [TestMethod]
        public void LoginPost_Fallido_NoGuardaSesionYMuestraElCodigo()
        {
            cliente.RespuestaAutenticar = new RespuestaAutenticacion { Estado = "ERROR", Codigo = "AUTH-001", Mensaje = "Usuario o clave incorrectos" };
            LoginVista vista = AsercionesWeb.Modelo<LoginVista>(controlador.Login("ana", "mal"));
            Assert.AreEqual("[AUTH-001] Usuario o clave incorrectos", vista.Error);
            Assert.AreEqual("ana", vista.Usuario);
            Assert.IsNull(SesionGuardada());
        }

        [TestMethod]
        public void LoginPost_RespuestaSinToken_NoGuardaSesion()
        {
            cliente.RespuestaAutenticar.Token = null;
            controlador.Login("ana", "Cromero2026");
            Assert.IsNull(SesionGuardada());
        }

        [TestMethod]
        public void LoginPost_ErrorDeServicio_NoMuestraElDetalleTecnico()
        {
            cliente.RespuestaAutenticar = new RespuestaAutenticacion
            {
                Estado = "ERROR",
                Codigo = "SER-001",
                Mensaje = "El servicio no está disponible en este momento"
            };
            LoginVista vista = AsercionesWeb.Modelo<LoginVista>(controlador.Login("ana", "x"));
            Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", vista.Error);
            Assert.IsFalse(vista.Error.Contains("http"));
            Assert.IsFalse(vista.Error.Contains("localhost"));
        }

        [TestMethod]
        public void Logout_AbandonaLaSesionHttp()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            controlador.Logout();
            Assert.AreEqual(1, sesion.Abandonos);
            Assert.IsFalse(sesion.Contiene(SesionWeb.Clave));
        }

        [TestMethod]
        public void Logout_LlamaCerrarSesionConElTokenVigente()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-9");
            controlador.Logout();
            Assert.AreEqual("CerrarSesion", cliente.Ultima.Metodo);
            Assert.AreEqual("tok-9", cliente.Ultima.Token);
        }

        [TestMethod]
        public void Logout_RedirigeAlAccesoConElMensajeDelServidor()
        {
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            ActionResult resultado = controlador.Logout();
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
            Assert.AreEqual("Sesión cerrada correctamente", AsercionesWeb.Aviso(resultado));
        }

        [TestMethod]
        public void Logout_SinSesion_IgualmenteAbandonaYRedirige()
        {
            ActionResult resultado = controlador.Logout();
            Assert.AreEqual(1, sesion.Abandonos);
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
        }

        [TestMethod]
        public void CuentaController_TieneAccionesLoginYLogout()
        {
            Assert.IsTrue(typeof(CuentaController).GetMethods().Count(m => m.Name == "Login") == 2);
            Assert.IsNotNull(typeof(CuentaController).GetMethod("Logout"));
        }
    }

    [TestClass]
    public class CuentasControllerPruebas
    {
        private ClienteFalso cliente;
        private SesionFalsa sesion;
        private CuentasController controlador;

        [TestInitialize]
        public void Preparar()
        {
            PreparacionMvc.RegistrarRutas();
            cliente = new ClienteFalso();
            sesion = new SesionFalsa();
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            controlador = new CuentasController(cliente);
            PreparacionMvc.Asociar(controlador, new ContextoFalso(sesion, "~/Cuentas/Listar", "POST"), "Cuentas", "Listar");
        }

        [TestMethod]
        public void Index_MuestraElFormularioVacio()
        {
            CuentasVista vista = AsercionesWeb.Modelo<CuentasVista>(controlador.Index());
            Assert.IsFalse(vista.Consultado);
            Assert.AreEqual(0, cliente.Llamadas.Count);
        }

        [TestMethod]
        public void Listar_EnviaLaCedulaYElToken()
        {
            controlador.Listar("1712345678");
            Llamada llamada = cliente.Ultima;
            Assert.AreEqual("ListarCuentasPorCliente", llamada.Metodo);
            Assert.AreEqual("1712345678", llamada.Cedula);
            Assert.AreEqual("tok-1", llamada.Token);
        }

        [TestMethod]
        public void Listar_MapeaLasCuentasSinExponerLaClave()
        {
            cliente.RespuestaListar.Cuentas = new List<CuentaDTO>
            {
                new CuentaDTO { Codigo = "CTA-1", Moneda = "USD", DescripcionMoneda = "Dólar", Saldo = 10.5m, Estado = "ACTIVA", Clave = "secreto" }
            };
            CuentasVista vista = AsercionesWeb.Modelo<CuentasVista>(controlador.Listar("1712345678"));
            Assert.AreEqual(1, vista.Cuentas.Count);
            Assert.AreEqual("CTA-1", vista.Cuentas[0].Codigo);
            Assert.AreEqual("Dólar", vista.Cuentas[0].Moneda);
            Assert.AreEqual("10.50", vista.Cuentas[0].Saldo);
            Assert.AreEqual("Juan Ruiz", vista.NombreCliente);
            Assert.IsNull(typeof(CuentaFila).GetProperty("Clave"));
            Assert.IsFalse(vista.Cuentas.Any(c => c.Codigo.Contains("secreto") || c.Estado.Contains("secreto")));
        }

        [TestMethod]
        public void Listar_SinCuentas_QuedaConsultadoYVacio()
        {
            CuentasVista vista = AsercionesWeb.Modelo<CuentasVista>(controlador.Listar("1712345678"));
            Assert.IsTrue(vista.Consultado);
            Assert.AreEqual(0, vista.Cuentas.Count);
            Assert.IsNull(vista.Error);
        }

        [TestMethod]
        [DataRow("AUTH-003")]
        [DataRow("AUTH-004")]
        public void Listar_TokenNoValido_InvalidaLaSesionYRedirigeAlAcceso(string codigo)
        {
            cliente.RespuestaListar.Codigo = codigo;
            cliente.RespuestaListar.Estado = "ERROR";
            cliente.RespuestaListar.Mensaje = "El token de sesión no es válido";
            ActionResult resultado = controlador.Listar("1712345678");
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
            Assert.AreEqual("[" + codigo + "] El token de sesión no es válido", AsercionesWeb.Aviso(resultado));
            Assert.AreEqual(1, sesion.Abandonos);
            Assert.IsFalse(sesion.Contiene(SesionWeb.Clave));
        }

        [TestMethod]
        public void Listar_ErrorDeServicio_MuestraErrorEnLinea()
        {
            cliente.RespuestaListar = new RespuestaCuentas { Estado = "ERROR", Codigo = "SER-001", Mensaje = "El servicio no está disponible en este momento" };
            CuentasVista vista = AsercionesWeb.Modelo<CuentasVista>(controlador.Listar("1712345678"));
            Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", vista.Error);
            Assert.AreEqual(0, sesion.Abandonos);
        }

        [TestMethod]
        public void Consultar_EnviaLaCuentaYMapeaElDetalleSinClave()
        {
            cliente.RespuestaConsultarCuenta.Cuenta.Clave = "secreto";
            ConsultaCuentaVista vista = AsercionesWeb.Modelo<ConsultaCuentaVista>(controlador.Consultar("CTA-1"));
            Assert.AreEqual("ConsultarCuenta", cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-1", cliente.Ultima.Cuenta);
            Assert.IsNotNull(vista.Detalle);
            Assert.AreEqual("CTA-1", vista.Detalle.Codigo);
            Assert.AreEqual("100.00", vista.Detalle.Saldo);
            Assert.AreEqual("Juan Ruiz", vista.Detalle.NombreCliente);
            Assert.IsNull(typeof(CuentaDetalle).GetProperty("Clave"));
        }

        [TestMethod]
        public void Consultar_CuentaNoEncontrada_MuestraElErrorDelServidor()
        {
            cliente.RespuestaConsultarCuenta = new RespuestaCuenta { Estado = "ERROR", Codigo = "CTA-404", Mensaje = "Cuenta no encontrada" };
            ConsultaCuentaVista vista = AsercionesWeb.Modelo<ConsultaCuentaVista>(controlador.Consultar("CTA-X"));
            Assert.IsNull(vista.Detalle);
            Assert.AreEqual("[CTA-404] Cuenta no encontrada", vista.Error);
        }

        [TestMethod]
        public void Consultar_TokenExpirado_RedirigeAlAcceso()
        {
            cliente.RespuestaConsultarCuenta.Codigo = "AUTH-004";
            cliente.RespuestaConsultarCuenta.Estado = "ERROR";
            cliente.RespuestaConsultarCuenta.Mensaje = "Token expirado";
            ActionResult resultado = controlador.Consultar("CTA-1");
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
            Assert.AreEqual("[AUTH-004] Token expirado", AsercionesWeb.Aviso(resultado));
        }

        [TestMethod]
        public void Cuentas_LaAccionDeListarEsUnPost()
        {
            Assert.IsNotNull(typeof(CuentasController).GetMethod("Listar").GetCustomAttributes(typeof(HttpPostAttribute), false).FirstOrDefault());
        }
    }

    [TestClass]
    public class MovimientosControllerPruebas
    {
        private ClienteFalso cliente;
        private SesionFalsa sesion;
        private MovimientosController controlador;

        [TestInitialize]
        public void Preparar()
        {
            PreparacionMvc.RegistrarRutas();
            cliente = new ClienteFalso();
            sesion = new SesionFalsa();
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            controlador = new MovimientosController(cliente);
            PreparacionMvc.Asociar(controlador, new ContextoFalso(sesion, "~/Movimientos/Consultar", "POST"), "Movimientos", "Consultar");
        }

        private static MovimientoDTO Movimiento(int numero, string tipo, string accion, decimal importe)
        {
            return new MovimientoDTO
            {
                Numero = numero,
                Fecha = "2026-11-01",
                TipoCodigo = "T" + numero,
                TipoDescripcion = tipo,
                TipoAccion = accion,
                Importe = importe,
                Referencia = "REF-" + numero,
                Empleado = "E-001"
            };
        }

        [TestMethod]
        public void Consultar_EnviaLaCuentaYElToken()
        {
            controlador.Consultar("CTA-1");
            Assert.AreEqual("ConsultarMovimientos", cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-1", cliente.Ultima.Cuenta);
            Assert.AreEqual("tok-1", cliente.Ultima.Token);
        }

        [TestMethod]
        public void Consultar_FilasTienenExactamenteCincoCeldas()
        {
            cliente.RespuestaMovimientosConsulta.Movimientos = new List<MovimientoDTO>
            {
                Movimiento(1, "Depósito", "Crédito", 10.5m),
                Movimiento(2, "Retiro", "Débito", 3m),
                Movimiento(3, "Transferencia", "Débito", 0.1m)
            };
            MovimientosVista vista = AsercionesWeb.Modelo<MovimientosVista>(controlador.Consultar("CTA-1"));
            Assert.AreEqual(3, vista.Filas.Count);
            foreach (FilaMovimiento fila in vista.Filas)
            {
                Assert.AreEqual(5, fila.Celdas().Length);
            }
            CollectionAssert.AreEqual(new[] { "1", "2026-11-01", "Depósito", "Crédito", "10.50" }, vista.Filas[0].Celdas());
            CollectionAssert.AreEqual(new[] { "2", "2026-11-01", "Retiro", "Débito", "3.00" }, vista.Filas[1].Celdas());
        }

        [TestMethod]
        public void Consultar_LaFilaSoloTieneLosCincoCamposDeLaTabla()
        {
            Assert.AreEqual(5, typeof(FilaMovimiento).GetProperties(BindingFlags.Public | BindingFlags.Instance).Length);
            Assert.IsNull(typeof(FilaMovimiento).GetProperty("Referencia"));
            Assert.IsNull(typeof(FilaMovimiento).GetProperty("Empleado"));
        }

        [TestMethod]
        public void Consultar_SinMovimientos_QuedaConsultadoYVacio()
        {
            MovimientosVista vista = AsercionesWeb.Modelo<MovimientosVista>(controlador.Consultar("CTA-1"));
            Assert.IsTrue(vista.Consultado);
            Assert.AreEqual(0, vista.Filas.Count);
        }

        [TestMethod]
        public void Consultar_SinDescripcion_UsaElCodigoDelTipo()
        {
            MovimientoDTO sinDescripcion = Movimiento(4, null, "Crédito", 1m);
            cliente.RespuestaMovimientosConsulta.Movimientos = new List<MovimientoDTO> { sinDescripcion };
            MovimientosVista vista = AsercionesWeb.Modelo<MovimientosVista>(controlador.Consultar("CTA-1"));
            Assert.AreEqual("T4", vista.Filas[0].Tipo);
        }

        [TestMethod]
        public void Consultar_ErrorDelServidor_MuestraCodigoEntreCorchetes()
        {
            cliente.RespuestaMovimientosConsulta = new RespuestaMovimientos { Estado = "ERROR", Codigo = "MOV-010", Mensaje = "La cuenta no existe" };
            MovimientosVista vista = AsercionesWeb.Modelo<MovimientosVista>(controlador.Consultar("CTA-9"));
            Assert.AreEqual("[MOV-010] La cuenta no existe", vista.Error);
            Assert.AreEqual(0, vista.Filas.Count);
        }

        [TestMethod]
        public void Consultar_TokenNoValido_RedirigeAlAcceso()
        {
            cliente.RespuestaMovimientosConsulta.Codigo = "AUTH-003";
            cliente.RespuestaMovimientosConsulta.Estado = "ERROR";
            cliente.RespuestaMovimientosConsulta.Mensaje = "El token de sesión no es válido";
            ActionResult resultado = controlador.Consultar("CTA-1");
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
            Assert.AreEqual(1, sesion.Abandonos);
        }

        [TestMethod]
        public void Index_MuestraElFormularioSinConsultar()
        {
            MovimientosVista vista = AsercionesWeb.Modelo<MovimientosVista>(controlador.Index());
            Assert.IsFalse(vista.Consultado);
        }
    }

    [TestClass]
    public class OperacionesControllerPruebas
    {
        private ClienteFalso cliente;
        private SesionFalsa sesion;
        private OperacionesController controlador;

        [TestInitialize]
        public void Preparar()
        {
            PreparacionMvc.RegistrarRutas();
            cliente = new ClienteFalso();
            sesion = new SesionFalsa();
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            controlador = new OperacionesController(cliente);
            PreparacionMvc.Asociar(controlador, new ContextoFalso(sesion, "~/Operaciones/Ejecutar", "POST"), "Operaciones", "Ejecutar");
        }

        private OperacionesVista Ejecutar(string operacion, string cuenta, string destino, string monto)
        {
            return AsercionesWeb.Modelo<OperacionesVista>(controlador.Ejecutar(operacion, cuenta, destino, monto));
        }

        [TestMethod]
        [DataRow("eliminar", "CTA-1", "", "10")]
        [DataRow("DEPOSITO", "CTA-1", "", "10")]
        [DataRow("depositar", "CTA-1", "", "10")]
        [DataRow("saldo", "CTA-1", "", "10")]
        [DataRow(" deposito", "CTA-1", "", "10")]
        [DataRow("", "CTA-1", "", "10")]
        [DataRow(null, "CTA-1", "", "10")]
        [DataRow("cerrar", "CTA-1", "CTA-2", "5")]
        public void Ejecutar_OperacionDesconocida_DevuelveDat001SinLlamarAlServidor(string operacion, string cuenta, string destino, string monto)
        {
            OperacionesVista vista = Ejecutar(operacion, cuenta, destino, monto);
            Assert.AreEqual("[DAT-001] Los datos de la petición están incompletos o son incorrectos", vista.Mensaje);
            Assert.IsFalse(vista.Exito);
            Assert.AreEqual(0, cliente.Llamadas.Count);
        }

        [TestMethod]
        [DataRow("deposito", "abc")]
        [DataRow("deposito", "")]
        [DataRow("deposito", "1 000")]
        [DataRow("deposito", "10.5.5")]
        [DataRow("deposito", null)]
        [DataRow("deposito", "1e3")]
        [DataRow("retiro", "abc")]
        [DataRow("retiro", "12a")]
        [DataRow("retiro", "$5")]
        [DataRow("transferencia", "NaN")]
        [DataRow("transferencia", "--5")]
        [DataRow("transferencia", "   ")]
        public void Ejecutar_MontoNoNumerico_DevuelveMon001SinLlamarAlServidor(string operacion, string monto)
        {
            OperacionesVista vista = Ejecutar(operacion, "CTA-1", "CTA-2", monto);
            Assert.AreEqual("[MON-001] El monto debe ser mayor que cero y con máximo dos decimales", vista.Mensaje);
            Assert.AreEqual(0, cliente.Llamadas.Count);
        }

        [TestMethod]
        [DataRow("deposito", "RealizarDeposito")]
        [DataRow("retiro", "RealizarRetiro")]
        [DataRow("transferencia", "RealizarTransferencia")]
        public void Ejecutar_MontoConTresDecimales_SeEnviaExacto(string operacion, string metodo)
        {
            Ejecutar(operacion, "CTA-1", "CTA-2", "10.555");
            Assert.AreEqual(metodo, cliente.Ultima.Metodo);
            Assert.AreEqual(10.555m, cliente.Ultima.Monto);
        }

        [TestMethod]
        [DataRow("deposito", "0", 0)]
        [DataRow("deposito", "-5", -5)]
        [DataRow("retiro", "-0.001", -0.001)]
        [DataRow("retiro", "0,01", 0.01)]
        [DataRow("transferencia", "10,50", 10.5)]
        [DataRow("transferencia", "4,125", 4.125)]
        [DataRow("deposito", "+3", 3)]
        public void Ejecutar_CeroNegativoYDecimalesSeEnvianTalCual(string operacion, string monto, double esperado)
        {
            Ejecutar(operacion, "CTA-1", "CTA-2", monto);
            Assert.AreEqual((decimal)esperado, cliente.Ultima.Monto);
        }

        [TestMethod]
        public void Ejecutar_Transferencia_EnviaOrigenDestinoYToken()
        {
            Ejecutar("transferencia", "CTA-1", "CTA-2", "25");
            Assert.AreEqual("RealizarTransferencia", cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-1", cliente.Ultima.Cuenta);
            Assert.AreEqual("CTA-2", cliente.Ultima.CuentaDestino);
            Assert.AreEqual("tok-1", cliente.Ultima.Token);
        }

        [TestMethod]
        public void Ejecutar_Retiro_LlamaRealizarRetiroConLaCuenta()
        {
            Ejecutar("retiro", "CTA-7", "", "25");
            Assert.AreEqual("RealizarRetiro", cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-7", cliente.Ultima.Cuenta);
        }

        [TestMethod]
        public void Ejecutar_Exito_MuestraElMensajeLiteralDelServidor()
        {
            OperacionesVista vista = Ejecutar("deposito", "CTA-1", "", "25");
            Assert.IsTrue(vista.Exito);
            Assert.AreEqual("Depósito realizado", vista.Mensaje);
        }

        [TestMethod]
        public void Ejecutar_ErrorDelServidor_MuestraCodigoYMensaje()
        {
            cliente.RespuestaRetiro = new RespuestaMovimiento { Estado = "ERROR", Codigo = "SAL-001", Mensaje = "Saldo insuficiente" };
            OperacionesVista vista = Ejecutar("retiro", "CTA-1", "", "999");
            Assert.IsFalse(vista.Exito);
            Assert.AreEqual("[SAL-001] Saldo insuficiente", vista.Mensaje);
        }

        [TestMethod]
        public void Ejecutar_TokenExpirado_RedirigeAlAccesoYAbandonaLaSesion()
        {
            cliente.RespuestaDeposito.Codigo = "AUTH-004";
            cliente.RespuestaDeposito.Estado = "ERROR";
            cliente.RespuestaDeposito.Mensaje = "Token expirado";
            ActionResult resultado = controlador.Ejecutar("deposito", "CTA-1", "", "25");
            AsercionesWeb.AssertRedirige(resultado, "Cuenta", "Login");
            Assert.AreEqual(1, sesion.Abandonos);
        }

        [TestMethod]
        public void Ejecutar_ErrorDeServicio_NoMuestraDetalleTecnico()
        {
            cliente.RespuestaDeposito = new RespuestaMovimiento { Estado = "ERROR", Codigo = "SER-001", Mensaje = "El servicio no está disponible en este momento" };
            OperacionesVista vista = Ejecutar("deposito", "CTA-1", "", "25");
            Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", vista.Mensaje);
            Assert.IsFalse(vista.Mensaje.Contains("http"));
        }

        [TestMethod]
        public void Index_PrefiereDepositoComoOperacion()
        {
            OperacionesVista vista = AsercionesWeb.Modelo<OperacionesVista>(controlador.Index());
            Assert.AreEqual("deposito", vista.Operacion);
            Assert.AreEqual(0, cliente.Llamadas.Count);
        }
    }

    [TestClass]
    public class CostosControllerPruebas
    {
        private ClienteFalso cliente;
        private SesionFalsa sesion;
        private CostosController controlador;

        [TestInitialize]
        public void Preparar()
        {
            PreparacionMvc.RegistrarRutas();
            cliente = new ClienteFalso();
            sesion = new SesionFalsa();
            PreparacionMvc.IniciarSesion(sesion, "tok-1");
            controlador = new CostosController(cliente);
            PreparacionMvc.Asociar(controlador, new ContextoFalso(sesion, "~/Costos/Consultar", "POST"), "Costos", "Consultar");
        }

        [TestMethod]
        public void Consultar_EnviaLaCuentaYDevuelveLosDatos()
        {
            CostosVista vista = AsercionesWeb.Modelo<CostosVista>(controlador.Consultar("CTA-1"));
            Assert.AreEqual("ConsultarCostosMantenimiento", cliente.Ultima.Metodo);
            Assert.AreEqual("CTA-1", cliente.Ultima.Cuenta);
            Assert.IsNotNull(vista.Datos);
            Assert.AreEqual(100m, vista.Datos.SaldoActual);
            Assert.AreEqual(5, vista.Datos.OperacionesSinCosto);
        }

        [TestMethod]
        public void Consultar_ErrorDelServidor_MuestraErrorEnLinea()
        {
            cliente.RespuestaCostos = new RespuestaCostos { Estado = "ERROR", Codigo = "CST-002", Mensaje = "Cuenta cerrada" };
            CostosVista vista = AsercionesWeb.Modelo<CostosVista>(controlador.Consultar("CTA-1"));
            Assert.IsNull(vista.Datos);
            Assert.AreEqual("[CST-002] Cuenta cerrada", vista.Error);
        }

        [TestMethod]
        public void Consultar_TokenNoValido_RedirigeAlAcceso()
        {
            cliente.RespuestaCostos.Codigo = "AUTH-003";
            cliente.RespuestaCostos.Estado = "ERROR";
            cliente.RespuestaCostos.Mensaje = "El token de sesión no es válido";
            AsercionesWeb.AssertRedirige(controlador.Consultar("CTA-1"), "Cuenta", "Login");
            Assert.AreEqual(1, sesion.Abandonos);
        }
    }

    [TestClass]
    public class SeguridadAccionesPruebas
    {
        private static IEnumerable<Type> ControladoresMvc()
        {
            return typeof(CuentaController).Assembly.GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && !t.IsAbstract && t.Name.EndsWith("Controller"));
        }

        [TestMethod]
        public void TodoControladorMvcTieneAlMenosUnaAccion()
        {
            Assert.AreEqual(6, ControladoresMvc().Count());
        }

        [TestMethod]
        public void TodaAccionPost_LlevaValidacionAntiForgery()
        {
            int posts = 0;
            foreach (Type tipo in ControladoresMvc())
            {
                foreach (MethodInfo metodo in tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (metodo.GetCustomAttributes(typeof(HttpPostAttribute), false).Length == 0)
                    {
                        continue;
                    }
                    posts++;
                    Assert.AreEqual(1, metodo.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).Length,
                        tipo.Name + "." + metodo.Name + " no valida el token antifalsificación");
                }
            }
            Assert.AreEqual(7, posts, "Se esperaban siete acciones POST y hubo " + posts);
        }

        [TestMethod]
        public void Operaciones_ConstantesDeLaListaCerrada()
        {
            Assert.AreEqual("deposito", OperacionesController.OpDeposito);
            Assert.AreEqual("retiro", OperacionesController.OpRetiro);
            Assert.AreEqual("transferencia", OperacionesController.OpTransferencia);
        }
    }
}
