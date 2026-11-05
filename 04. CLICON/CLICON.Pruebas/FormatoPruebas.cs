using System.Globalization;
using ec.edu.monster.consola.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.consola.pruebas
{
    [TestClass]
    public class FormatoPruebas
    {
        [TestMethod]
        [DataRow("abc")]
        [DataRow("12a")]
        [DataRow("a12")]
        [DataRow("1e3")]
        [DataRow("$5")]
        [DataRow("NaN")]
        [DataRow("1,2,3")]
        [DataRow("10.5.5")]
        [DataRow("--5")]
        [DataRow("5-")]
        [DataRow("1 000")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("\t")]
        [DataRow((string)null)]
        public void TryLeerMonto_TextoNoNumerico_DevuelveFalso(string texto)
        {
            decimal monto;
            Assert.IsFalse(Formato.TryLeerMonto(texto, out monto));
        }

        [TestMethod]
        [DataRow("10.555", "10.555")]
        [DataRow("0", "0")]
        [DataRow("-5", "-5")]
        [DataRow("0.001", "0.001")]
        [DataRow("10,50", "10.50")]
        [DataRow("10,5", "10.5")]
        [DataRow("  7  ", "7")]
        [DataRow("1000", "1000")]
        [DataRow("123456.789", "123456.789")]
        [DataRow("-0.001", "-0.001")]
        [DataRow("+3", "3")]
        [DataRow("0,01", "0.01")]
        [DataRow("99999999.99", "99999999.99")]
        [DataRow("4,125", "4.125")]
        [DataRow("-12,5", "-12.5")]
        public void TryLeerMonto_NumeroValido_DevuelveVerdaderoConValorExacto(string texto, string esperado)
        {
            decimal monto;
            Assert.IsTrue(Formato.TryLeerMonto(texto, out monto));
            Assert.AreEqual(decimal.Parse(esperado, CultureInfo.InvariantCulture), monto);
            Assert.AreEqual(esperado, monto.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TryLeerMonto_NoRedondeaValoresConTresDecimales()
        {
            decimal monto;
            Assert.IsTrue(Formato.TryLeerMonto("10.555", out monto));
            Assert.AreEqual(10.555m, monto);
            Assert.AreEqual("10.555", monto.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void TryLeerMonto_CeroYNegativoSeEnvianTalCual()
        {
            decimal cero;
            decimal negativo;
            Assert.IsTrue(Formato.TryLeerMonto("0", out cero));
            Assert.IsTrue(Formato.TryLeerMonto("-5", out negativo));
            Assert.AreEqual(0m, cero);
            Assert.AreEqual(-5m, negativo);
        }

        [TestMethod]
        public void TryLeerMonto_TextoInvalidoDejaMontoEnCero()
        {
            decimal monto = 99m;
            Assert.IsFalse(Formato.TryLeerMonto("abc", out monto));
            Assert.AreEqual(0m, monto);
        }

        [TestMethod]
        [DataRow(10.5, "10.50")]
        [DataRow(0.0, "0.00")]
        [DataRow(1.0, "1.00")]
        [DataRow(1234.5, "1234.50")]
        [DataRow(-5.0, "-5.00")]
        [DataRow(100.0, "100.00")]
        [DataRow(0.1, "0.10")]
        public void Monto_FormateaConDosDecimales(double valor, string esperado)
        {
            Assert.AreEqual(esperado, Formato.Monto((decimal)valor));
        }

        [TestMethod]
        [DataRow("10.555", "10.56")]
        [DataRow("10.554", "10.55")]
        [DataRow("-2.345", "-2.35")]
        [DataRow("0.005", "0.01")]
        [DataRow("0.001", "0.00")]
        [DataRow("2.5", "2.50")]
        [DataRow("99.999", "100.00")]
        [DataRow("1.115", "1.12")]
        public void Monto_RedondeaAlejandoDeCeroEnElMedio(string valor, string esperado)
        {
            Assert.AreEqual(esperado, Formato.Monto(decimal.Parse(valor, CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        [DataRow("200", "OK", "[200] OK")]
        [DataRow("AUTH-001", "Usuario o clave incorrectos", "[AUTH-001] Usuario o clave incorrectos")]
        [DataRow("SER-001", "Error interno", "[SER-001] Error interno")]
        [DataRow("MON-001", "El monto debe ser mayor que cero y con máximo dos decimales", "[MON-001] El monto debe ser mayor que cero y con máximo dos decimales")]
        [DataRow("", "", "[] ")]
        [DataRow("MOV-010", "", "[MOV-010] ")]
        [DataRow("", "Sin código", "[] Sin código")]
        [DataRow("AUTH-004", "Token expirado", "[AUTH-004] Token expirado")]
        public void Resultado_ArmaCodigoEntreCorchetes(string codigo, string mensaje, string esperado)
        {
            Assert.AreEqual(esperado, Formato.Resultado(codigo, mensaje));
        }

        [TestMethod]
        public void Resultado_CodigoNuloNoLanzaExcepcion()
        {
            Assert.AreEqual("[] Mensaje", Formato.Resultado(null, "Mensaje"));
        }

        [TestMethod]
        public void Resultado_MensajeNuloNoLanzaExcepcion()
        {
            Assert.AreEqual("[200] ", Formato.Resultado("200", null));
        }

        [TestMethod]
        [DataRow("texto", "texto")]
        [DataRow("", "")]
        [DataRow("Ñandú", "Ñandú")]
        [DataRow(null, "")]
        public void Texto_NuloSeConvierteEnCadenaVacia(string valor, string esperado)
        {
            Assert.AreEqual(esperado, Formato.Texto(valor));
        }

        [TestMethod]
        public void CodigoYMensajeDeMontoInvalidoSonLosDelContrato()
        {
            Assert.AreEqual("MON-001", Formato.CodigoMontoInvalido);
            Assert.AreEqual("El monto debe ser mayor que cero y con máximo dos decimales", Formato.MensajeMontoInvalido);
        }
    }
}
