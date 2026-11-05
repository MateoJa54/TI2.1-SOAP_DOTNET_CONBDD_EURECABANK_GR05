using System.Globalization;
using System.IO;
using ec.edu.monster.consola.modelo;
using ec.edu.monster.consola.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.consola.pruebas
{
    [TestClass]
    public class VistasPruebas
    {
        private const string MensajeMonto = "[MON-001] El monto debe ser mayor que cero y con máximo dos decimales";

        // Credenciales del caso de prueba; la clave se envía como hash.
        private static string Login()
        {
            return Ambiente.Lineas("ana", "Cromero2026");
        }

        private static string L(params string[] lineas)
        {
            return Ambiente.Lineas(lineas);
        }

        // Líneas que pide cada opción del menú, terminando con la opción 0 para cerrar sesión.
        private static string Operacion(string opcion, string monto)
        {
            switch (opcion)
            {
                case "1": return L("1", "1712345678", "0");
                case "2": return L("2", "CTA-1", "0");
                case "3": return L("3", "CTA-1", "0");
                case "4": return L("4", "CTA-1", monto, "0");
                case "5": return L("5", "CTA-1", monto, "0");
                case "6": return L("6", "CTA-1", "CTA-2", monto, "0");
                case "7": return L("7", "CTA-1", "0");
                default: return L(opcion, "0");
            }
        }

        private static Llamada Buscar(ClienteFalso cliente, string metodo)
        {
            foreach (Llamada l in cliente.Llamadas)
            {
                if (l.Metodo == metodo) return l;
            }
            Assert.Fail("No se llamó al método " + metodo);
            return null;
        }

        // ---------- Autenticación ----------

        [TestMethod]
        public void LoginCorrecto_MuestraBienvenidaYElMenu()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L("0"));

            StringAssert.Contains(salida, "=== EURECABANK - Inicio de sesión ===");
            StringAssert.Contains(salida, "[200] Autenticación correcta");
            StringAssert.Contains(salida, "Bienvenido, Ana Pérez (Cajero)");
            StringAssert.Contains(salida, "=== EURECABANK - Menú principal ===");
            StringAssert.Contains(salida, "Hasta luego.");
        }

        [TestMethod]
        public void LoginCorrecto_EnviaElHashDeLaClaveNoLaClaveEnClaro()
        {
            Ambiente amb = new Ambiente();

            amb.Programa(Login() + L("0"));

            Llamada login = Buscar(amb.Cliente, "Autenticar");
            Assert.AreEqual("ana", login.Usuario);
            Assert.AreEqual("891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4", login.ClaveHash);
            Assert.AreNotEqual("Cromero2026", login.ClaveHash);
        }

        [TestMethod]
        public void LoginCorrecto_RecortaEspaciosDelUsuario()
        {
            Ambiente amb = new Ambiente();

            amb.Programa(L("   ana  ", "Cromero2026", "0"));

            Assert.AreEqual("ana", Buscar(amb.Cliente, "Autenticar").Usuario);
        }

        [TestMethod]
        [DataRow("AUTH-001", "Usuario o clave incorrectos")]
        [DataRow("AUTH-002", "Usuario bloqueado por intentos fallidos")]
        [DataRow("SER-001", "Error interno del servicio")]
        public void LoginFallido_MuestraCodigoYReintenta(string codigo, string mensaje)
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.ColaAutenticar.Enqueue(new RespuestaAutenticacion { Codigo = codigo, Mensaje = mensaje, Estado = "ERROR" });

            string salida = amb.Programa(L("ana", "mal") + Login() + L("0"));

            StringAssert.Contains(salida, "[" + codigo + "] " + mensaje);
            StringAssert.Contains(salida, "Bienvenido, Ana Pérez (Cajero)");
            Assert.AreEqual(2, amb.Cliente.Cuantas("Autenticar"));
        }

        [TestMethod]
        public void LoginFallido_SinSegundoIntentoTerminaSinSesion()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.ColaAutenticar.Enqueue(new RespuestaAutenticacion { Codigo = "AUTH-001", Mensaje = "Incorrecto" });

            string salida = amb.Programa(L("ana", "mal"));

            StringAssert.Contains(salida, "[AUTH-001] Incorrecto");
            StringAssert.Contains(salida, "Hasta luego.");
            Assert.AreEqual(0, amb.Cliente.Cuantas("CerrarSesion"));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("\t")]
        public void UsuarioVacio_SaleSinLlamarAlCliente(string usuario)
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(L(usuario));

            Assert.AreEqual(0, amb.Cliente.Llamadas.Count);
            StringAssert.Contains(salida, "Hasta luego.");
        }

        [TestMethod]
        public void FinDeEntradaEnUsuario_SaleSinLlamarAlCliente()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa("");

            Assert.AreEqual(0, amb.Cliente.Llamadas.Count);
            StringAssert.Contains(salida, "Hasta luego.");
        }

        [TestMethod]
        public void ClaveNoIngresada_SaleSinLlamarAlCliente()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(L("ana"));

            Assert.AreEqual(0, amb.Cliente.Llamadas.Count);
            StringAssert.Contains(salida, "Hasta luego.");
        }

        // ---------- Menú: enrutamiento y cierre ----------

        [TestMethod]
        [DataRow("1", "ListarCuentasPorCliente")]
        [DataRow("2", "ConsultarCuenta")]
        [DataRow("3", "ConsultarMovimientos")]
        [DataRow("4", "RealizarDeposito")]
        [DataRow("5", "RealizarRetiro")]
        [DataRow("6", "RealizarTransferencia")]
        [DataRow("7", "ConsultarCostosMantenimiento")]
        public void Menu_CadaOpcionLlamaAlMetodoCorrespondiente(string opcion, string metodo)
        {
            Ambiente amb = new Ambiente();

            amb.Programa(Login() + Operacion(opcion, "10"));

            Assert.AreEqual(1, amb.Cliente.Cuantas(metodo));
        }

        [TestMethod]
        public void Menu_Opcion0CierraSesionConElToken()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L("0"));

            Assert.AreEqual(1, amb.Cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual("tok-1", Buscar(amb.Cliente, "CerrarSesion").Token);
            StringAssert.Contains(salida, "[200] Sesión cerrada correctamente");
        }

        [TestMethod]
        [DataRow("9")]
        [DataRow("x")]
        [DataRow("")]
        [DataRow("99")]
        [DataRow("00")]
        [DataRow("-1")]
        public void Menu_OpcionInvalidaAvisaYSigueElMenu(string opcion)
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L(opcion, "0"));

            StringAssert.Contains(salida, "Opción no válida.");
            Assert.AreEqual(1, amb.Cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual(0, amb.Cliente.Cuantas("ListarCuentasPorCliente"));
        }

        [TestMethod]
        [DataRow("AUTH-003")]
        [DataRow("AUTH-004")]
        public void SesionPerdida_VuelveAlLogin(string codigo)
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaListar.Codigo = codigo;
            amb.Cliente.RespuestaListar.Mensaje = "Token vencido";

            string salida = amb.Programa(Login() + L("1", "1712345678"));

            StringAssert.Contains(salida, "[" + codigo + "] Token vencido");
            StringAssert.Contains(salida, "La sesión terminó. Inicie sesión nuevamente.");
            Assert.AreEqual(0, amb.Cliente.Cuantas("CerrarSesion"));
            Assert.AreEqual(1, amb.Cliente.Cuantas("ListarCuentasPorCliente"));
        }

        [TestMethod]
        public void Auth004_DevuelveAlLoginYUnNuevoLoginFunciona()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaListar.Codigo = "AUTH-004";
            amb.Cliente.RespuestaListar.Mensaje = "Token expirado";

            // Primer login, listado que expira la sesión, segundo login y salida.
            string salida = amb.Programa(Login() + L("1", "1712345678") + Login() + L("0"));

            StringAssert.Contains(salida, "[AUTH-004] Token expirado");
            StringAssert.Contains(salida, "Inicie sesión nuevamente.");
            Assert.AreEqual(2, amb.Cliente.Cuantas("Autenticar"));
            Assert.AreEqual(1, amb.Cliente.Cuantas("CerrarSesion"));
        }

        [TestMethod]
        public void SER001_SeMuestraTalCualYLaSesionSigue()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaConsultarCuenta.Codigo = "SER-001";
            amb.Cliente.RespuestaConsultarCuenta.Mensaje = "Error interno del servicio";

            string salida = amb.Programa(Login() + L("2", "CTA-1", "0"));

            StringAssert.Contains(salida, "[SER-001] Error interno del servicio");
            Assert.AreEqual(1, amb.Cliente.Cuantas("CerrarSesion"));
        }

        // ---------- Montos ----------

        [TestMethod]
        [DataRow("4", "abc")]
        [DataRow("4", "12a")]
        [DataRow("4", "")]
        [DataRow("4", "   ")]
        [DataRow("4", "1e3")]
        [DataRow("4", "1,2,3")]
        [DataRow("5", "abc")]
        [DataRow("5", "12a")]
        [DataRow("5", "")]
        [DataRow("5", "   ")]
        [DataRow("5", "1e3")]
        [DataRow("5", "1,2,3")]
        [DataRow("6", "abc")]
        [DataRow("6", "12a")]
        [DataRow("6", "")]
        [DataRow("6", "   ")]
        [DataRow("6", "1e3")]
        [DataRow("6", "1,2,3")]
        public void MontoNoNumerico_MuestraMON001SinLlamarAlCliente(string opcion, string monto)
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + Operacion(opcion, monto));

            StringAssert.Contains(salida, MensajeMonto);
            Assert.AreEqual(0, amb.Cliente.Cuantas("RealizarDeposito"));
            Assert.AreEqual(0, amb.Cliente.Cuantas("RealizarRetiro"));
            Assert.AreEqual(0, amb.Cliente.Cuantas("RealizarTransferencia"));
            Assert.AreEqual(1, amb.Cliente.Cuantas("CerrarSesion"));
        }

        [TestMethod]
        [DataRow("4", "RealizarDeposito", "10.555", "10.555")]
        [DataRow("5", "RealizarRetiro", "10.555", "10.555")]
        [DataRow("6", "RealizarTransferencia", "10.555", "10.555")]
        [DataRow("4", "RealizarDeposito", "0", "0")]
        [DataRow("5", "RealizarRetiro", "0", "0")]
        [DataRow("6", "RealizarTransferencia", "0", "0")]
        [DataRow("4", "RealizarDeposito", "-5", "-5")]
        [DataRow("5", "RealizarRetiro", "-5", "-5")]
        [DataRow("6", "RealizarTransferencia", "-5", "-5")]
        [DataRow("4", "RealizarDeposito", "0.001", "0.001")]
        [DataRow("5", "RealizarRetiro", "0.001", "0.001")]
        [DataRow("6", "RealizarTransferencia", "0.001", "0.001")]
        [DataRow("4", "RealizarDeposito", "10,50", "10.50")]
        [DataRow("5", "RealizarRetiro", "10,50", "10.50")]
        [DataRow("6", "RealizarTransferencia", "10,50", "10.50")]
        public void MontoValido_LlegaAlClienteTalCual(string opcion, string metodo, string monto, string esperado)
        {
            Ambiente amb = new Ambiente();

            amb.Programa(Login() + Operacion(opcion, monto));

            Llamada l = Buscar(amb.Cliente, metodo);
            Assert.IsTrue(l.Monto.HasValue);
            Assert.AreEqual(decimal.Parse(esperado, CultureInfo.InvariantCulture), l.Monto.Value);
            Assert.AreEqual(esperado, l.Monto.Value.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void MontoDiezConTresDecimales_SeEnviaComoDecimalExacto()
        {
            Ambiente amb = new Ambiente();

            amb.Programa(Login() + L("4", "CTA-1", "10.555", "0"));

            Assert.AreEqual(10.555m, Buscar(amb.Cliente, "RealizarDeposito").Monto.Value);
        }

        // ---------- Tablas y resultados ----------

        [TestMethod]
        public void Cuentas_ListadoMuestraClienteYTablaDeCuentas()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaListar.Cuentas.Add(new CuentaDTO { Codigo = "CTA-1", DescripcionMoneda = "Dólar", Saldo = 1234.5m, Estado = "ACTIVA" });

            string salida = amb.Programa(Login() + Operacion("1", "10"));

            StringAssert.Contains(salida, "Cliente: Juan Ruiz (1712345678)");
            StringAssert.Contains(salida, string.Format("{0,-10}{1,-10}{2,-14}{3,-12}", "Cuenta", "Moneda", "Saldo", "Estado"));
            StringAssert.Contains(salida, string.Format("{0,-10}{1,-10}{2,-14}{3,-12}", "CTA-1", "Dólar", "1234.50", "ACTIVA"));
        }

        [TestMethod]
        public void Cuentas_ClienteSinCuentasLoIndica()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + Operacion("1", "10"));

            StringAssert.Contains(salida, "El cliente no tiene cuentas.");
        }

        [TestMethod]
        public void Cuentas_ErrorNoMuestraTabla()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaListar.Codigo = "CLI-404";
            amb.Cliente.RespuestaListar.Mensaje = "Cliente no existe";

            string salida = amb.Programa(Login() + L("1", "0000000000", "0"));

            StringAssert.Contains(salida, "[CLI-404] Cliente no existe");
            Assert.IsFalse(salida.Contains("Cliente: "));
        }

        [TestMethod]
        public void Consultar_CuentaMuestraSusDatos()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + Operacion("2", "10"));

            StringAssert.Contains(salida, "Cuenta        : CTA-1");
            StringAssert.Contains(salida, "Titular       : Juan Ruiz");
            StringAssert.Contains(salida, "Saldo         : 100.00");
            StringAssert.Contains(salida, "Operaciones   : 3");
        }

        [TestMethod]
        public void Consultar_ErrorNoMuestraDetalle()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaConsultarCuenta.Codigo = "CTA-404";
            amb.Cliente.RespuestaConsultarCuenta.Mensaje = "Cuenta no encontrada";
            amb.Cliente.RespuestaConsultarCuenta.Cuenta = null;

            string salida = amb.Programa(Login() + L("2", "CTA-9", "0"));

            StringAssert.Contains(salida, "[CTA-404] Cuenta no encontrada");
            Assert.IsFalse(salida.Contains("Titular"));
        }

        [TestMethod]
        public void Movimientos_TablaImprimeNumeroFechaTipoAccionEImporte()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaMovimientosConsulta.Movimientos.Add(new MovimientoDTO
            {
                Numero = 1,
                Fecha = "2026-01-05",
                TipoCodigo = "DEP",
                TipoDescripcion = "Depósito",
                TipoAccion = "CREDITO",
                Importe = 150.5m
            });
            amb.Cliente.RespuestaMovimientosConsulta.Movimientos.Add(new MovimientoDTO
            {
                Numero = 2,
                Fecha = "2026-01-06",
                TipoCodigo = "RET",
                TipoDescripcion = "Retiro en ventanilla",
                TipoAccion = "DEBITO",
                Importe = 20m
            });

            string salida = amb.Programa(Login() + Operacion("3", "10"));

            StringAssert.Contains(salida, string.Format("{0,-6}{1,-12}{2,-24}{3,-9}{4,12}", "Nº", "Fecha", "Tipo", "Acción", "Importe"));
            StringAssert.Contains(salida, string.Format("{0,-6}{1,-12}{2,-24}{3,-9}{4,12}", 1, "2026-01-05", "Depósito", "CREDITO", "150.50"));
            StringAssert.Contains(salida, string.Format("{0,-6}{1,-12}{2,-24}{3,-9}{4,12}", 2, "2026-01-06", "Retiro en ventanilla", "DEBITO", "20.00"));
        }

        [TestMethod]
        public void Movimientos_CuentaSinMovimientosLoIndica()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + Operacion("3", "10"));

            StringAssert.Contains(salida, "La cuenta no tiene movimientos.");
        }

        [TestMethod]
        public void Deposito_MuestraNumeroCargoYSaldoNuevo()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L("4", "CTA-1", "25.5", "0"));

            StringAssert.Contains(salida, "[200] Depósito realizado");
            StringAssert.Contains(salida, "Movimiento nº 7");
            StringAssert.Contains(salida, "Cargo por movimiento : 0.50");
            StringAssert.Contains(salida, "Saldo nuevo          : 125.50");
        }

        [TestMethod]
        public void Retiro_MuestraNumeroCargoYSaldoNuevo()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L("5", "CTA-1", "25.5", "0"));

            StringAssert.Contains(salida, "Movimiento nº 8");
            StringAssert.Contains(salida, "Saldo nuevo          : 90.00");
        }

        [TestMethod]
        public void Transferencia_MuestraMovimientosSaldosEItf()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + L("6", "CTA-1", "CTA-2", "25.5", "0"));

            StringAssert.Contains(salida, "Movimiento origen : 9");
            StringAssert.Contains(salida, "Movimiento destino : 10");
            StringAssert.Contains(salida, "ITF aplicado      : 0.10");
            StringAssert.Contains(salida, "Saldo origen      : 50.00");
            StringAssert.Contains(salida, "Saldo destino     : 60.00");
        }

        [TestMethod]
        public void Costos_MuestraLosCostosDeLaCuenta()
        {
            Ambiente amb = new Ambiente();

            string salida = amb.Programa(Login() + Operacion("7", "10"));

            StringAssert.Contains(salida, "Saldo actual              : 100.00");
            StringAssert.Contains(salida, "Costo de mantenimiento    : 2.00");
            StringAssert.Contains(salida, "Límite de mantenimiento   : 500.00");
            StringAssert.Contains(salida, "Costo por movimiento      : 0.25");
            StringAssert.Contains(salida, "Interés mensual proyectado: 0.40");
        }

        [TestMethod]
        public void Costos_ErrorNoMuestraDetalle()
        {
            Ambiente amb = new Ambiente();
            amb.Cliente.RespuestaCostos.Codigo = "CTA-404";
            amb.Cliente.RespuestaCostos.Mensaje = "Cuenta no encontrada";

            string salida = amb.Programa(Login() + L("7", "CTA-9", "0"));

            StringAssert.Contains(salida, "[CTA-404] Cuenta no encontrada");
            Assert.IsFalse(salida.Contains("Saldo actual"));
        }
    }

    [TestClass]
    public class ConsolaPruebas
    {
        [TestMethod]
        public void Leer_DevuelveLaLineaYEscribeLaEtiqueta()
        {
            StringWriter salida = new StringWriter();
            Consola consola = new Consola(new StringReader("hola\n"), salida);

            Assert.AreEqual("hola", consola.Leer("Nombre"));
            StringAssert.Contains(salida.ToString(), "Nombre: ");
        }

        [TestMethod]
        public void Leer_FinDeEntradaDevuelveNulo()
        {
            Consola consola = new Consola(new StringReader(""), new StringWriter());
            Assert.IsNull(consola.Leer("Nombre"));
        }

        [TestMethod]
        public void LeerSecreto_ConEntradaRedirigidaLeeDeLaMismaEntrada()
        {
            StringWriter salida = new StringWriter();
            Consola consola = new Consola(new StringReader("secreto\n"), salida);

            Assert.AreEqual("secreto", consola.LeerSecreto("Clave"));
            StringAssert.Contains(salida.ToString(), "Clave: ");
        }

        [TestMethod]
        public void LeerSecreto_DespuesDeLeerSigueConLaSiguienteLinea()
        {
            Consola consola = new Consola(new StringReader("usuario\nclave\n"), new StringWriter());

            Assert.AreEqual("usuario", consola.Leer("Usuario"));
            Assert.AreEqual("clave", consola.LeerSecreto("Clave"));
        }

        [TestMethod]
        public void LeerSecreto_FinDeEntradaDevuelveNulo()
        {
            Consola consola = new Consola(new StringReader(""), new StringWriter());
            Assert.IsNull(consola.LeerSecreto("Clave"));
        }

        [TestMethod]
        public void LeerSecreto_LineaVaciaDevuelveCadenaVacia()
        {
            Consola consola = new Consola(new StringReader("\n"), new StringWriter());
            Assert.AreEqual("", consola.LeerSecreto("Clave"));
        }

        [TestMethod]
        public void Linea_EscribeTextoYSaltoDeLinea()
        {
            StringWriter salida = new StringWriter();
            new Consola(new StringReader(""), salida).Linea("uno");
            Assert.AreEqual("uno" + System.Environment.NewLine, salida.ToString());
        }

        [TestMethod]
        public void Linea_SinArgumentoEscribeSoloSaltoDeLinea()
        {
            StringWriter salida = new StringWriter();
            new Consola(new StringReader(""), salida).Linea();
            Assert.AreEqual(System.Environment.NewLine, salida.ToString());
        }

        [TestMethod]
        public void Escribir_NoAgregaSaltoDeLinea()
        {
            StringWriter salida = new StringWriter();
            new Consola(new StringReader(""), salida).Escribir("sin salto");
            Assert.AreEqual("sin salto", salida.ToString());
        }
    }
}
