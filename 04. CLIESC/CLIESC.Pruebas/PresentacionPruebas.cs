using System;
using System.IO;
using System.Linq;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.util;
using ec.edu.monster.escritorio.vista;
using ec.edu.monster.escritorio.ws;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    [TestClass]
    public class PresentacionPruebas
    {
        private const string MensajeMonto = "[MON-001] El monto debe ser mayor que cero y con máximo dos decimales";

        [TestMethod]
        [DataRow("200", "Operación correcta", "[200] Operación correcta")]
        [DataRow("AUTH-001", "Credenciales no válidas", "[AUTH-001] Credenciales no válidas")]
        [DataRow("SAL-001", "Saldo insuficiente", "[SAL-001] Saldo insuficiente")]
        [DataRow("MON-001", "", "[MON-001] ")]
        public void Mensaje_MuestraCodigoYMensajeTalComoLlegan(string codigo, string mensaje, string esperado)
        {
            Assert.AreEqual(esperado, Presentacion.Mensaje(new RespuestaBase("ERROR", codigo, mensaje)));
        }

        [TestMethod]
        public void Mensaje_RespuestaNulaMuestraElServicioNoDisponibleSinDetalle()
        {
            Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", Presentacion.Mensaje(null));
        }

        [TestMethod]
        [DataRow("200", true)]
        [DataRow("201", false)]
        [DataRow("AUTH-003", false)]
        [DataRow("SER-001", false)]
        [DataRow("", false)]
        public void EsExitosa_SoloCuentaElCodigo200(string codigo, bool esperado)
        {
            Assert.AreEqual(esperado, Presentacion.EsExitosa(new RespuestaBase("OK", codigo, "x")));
        }

        [TestMethod]
        public void EsExitosa_NulaEsFalsa()
        {
            Assert.IsFalse(Presentacion.EsExitosa(null));
        }

        [TestMethod]
        [DataRow("AUTH-003", true)]
        [DataRow("AUTH-004", true)]
        [DataRow("AUTH-001", false)]
        [DataRow("AUTH-002", false)]
        [DataRow("SER-001", false)]
        [DataRow("200", false)]
        [DataRow("MON-001", false)]
        public void EsSesionInvalida_SoloAuth003YAuth004(string codigo, bool esperado)
        {
            Assert.AreEqual(esperado, Presentacion.EsSesionInvalida(new RespuestaBase("ERROR", codigo, "x")));
        }

        [TestMethod]
        public void EsSesionInvalida_NulaEsFalsa()
        {
            Assert.IsFalse(Presentacion.EsSesionInvalida(null));
        }

        [TestMethod]
        [DataRow("10", 10.0)]
        [DataRow("10,50", 10.5)]
        [DataRow("0", 0.0)]
        [DataRow("-5", -5.0)]
        [DataRow("10.555", 10.555)]
        [DataRow("  3.25  ", 3.25)]
        [DataRow("1500", 1500.0)]
        public void TryMonto_NumeroSeEnviaTalCual(string texto, double esperado)
        {
            decimal monto;
            string error;
            Assert.IsTrue(Presentacion.TryMonto(texto, out monto, out error));
            Assert.IsNull(error);
            Assert.AreEqual((decimal)esperado, monto);
        }

        [TestMethod]
        [DataRow("abc")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("12a")]
        [DataRow("$5")]
        [DataRow("cinco")]
        [DataRow(null)]
        public void TryMonto_NoNumeroDaMON001SinEnviar(string texto)
        {
            decimal monto;
            string error;
            Assert.IsFalse(Presentacion.TryMonto(texto, out monto, out error));
            Assert.AreEqual(MensajeMonto, error);
        }

        [TestMethod]
        public void FilaCuenta_FormateaSaldoConDosDecimales()
        {
            var cuenta = new CuentaDTO { Codigo = "CTA-1", Moneda = "USD", Saldo = 1234.5m, Estado = "ACTIVA" };
            string[] fila = Presentacion.FilaCuenta(cuenta);
            CollectionAssert.AreEqual(new[] { "CTA-1", "USD", "1234.50", "ACTIVA" }, fila);
        }

        [TestMethod]
        [DataRow(0, "0.00")]
        [DataRow(1, "1.00")]
        [DataRow(10.5, "10.50")]
        [DataRow(99.999, "100.00")]
        [DataRow(-3, "-3.00")]
        public void FilaCuenta_SaldosSeMuestranConDosDecimales(double saldo, string esperado)
        {
            string[] fila = Presentacion.FilaCuenta(new CuentaDTO { Codigo = "C", Saldo = (decimal)saldo });
            Assert.AreEqual(esperado, fila[2]);
        }

        [TestMethod]
        public void FilaCuenta_CamposNulosSeMuestranVacios()
        {
            string[] fila = Presentacion.FilaCuenta(new CuentaDTO());
            Assert.AreEqual(string.Empty, fila[0]);
            Assert.AreEqual(string.Empty, fila[1]);
            Assert.AreEqual("0.00", fila[2]);
            Assert.AreEqual(string.Empty, fila[3]);
        }

        [TestMethod]
        public void FilaMovimiento_TieneCincoColumnasEnElOrdenPedido()
        {
            var movimiento = new MovimientoDTO
            {
                Numero = 12,
                Fecha = "2026-11-04",
                TipoCodigo = "DEP",
                TipoDescripcion = "Depósito",
                TipoAccion = "CREDITO",
                Importe = 25.5m
            };
            CollectionAssert.AreEqual(new[] { "12", "2026-11-04", "Depósito", "CREDITO", "25.50" }, Presentacion.FilaMovimiento(movimiento));
        }

        [TestMethod]
        public void Tipo_SinDescripcionUsaElCodigo()
        {
            Assert.AreEqual("RET", Presentacion.Tipo(new MovimientoDTO { TipoCodigo = "RET" }));
        }

        [TestMethod]
        public void Tipo_ConDescripcionUsaLaDescripcion()
        {
            Assert.AreEqual("Retiro", Presentacion.Tipo(new MovimientoDTO { TipoCodigo = "RET", TipoDescripcion = "Retiro" }));
        }

        [TestMethod]
        [DataRow(1, "1")]
        [DataRow(250, "250")]
        [DataRow(0, "0")]
        public void FilaMovimiento_NumeroSinDecimales(int numero, string esperado)
        {
            Assert.AreEqual(esperado, Presentacion.FilaMovimiento(new MovimientoDTO { Numero = numero })[0]);
        }

        [TestMethod]
        public void DetalleCuenta_TieneSieteCamposEnOrden()
        {
            var cuenta = new CuentaDTO
            {
                Codigo = "CTA-9",
                Moneda = "USD",
                Saldo = 80m,
                Estado = "BLOQUEADA",
                NombreCliente = "Juan Ruiz",
                CodigoSucursal = "S01",
                FechaCreacion = "2024-01-02"
            };
            var pares = Presentacion.DetalleCuenta(cuenta);
            CollectionAssert.AreEqual(
                new[] { "Código", "Moneda", "Saldo", "Estado", "Titular", "Sucursal", "Creada" },
                pares.Select(p => p.Key).ToArray());
            CollectionAssert.AreEqual(
                new[] { "CTA-9", "USD", "80.00", "BLOQUEADA", "Juan Ruiz", "S01", "2024-01-02" },
                pares.Select(p => p.Value).ToArray());
        }

        [TestMethod]
        public void Costos_TieneSeisIndicadoresEnOrden()
        {
            var costos = new RespuestaCostos
            {
                SaldoActual = 100m,
                CostoMantenimiento = 2m,
                LimiteMantenimiento = 500m,
                CostoMovimiento = 0.25m,
                OperacionesSinCosto = 5,
                InteresProyectado = 0.4m
            };
            var pares = Presentacion.Costos(costos);
            CollectionAssert.AreEqual(
                new[] { "Saldo actual", "Costo de mantenimiento", "Límite de mantenimiento", "Costo por movimiento", "Operaciones sin costo", "Interés proyectado" },
                pares.Select(p => p.Key).ToArray());
            CollectionAssert.AreEqual(
                new[] { "100.00", "2.00", "500.00", "0.25", "5", "0.40" },
                pares.Select(p => p.Value).ToArray());
        }

        [TestMethod]
        public void Costos_RespuestaVaciaMuestraCeros()
        {
            var pares = Presentacion.Costos(new RespuestaCostos());
            Assert.AreEqual(6, pares.Count);
            Assert.AreEqual("0.00", pares[0].Value);
            Assert.AreEqual("0", pares[4].Value);
        }

        [TestMethod]
        public void FalloInesperado_MuestraSoloElMensajeDeServicio()
        {
            string ruta = Path.Combine(Path.GetTempPath(), "cliesc-pruebas-" + Guid.NewGuid().ToString("N") + ".log");
            string previa = Registro.Ruta;
            try
            {
                Registro.Ruta = ruta;
                string texto = Presentacion.FalloInesperado(new InvalidOperationException("detalle tecnico secreto"), "Prueba.Contexto");

                Assert.AreEqual("[SER-001] El servicio no está disponible en este momento", texto);
                Assert.IsFalse(texto.Contains("detalle tecnico"));
                Assert.IsTrue(File.Exists(ruta));
                string registro = File.ReadAllText(ruta);
                Assert.IsTrue(registro.Contains("Prueba.Contexto"));
                Assert.IsTrue(registro.Contains("detalle tecnico secreto"));
            }
            finally
            {
                Registro.Ruta = previa;
                if (File.Exists(ruta))
                {
                    File.Delete(ruta);
                }
            }
        }

        [TestMethod]
        public void Constantes_TextosDeEstadoSonLosPedidos()
        {
            Assert.AreEqual("Consultando...", Presentacion.Consultando);
            Assert.AreEqual("Sin resultados", Presentacion.SinResultados);
            Assert.AreEqual("200", Presentacion.CodigoExito);
            Assert.AreEqual(ClienteWSEurekabank.CodigoServicioNoDisponible, "SER-001");
        }
    }
}
