using System.Linq;
using System.Text;
using ec.edu.monster.modelos;
using ec.edu.monster.utilidades;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.utilidades
{
    [TestClass]
    public class UtilidadesTest
    {
        [TestMethod]
        [DataRow("Cromero2026", "891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4")]
        [DataRow("Castro2026", "d4b83a0cf9194ca8c9d5cb3f714596cfeb6543df33b618f9b05ef174df87aea4")]
        [DataRow("Ramos2026", "f0bd8805123716f1d5237930643ae643b43c7f378ff8d387090eacfa73a128ee")]
        [DataRow("Cruz2026", "c6a5a412bea19bfa4133c0a15a97c20ddd49dc519330e07d82bdd9db926cc0ac")]
        public void Sha256_DaElHashDeLaSemillaDelContrato(string clave, string esperado)
        {
            Assert.AreEqual(esperado, HashClave.Sha256(clave));
        }

        [TestMethod]
        public void Sha256_ConNulo_Falla()
        {
            Assert.ThrowsException<System.ArgumentNullException>(() => HashClave.Sha256(null));
        }

        [TestMethod]
        public void Coincide_ConClaveEnClaro()
        {
            Assert.IsTrue(HashClave.Coincide("Cromero2026", HashClave.Sha256("Cromero2026")));
        }

        [TestMethod]
        public void Coincide_ConHashHexadecimal()
        {
            string hash = HashClave.Sha256("Cromero2026");
            Assert.IsTrue(HashClave.Coincide(hash, hash));
        }

        [TestMethod]
        public void Coincide_ConHashEnMayusculas_NoCoincide()
        {
            string hash = HashClave.Sha256("Cromero2026");
            Assert.IsFalse(HashClave.Coincide(hash.ToUpperInvariant(), hash));
        }

        [TestMethod]
        public void Coincide_ToleraEspaciosAlrededorDelHash()
        {
            string hash = HashClave.Sha256("Cromero2026");
            Assert.IsTrue(HashClave.Coincide("  " + hash + " ", hash));
        }

        [TestMethod]
        public void Coincide_NoToleraEspacioFinalEnLaClaveEnClaro()
        {
            Assert.IsFalse(HashClave.Coincide("Cromero2026 ", HashClave.Sha256("Cromero2026")));
        }

        [TestMethod]
        [DataRow("incorrecta")]
        [DataRow("cromero2026")]
        [DataRow("")]
        public void Coincide_ConClaveIncorrecta_Falla(string clave)
        {
            Assert.IsFalse(HashClave.Coincide(clave, HashClave.Sha256("Cromero2026")));
        }

        [TestMethod]
        public void Coincide_ConNulos_Falla()
        {
            Assert.IsFalse(HashClave.Coincide(null, "x"));
            Assert.IsFalse(HashClave.Coincide("x", null));
        }

        [TestMethod]
        public void Coincide_ConHashSha1Heredado_NuncaCoincide()
        {
            Assert.IsFalse(HashClave.Coincide("cualquiera", "32b3491336522e073489725b5daf298cd749007a"));
        }

        [TestMethod]
        public void IgualesEnTiempoConstante_CasosBasicos()
        {
            Assert.IsTrue(HashClave.IgualesEnTiempoConstante(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 3 }));
            Assert.IsFalse(HashClave.IgualesEnTiempoConstante(new byte[] { 1, 2, 3 }, new byte[] { 1, 2, 4 }));
            Assert.IsFalse(HashClave.IgualesEnTiempoConstante(new byte[] { 1, 2, 3 }, new byte[] { 1, 2 }));
            Assert.IsFalse(HashClave.IgualesEnTiempoConstante(new byte[] { 1, 2 }, new byte[] { 1, 2, 0 }));
            Assert.IsTrue(HashClave.IgualesEnTiempoConstante(new byte[0], new byte[0]));
            Assert.IsFalse(HashClave.IgualesEnTiempoConstante(null, new byte[0]));
        }

        [TestMethod]
        [DataRow("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", true)]
        [DataRow("0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF", true)]
        [DataRow("0123456789abcdef", false)]
        [DataRow("g123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", false)]
        [DataRow("", false)]
        [DataRow(null, false)]
        public void EsHashSha256(string texto, bool esperado)
        {
            Assert.AreEqual(esperado, HashClave.EsHashSha256(texto));
        }

        [TestMethod]
        public void AHexadecimal_UsaMinusculasYDosDigitosPorByte()
        {
            Assert.AreEqual("000fff", HashClave.AHexadecimal(new byte[] { 0, 15, 255 }));
        }

        [TestMethod]
        [DataRow("10.005", "10.01")]
        [DataRow("10.004", "10.00")]
        [DataRow("0.125", "0.13")]
        [DataRow("2.675", "2.68")]
        [DataRow("-0.005", "-0.01")]
        [DataRow("1", "1.00")]
        public void ADosDecimales_RedondeaHaciaArribaEnElPuntoMedio(string entrada, string esperado)
        {
            Assert.AreEqual(D(esperado), FormatoDinero.ADosDecimales(D(entrada)));
            Assert.AreEqual(2, EscalaDe(FormatoDinero.ADosDecimales(D(entrada))));
        }

        [TestMethod]
        public void ADosDecimales_ConNuloNullable_DevuelveNulo()
        {
            Assert.IsNull(FormatoDinero.ADosDecimales((decimal?)null));
            Assert.AreEqual(1.50m, FormatoDinero.ADosDecimales((decimal?)1.499m + 0.001m));
        }

        [TestMethod]
        public void SumarYRestar_MantienenDosDecimales()
        {
            Assert.AreEqual(3.30m, FormatoDinero.Sumar(1.1m, 2.2m));
            Assert.AreEqual(-1.10m, FormatoDinero.Restar(1.1m, 2.2m));
            Assert.AreEqual(2, EscalaDe(FormatoDinero.Sumar(1m, 2m)));
        }

        [TestMethod]
        [DataRow("100", "0.08", "0.08")]
        [DataRow("500", "0.08", "0.40")]
        [DataRow("1", "0.08", "0.00")]
        [DataRow("6.25", "0.08", "0.01")]
        [DataRow("5000", "0.08", "4.00")]
        [DataRow("1000", "0.7", "7.00")]
        public void Porcentaje_CalculaItfEInteres(string baseImporte, string porcentaje, string esperado)
        {
            Assert.AreEqual(D(esperado), FormatoDinero.Porcentaje(D(baseImporte), D(porcentaje)));
        }

        [TestMethod]
        [DataRow("0.01", true)]
        [DataRow("1", true)]
        [DataRow("10.50", true)]
        [DataRow("10.5", true)]
        [DataRow("10.500", true)]
        [DataRow("79228162514264337593543950.33", true)]
        [DataRow("0", false)]
        [DataRow("0.00", false)]
        [DataRow("-1", false)]
        [DataRow("-0.01", false)]
        [DataRow("10.555", false)]
        [DataRow("0.001", false)]
        [DataRow("10.001", false)]
        public void EsMontoValido(string monto, bool esperado)
        {
            Assert.AreEqual(esperado, FormatoDinero.EsMontoValido(D(monto)));
        }

        [TestMethod]
        public void EsMontoValido_ConNulo_EsFalso()
        {
            Assert.IsFalse(FormatoDinero.EsMontoValido(null));
        }

        [TestMethod]
        public void Cero_TieneEscalaDos()
        {
            Assert.AreEqual(2, EscalaDe(FormatoDinero.Cero));
        }

        [TestMethod]
        [DataRow(null, true)]
        [DataRow("", true)]
        [DataRow("   ", true)]
        [DataRow("\t\n", true)]
        [DataRow(" ", true)]
        [DataRow("a", false)]
        [DataRow(" a ", false)]
        [DataRow("0", false)]
        public void EstaVacio(string texto, bool esperado)
        {
            Assert.AreEqual(esperado, Textos.EstaVacio(texto));
        }

        [TestMethod]
        public void Recortar()
        {
            Assert.IsNull(Textos.Recortar(null));
            Assert.AreEqual("a b", Textos.Recortar("  a b \n"));
        }

        [TestMethod]
        public void Catalogo_TieneLosDiecisieteCodigosDelContrato()
        {
            Assert.AreEqual(17, CodigoRespuesta.Todos.Length);
            Assert.AreEqual(17, CodigoRespuesta.Todos.Select(c => c.Codigo).Distinct().Count());
        }

        [TestMethod]
        [DataRow("200", "Operación realizada correctamente", true)]
        [DataRow("DAT-001", "Los datos de la petición están incompletos o son incorrectos", false)]
        [DataRow("AUTH-001", "El usuario o la contraseña son incorrectos", false)]
        [DataRow("AUTH-002", "El usuario se encuentra inactivo", false)]
        [DataRow("AUTH-003", "El token de sesión no es válido", false)]
        [DataRow("AUTH-004", "El token de sesión ha expirado, inicie sesión nuevamente", false)]
        [DataRow("CTA-001", "La cuenta solicitada no existe", false)]
        [DataRow("CTA-002", "La cuenta no se encuentra en estado ACTIVO", false)]
        [DataRow("CTA-003", "La cuenta origen y la cuenta destino no pueden ser iguales", false)]
        [DataRow("CTA-004", "La cuenta de destino pertenece a una moneda distinta", false)]
        [DataRow("CLI-001", "El cliente solicitado no existe", false)]
        [DataRow("SAL-001", "El saldo de la cuenta es insuficiente para la operación", false)]
        [DataRow("MON-001", "El monto debe ser mayor que cero y con máximo dos decimales", false)]
        [DataRow("MON-002", "La moneda de la cuenta no está soportada por el sistema", false)]
        [DataRow("LIM-001", "El monto supera el límite máximo de mantenimiento de la cuenta", false)]
        [DataRow("SER-001", "El servicio no está disponible en este momento", false)]
        [DataRow("ERR-001", "Ocurrió un error interno, intente nuevamente", false)]
        public void Catalogo_MensajeLiteralYEstado(string codigo, string mensaje, bool exitoso)
        {
            CodigoRespuesta encontrado = CodigoRespuesta.Todos.Single(c => c.Codigo == codigo);
            Assert.AreEqual(mensaje, encontrado.Mensaje);
            Assert.AreEqual(exitoso, encontrado.Exitoso);
            Assert.AreEqual(exitoso ? "OK" : "ERROR", encontrado.Estado);
        }

        [TestMethod]
        public void Catalogo_LosMensajesSonUtf8ConTildes()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(CodigoRespuesta.Auth004.Mensaje);
            Assert.AreEqual(CodigoRespuesta.Auth004.Mensaje, Encoding.UTF8.GetString(bytes));
            Assert.IsTrue(CodigoRespuesta.Auth004.Mensaje.Contains("sesión"));
        }

        [TestMethod]
        [DataRow("Autenticación exitosa", "MensajeAutenticacionOk")]
        [DataRow("Sesión cerrada correctamente", "MensajeCerrarSesionOk")]
        [DataRow("Cuentas consultadas correctamente", "MensajeCuentasOk")]
        [DataRow("Cuenta consultada correctamente", "MensajeCuentaOk")]
        [DataRow("Movimientos consultados correctamente", "MensajeMovimientosOk")]
        [DataRow("Depósito realizado correctamente", "MensajeDepositoOk")]
        [DataRow("Retiro realizado correctamente", "MensajeRetiroOk")]
        [DataRow("Transferencia realizada correctamente", "MensajeTransferenciaOk")]
        [DataRow("Costos de mantenimiento consultados correctamente", "MensajeCostosOk")]
        public void MensajesDeExito_SonLosDelContrato(string esperado, string campo)
        {
            string valor = (string)typeof(Codigos).GetField(campo).GetValue(null);
            Assert.AreEqual(esperado, valor);
        }

        [TestMethod]
        public void Codigos_ConstantesBase()
        {
            Assert.AreEqual("http://ws.monster.edu.ec/", Codigos.EspacioDeNombres);
            Assert.AreEqual("OK", Codigos.EstadoOk);
            Assert.AreEqual("ERROR", Codigos.EstadoError);
            Assert.AreEqual("200", Codigos.CodigoOk);
        }

        private static decimal D(string texto)
        {
            return decimal.Parse(texto, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static int EscalaDe(decimal valor)
        {
            return (decimal.GetBits(valor)[3] >> 16) & 0xFF;
        }
    }
}
