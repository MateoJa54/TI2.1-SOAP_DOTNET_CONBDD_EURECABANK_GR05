using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ec.edu.monster.modelos;
using ec.edu.monster.pruebas.Apoyo;
using ec.edu.monster.servicios;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.pruebas.servicios
{
    public abstract class MovimientosBase
    {
        protected Entorno entorno;
        protected string token;

        [TestInitialize]
        public void Preparar()
        {
            entorno = new Entorno();
            token = entorno.Entrar();
        }

        protected static decimal D(string texto)
        {
            return decimal.Parse(texto, CultureInfo.InvariantCulture);
        }

        protected ServicioMovimientos S
        {
            get { return entorno.Movimientos; }
        }

        protected void SinEscrituras()
        {
            Assert.AreEqual(0, entorno.Base.EjecucionesDeUnidad, "no debe abrir una transacción");
        }
    }

    [TestClass]
    public class ConsultarMovimientosTest : MovimientosBase
    {
        [TestMethod]
        public void CuentaActiva_DevuelveSusMovimientos()
        {
            entorno.Base.Movimientos["00200002"].Add(new MovimientoDTO("0001", "2026-01-02", 10m, 1, null, "INGRESO", "003", "Deposito"));
            RespuestaMovimientos r = S.ConsultarMovimientos("00200002", token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Movimientos consultados correctamente", r.Mensaje);
            Assert.AreEqual("00200002", r.Cuenta);
            Assert.AreEqual(1, r.Movimientos.Count);
        }

        [TestMethod]
        public void CuentaSinMovimientos_DevuelveListaVacia()
        {
            Assert.AreEqual(0, S.ConsultarMovimientos("00200001", token).Movimientos.Count);
        }

        [TestMethod]
        public void RecortaElCodigoDeCuenta()
        {
            Assert.AreEqual("00200002", S.ConsultarMovimientos(" 00200002 ", token).Cuenta);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("", "DAT-001")]
        [DataRow("  ", "DAT-001")]
        [DataRow("11111111", "CTA-001")]
        [DataRow("00300001", "CTA-002")]
        public void Errores(string cuenta, string esperado)
        {
            RespuestaMovimientos r = S.ConsultarMovimientos(cuenta, token);
            Assert.AreEqual(esperado, r.Codigo);
            Assert.AreEqual(0, r.Movimientos.Count);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, S.ConsultarMovimientos("00200002", tokenMalo).Codigo);
        }

        [TestMethod]
        public void TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(31));
            Assert.AreEqual("AUTH-004", S.ConsultarMovimientos("00200002", token).Codigo);
        }
    }

    [TestClass]
    public class DepositoTest : MovimientosBase
    {
        [TestMethod]
        public void Exito_SumaAlSaldoYRegistraElMovimiento003()
        {
            RespuestaMovimiento r = S.RealizarDeposito("00200001", 50m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Depósito realizado correctamente", r.Mensaje);
            Assert.AreEqual(150.00m, r.SaldoNuevo);
            Assert.AreEqual(0.00m, r.CostoAplicado);
            Assert.AreEqual(0.00m, r.ItfAplicado);
            Assert.AreEqual(3, r.Movimiento.Numero);
            Assert.AreEqual("003", r.Movimiento.TipoCodigo);
            Assert.AreEqual("INGRESO", r.Movimiento.TipoAccion);
            Assert.AreEqual("2026-11-03", r.Movimiento.Fecha);
            Assert.AreEqual("0001", r.Movimiento.Empleado);
            Assert.IsNull(r.Movimiento.Referencia);
            Assert.AreEqual(150m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(3, entorno.Base.Contador("00200001"));
            Assert.AreEqual(1, entorno.Base.De("00200001").Count);
        }

        [TestMethod]
        public void NoCobraItf()
        {
            Assert.AreEqual(0.00m, S.RealizarDeposito("00200002", 1000m, token).ItfAplicado);
            Assert.AreEqual(1, entorno.Base.De("00200002").Count);
        }

        [TestMethod]
        [DataRow("0.01", "100.01")]
        [DataRow("10.5", "110.50")]
        [DataRow("10.50", "110.50")]
        [DataRow("10.500", "110.50")]
        [DataRow("999999.99", "1000099.99")]
        public void MontosValidos(string monto, string saldo)
        {
            RespuestaMovimiento r = S.RealizarDeposito("00200001", D(monto), token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(D(saldo), r.SaldoNuevo);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("0.00")]
        [DataRow("-1")]
        [DataRow("-0.01")]
        [DataRow("10.555")]
        [DataRow("0.001")]
        [DataRow("1.005")]
        public void MontosInvalidos_SonMon001_SinTocarLaBase(string monto)
        {
            RespuestaMovimiento r = S.RealizarDeposito("00200001", D(monto), token);
            Assert.AreEqual("MON-001", r.Codigo);
            Assert.AreEqual("El monto debe ser mayor que cero y con máximo dos decimales", r.Mensaje);
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            SinEscrituras();
        }

        [TestMethod]
        public void MontoNulo_EsDat001()
        {
            Assert.AreEqual("DAT-001", S.RealizarDeposito("00200001", null, token).Codigo);
            SinEscrituras();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(" ")]
        public void CuentaVacia_EsDat001(string cuenta)
        {
            Assert.AreEqual("DAT-001", S.RealizarDeposito(cuenta, 10m, token).Codigo);
        }

        [TestMethod]
        public void CuentaInexistente_EsCta001()
        {
            Assert.AreEqual("CTA-001", S.RealizarDeposito("11111111", 10m, token).Codigo);
        }

        [TestMethod]
        public void CuentaCancelada_EsCta002()
        {
            RespuestaMovimiento r = S.RealizarDeposito("00300001", 10m, token);
            Assert.AreEqual("CTA-002", r.Codigo);
            Assert.AreEqual(50m, entorno.Base.Saldo("00300001"));
            SinEscrituras();
        }

        [TestMethod]
        public void MonedaNoSoportada_EsMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("MON-002", S.RealizarDeposito("00200001", 10m, token).Codigo);
            SinEscrituras();
        }

        [TestMethod]
        public void Orden_Mon001GanaSobreCta001()
        {
            Assert.AreEqual("MON-001", S.RealizarDeposito("11111111", -5m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Dat001GanaSobreMon001()
        {
            Assert.AreEqual("DAT-001", S.RealizarDeposito("", -5m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Auth003GanaSobreTodo()
        {
            Assert.AreEqual("AUTH-003", S.RealizarDeposito("", -5m, "falso").Codigo);
        }

        [TestMethod]
        public void Orden_Cta002GanaSobreMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("CTA-002", S.RealizarDeposito("00300001", 10m, token).Codigo);
        }

        [TestMethod]
        public void TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(30));
            Assert.AreEqual("AUTH-004", S.RealizarDeposito("00200001", 10m, token).Codigo);
        }

        [TestMethod]
        public void MovimientoDieciseis_CobraElCargoYRegistraElMovimiento010()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaMovimiento r = S.RealizarDeposito("00200001", 50m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(2.00m, r.CostoAplicado);
            Assert.AreEqual(148.00m, r.SaldoNuevo);
            Assert.AreEqual(16, r.Movimiento.Numero);
            Assert.AreEqual(17, entorno.Base.Contador("00200001"));
            List<MovimientoDTO> movimientos = entorno.Base.De("00200001");
            Assert.AreEqual(2, movimientos.Count);
            Assert.AreEqual("010", movimientos[1].TipoCodigo);
            Assert.AreEqual(17, movimientos[1].Numero);
            Assert.AreEqual(2.00m, movimientos[1].Importe);
        }

        [TestMethod]
        public void MovimientoQuince_EsSinCosto()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 14;
            RespuestaMovimiento r = S.RealizarDeposito("00200001", 50m, token);
            Assert.AreEqual(0.00m, r.CostoAplicado);
            Assert.AreEqual(150.00m, r.SaldoNuevo);
            Assert.AreEqual(15, entorno.Base.Contador("00200001"));
        }

        [TestMethod]
        public void CargoEnDolares_UsaElCostoDeSuMoneda()
        {
            entorno.Base.Cuentas["00100002"].OperacionesRealizadas = 20;
            RespuestaMovimiento r = S.RealizarDeposito("00100002", 10m, token);
            Assert.AreEqual(0.60m, r.CostoAplicado);
            Assert.AreEqual(809.40m, r.SaldoNuevo);
        }

        [TestMethod]
        public void SinParametroDeOperaciones_TodoMovimientoTieneCargo()
        {
            entorno.Base.Parametros.Remove("002");
            Assert.AreEqual(2.00m, S.RealizarDeposito("00200001", 10m, token).CostoAplicado);
        }

        [TestMethod]
        public void SaldoNegativoImposible_ElCargoMayorQueSaldoMasMontoEsSal001()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 0m;
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 20;
            RespuestaMovimiento r = S.RealizarDeposito("00200001", 1m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual(0m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(0, entorno.Base.De("00200001").Count);
        }

        [TestMethod]
        public void DepositoQueCubreExactoElCargo_DejaSaldoCero()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 0m;
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 20;
            RespuestaMovimiento r = S.RealizarDeposito("00200001", 2m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.SaldoNuevo);
        }

        [TestMethod]
        public void FalloAlInsertar_DeshaceTodo_YResponderiaErr001EnLaFachada()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            entorno.Base.FallarAlInsertarMovimiento = true;
            entorno.Base.InsercionesHastaFallar = 2;
            Assert.ThrowsException<InvalidOperationException>(() => S.RealizarDeposito("00200001", 50m, token));
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(15, entorno.Base.Contador("00200001"));
            Assert.AreEqual(0, entorno.Base.De("00200001").Count);
        }

        [TestMethod]
        public void CadaDepositoUsaElContadorAnteriorMasUno()
        {
            S.RealizarDeposito("00200001", 1m, token);
            S.RealizarDeposito("00200001", 1m, token);
            S.RealizarDeposito("00200001", 1m, token);
            CollectionAssert.AreEqual(new[] { 3, 4, 5 }, entorno.Base.De("00200001").Select(m => m.Numero).ToArray());
        }

        [TestMethod]
        public void RecortaElCodigoDeCuenta()
        {
            Assert.AreEqual("200", S.RealizarDeposito(" 00200001 ", 5m, token).Codigo);
        }
    }

    [TestClass]
    public class RetiroTest : MovimientosBase
    {
        [TestMethod]
        public void Exito_RestaDelSaldoYRegistraElMovimiento004()
        {
            RespuestaMovimiento r = S.RealizarRetiro("00200002", 1000m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Retiro realizado correctamente", r.Mensaje);
            Assert.AreEqual(4000.00m, r.SaldoNuevo);
            Assert.AreEqual(4, r.Movimiento.Numero);
            Assert.AreEqual("004", r.Movimiento.TipoCodigo);
            Assert.AreEqual("SALIDA", r.Movimiento.TipoAccion);
            Assert.AreEqual(4000m, entorno.Base.Saldo("00200002"));
            Assert.AreEqual(0.00m, r.ItfAplicado);
        }

        [TestMethod]
        public void RetiroDelSaldoExacto_DejaCero()
        {
            RespuestaMovimiento r = S.RealizarRetiro("00200001", 100m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.SaldoNuevo);
        }

        [TestMethod]
        public void UnCentavoMasQueElSaldo_EsSal001_SinTocarLaBase()
        {
            RespuestaMovimiento r = S.RealizarRetiro("00200001", 100.01m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual("El saldo de la cuenta es insuficiente para la operación", r.Mensaje);
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            SinEscrituras();
        }

        [TestMethod]
        public void CargoQueNoAlcanza_SaldoJustoParaElMontoEsSal001()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaMovimiento r = S.RealizarRetiro("00200001", 100m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(15, entorno.Base.Contador("00200001"));
            Assert.AreEqual(0, entorno.Base.De("00200001").Count);
            SinEscrituras();
        }

        [TestMethod]
        public void SaldoExactoParaMontoMasCargo_Procede_YDejaCero()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaMovimiento r = S.RealizarRetiro("00200001", 98m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(2.00m, r.CostoAplicado);
            Assert.AreEqual(0.00m, r.SaldoNuevo);
            Assert.AreEqual(2, entorno.Base.De("00200001").Count);
            Assert.AreEqual("010", entorno.Base.De("00200001")[1].TipoCodigo);
        }

        [TestMethod]
        public void UnCentavoMenosParaMontoMasCargo_EsSal001()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            Assert.AreEqual("SAL-001", S.RealizarRetiro("00200001", 98.01m, token).Codigo);
        }

        [TestMethod]
        public void CargoEnElMovimientoQuince_NoHaceFaltaParaElSaldoExacto()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 14;
            RespuestaMovimiento r = S.RealizarRetiro("00200001", 100m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.CostoAplicado);
        }

        [TestMethod]
        public void Dentro_ElSaldoCambiaEntreLaLecturaYElBloqueo_EsSal001()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 500m;
            BaseEnMemoria b = entorno.Base;
            Entorno e = new Entorno(b);
            string t = e.Entrar();
            b.Cuentas["00200001"].Saldo = 500m;
            // simula que otra operación vació la cuenta justo antes del bloqueo
            ServicioMovimientos s = new ServicioMovimientos(new SaldoCambiante(b), b, b, b, b, b, b, e.Tokens, () => new DateTime(2026, 11, 3));
            Assert.AreEqual("SAL-001", s.RealizarRetiro("00200001", 400m, t).Codigo);
            Assert.AreEqual(50m, b.Saldo("00200001"));
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("-5")]
        [DataRow("10.555")]
        [DataRow("0.001")]
        public void MontosInvalidos_SonMon001(string monto)
        {
            Assert.AreEqual("MON-001", S.RealizarRetiro("00200001", D(monto), token).Codigo);
            SinEscrituras();
        }

        [TestMethod]
        public void MontoNulo_EsDat001()
        {
            Assert.AreEqual("DAT-001", S.RealizarRetiro("00200001", null, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("", "DAT-001")]
        [DataRow("11111111", "CTA-001")]
        [DataRow("00300001", "CTA-002")]
        public void Errores(string cuenta, string esperado)
        {
            Assert.AreEqual(esperado, S.RealizarRetiro(cuenta, 10m, token).Codigo);
        }

        [TestMethod]
        public void MonedaNoSoportada_EsMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("MON-002", S.RealizarRetiro("00200002", 10m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Sal001GanaSobreMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("SAL-001", S.RealizarRetiro("00200001", 1000m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Cta002GanaSobreSal001()
        {
            Assert.AreEqual("CTA-002", S.RealizarRetiro("00300001", 100000m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Mon001GanaSobreSal001()
        {
            Assert.AreEqual("MON-001", S.RealizarRetiro("00200001", 0m, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, S.RealizarRetiro("00200002", 10m, tokenMalo).Codigo);
        }

        [TestMethod]
        public void TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromHours(2));
            Assert.AreEqual("AUTH-004", S.RealizarRetiro("00200002", 10m, token).Codigo);
        }

        [TestMethod]
        public void FalloAlInsertar_DeshaceTodo()
        {
            entorno.Base.Cuentas["00200002"].OperacionesRealizadas = 15;
            entorno.Base.FallarAlInsertarMovimiento = true;
            entorno.Base.InsercionesHastaFallar = 2;
            Assert.ThrowsException<InvalidOperationException>(() => S.RealizarRetiro("00200002", 10m, token));
            Assert.AreEqual(5000m, entorno.Base.Saldo("00200002"));
            Assert.AreEqual(15, entorno.Base.Contador("00200002"));
            Assert.AreEqual(0, entorno.Base.De("00200002").Count);
        }

        [TestMethod]
        public void DosRetirosSeguidos_NumeranCorrelativo()
        {
            S.RealizarRetiro("00200002", 1m, token);
            S.RealizarRetiro("00200002", 1m, token);
            CollectionAssert.AreEqual(new[] { 4, 5 }, entorno.Base.De("00200002").Select(m => m.Numero).ToArray());
            Assert.AreEqual(4998m, entorno.Base.Saldo("00200002"));
        }

        private class SaldoCambiante : ec.edu.monster.datos.ICuentaDAO
        {
            private readonly BaseEnMemoria inner;

            public SaldoCambiante(BaseEnMemoria inner)
            {
                this.inner = inner;
            }

            public List<CuentaDTO> ListarPorCliente(string codigoCliente) { return inner.ListarPorCliente(codigoCliente); }

            // la primera lectura (sin bloqueo) ve saldo suficiente; el bloqueo ve la cuenta ya vaciada
            public CuentaDTO BuscarPorCodigo(string codigo)
            {
                CuentaDTO c = inner.BuscarPorCodigo(codigo);
                inner.Cuentas[codigo].Saldo = 50m;
                return c;
            }

            public CuentaDTO BloquearPorCodigo(ec.edu.monster.datos.ContextoTransaccional contexto, string codigo)
            {
                return inner.BloquearPorCodigo(contexto, codigo);
            }

            public void ActualizarSaldoYContador(ec.edu.monster.datos.ContextoTransaccional contexto, string codigo, decimal saldo, int contador)
            {
                inner.ActualizarSaldoYContador(contexto, codigo, saldo, contador);
            }
        }
    }

    [TestClass]
    public class TransferenciaTest : MovimientosBase
    {
        [TestMethod]
        public void Exito_MueveElDineroYCobraItf()
        {
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00200001", 1000m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual("Transferencia realizada correctamente", r.Mensaje);
            Assert.AreEqual(0.80m, r.ItfAplicado);
            Assert.AreEqual(3999.20m, r.SaldoOrigen);
            Assert.AreEqual(1100.00m, r.SaldoDestino);
            Assert.AreEqual(4, r.NumeroOrigen);
            Assert.AreEqual(3, r.NumeroDestino);
            Assert.AreEqual(3999.20m, entorno.Base.Saldo("00200002"));
            Assert.AreEqual(1100m, entorno.Base.Saldo("00200001"));
        }

        [TestMethod]
        public void RegistraLosMovimientos009_007_008ConSusReferencias()
        {
            S.RealizarTransferencia("00200002", "00200001", 1000m, token);
            List<MovimientoDTO> origen = entorno.Base.De("00200002");
            List<MovimientoDTO> destino = entorno.Base.De("00200001");
            Assert.AreEqual(2, origen.Count);
            Assert.AreEqual("009", origen[0].TipoCodigo);
            Assert.AreEqual("00200001", origen[0].Referencia);
            Assert.AreEqual(1000.00m, origen[0].Importe);
            Assert.AreEqual("007", origen[1].TipoCodigo);
            Assert.AreEqual(0.80m, origen[1].Importe);
            Assert.AreEqual(1, destino.Count);
            Assert.AreEqual("008", destino[0].TipoCodigo);
            Assert.AreEqual("00200002", destino[0].Referencia);
            Assert.AreEqual("INGRESO", destino[0].TipoAccion);
            Assert.AreEqual(5, entorno.Base.Contador("00200002"));
            Assert.AreEqual(3, entorno.Base.Contador("00200001"));
        }

        [TestMethod]
        [DataRow("100", "0.08")]
        [DataRow("1", "0.00")]
        [DataRow("6.25", "0.01")]
        [DataRow("6.24", "0.00")]
        [DataRow("500", "0.40")]
        [DataRow("1234.56", "0.99")]
        public void Itf_EsMontoPorOchoCentesimasDeUnPorCiento(string monto, string itf)
        {
            entorno.Base.Cuentas["00200002"].Saldo = 100000m;
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00200001", D(monto), token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(D(itf), r.ItfAplicado);
        }

        [TestMethod]
        public void ItfCero_NoRegistraMovimiento007()
        {
            S.RealizarTransferencia("00200002", "00200001", 1m, token);
            Assert.AreEqual(1, entorno.Base.De("00200002").Count);
            Assert.AreEqual(4, entorno.Base.Contador("00200002"));
        }

        [TestMethod]
        public void SaldoExactoParaMontoMasItf_Procede_YDejaCero()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 100.08m;
            RespuestaTransferencia r = S.RealizarTransferencia("00200001", "00200002", 100m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.SaldoOrigen);
        }

        [TestMethod]
        public void SaldoQueAlcanzaParaElMontoPeroNoParaElItf_EsSal001()
        {
            RespuestaTransferencia r = S.RealizarTransferencia("00200001", "00200002", 100m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(5000m, entorno.Base.Saldo("00200002"));
            SinEscrituras();
        }

        [TestMethod]
        public void UnCentavoMenosParaMontoMasItf_EsSal001()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 100.07m;
            Assert.AreEqual("SAL-001", S.RealizarTransferencia("00200001", "00200002", 100m, token).Codigo);
        }

        [TestMethod]
        public void SaldoParaMontoItfYCargo_Procede()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 102.08m;
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaTransferencia r = S.RealizarTransferencia("00200001", "00200002", 100m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(0.00m, r.SaldoOrigen);
            Assert.AreEqual(3, entorno.Base.De("00200001").Count);
            CollectionAssert.AreEqual(new[] { "009", "007", "010" }, entorno.Base.De("00200001").Select(m => m.TipoCodigo).ToArray());
            Assert.AreEqual(18, entorno.Base.Contador("00200001"));
        }

        [TestMethod]
        public void SaldoParaMontoEItfPeroNoParaElCargo_EsSal001()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 100.08m;
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaTransferencia r = S.RealizarTransferencia("00200001", "00200002", 100m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual(100.08m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(15, entorno.Base.Contador("00200001"));
            Assert.AreEqual(0, entorno.Base.De("00200001").Count);
        }

        [TestMethod]
        public void CargoEnLaCuentaDestino_SeDescuentaDeSuSaldo()
        {
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00200001", 50m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(148.00m, r.SaldoDestino);
            Assert.AreEqual(16, r.NumeroDestino);
            Assert.AreEqual(2, entorno.Base.De("00200001").Count);
            Assert.AreEqual(17, entorno.Base.Contador("00200001"));
        }

        [TestMethod]
        public void CargoMayorQueElSaldoDelDestino_EsSal001_SinDejarEscrituraParcial()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 0m;
            entorno.Base.Cuentas["00200001"].OperacionesRealizadas = 15;
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00200001", 1m, token);
            Assert.AreEqual("SAL-001", r.Codigo);
            Assert.AreEqual(5000m, entorno.Base.Saldo("00200002"));
            Assert.AreEqual(3, entorno.Base.Contador("00200002"));
            Assert.AreEqual(0, entorno.Base.De("00200002").Count);
            Assert.AreEqual(0, entorno.Base.De("00200001").Count);
        }

        [TestMethod]
        public void MismaCuenta_EsCta003()
        {
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00200002", 10m, token);
            Assert.AreEqual("CTA-003", r.Codigo);
            Assert.AreEqual("La cuenta origen y la cuenta destino no pueden ser iguales", r.Mensaje);
        }

        [TestMethod]
        public void MismaCuentaConEspacios_EsCta003()
        {
            Assert.AreEqual("CTA-003", S.RealizarTransferencia(" 00200002", "00200002 ", 10m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Cta003GanaSobreCta001()
        {
            Assert.AreEqual("CTA-003", S.RealizarTransferencia("11111111", "11111111", 10m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Mon001GanaSobreCta003()
        {
            Assert.AreEqual("MON-001", S.RealizarTransferencia("00200002", "00200002", 0m, token).Codigo);
        }

        [TestMethod]
        public void OrigenInexistente_EsCta001()
        {
            Assert.AreEqual("CTA-001", S.RealizarTransferencia("11111111", "00200001", 10m, token).Codigo);
        }

        [TestMethod]
        public void DestinoInexistente_EsCta001()
        {
            Assert.AreEqual("CTA-001", S.RealizarTransferencia("00200002", "11111111", 10m, token).Codigo);
        }

        [TestMethod]
        public void OrigenCancelado_EsCta002()
        {
            Assert.AreEqual("CTA-002", S.RealizarTransferencia("00300001", "00200001", 10m, token).Codigo);
        }

        [TestMethod]
        public void DestinoCancelado_EsCta002()
        {
            Assert.AreEqual("CTA-002", S.RealizarTransferencia("00200002", "00300001", 10m, token).Codigo);
        }

        [TestMethod]
        public void MonedaDistinta_EsCta004()
        {
            RespuestaTransferencia r = S.RealizarTransferencia("00200002", "00100002", 10m, token);
            Assert.AreEqual("CTA-004", r.Codigo);
            Assert.AreEqual("La cuenta de destino pertenece a una moneda distinta", r.Mensaje);
            SinEscrituras();
        }

        [TestMethod]
        public void Orden_Cta002GanaSobreCta004()
        {
            entorno.Base.Cuentas["00100002"].Estado = "ANULADO";
            Assert.AreEqual("CTA-002", S.RealizarTransferencia("00200002", "00100002", 10m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Cta004GanaSobreSal001()
        {
            Assert.AreEqual("CTA-004", S.RealizarTransferencia("00200001", "00100002", 100000m, token).Codigo);
        }

        [TestMethod]
        public void Orden_Sal001GanaSobreMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("SAL-001", S.RealizarTransferencia("00200001", "00200002", 100000m, token).Codigo);
        }

        [TestMethod]
        public void MonedaNoSoportada_EsMon002()
        {
            entorno.Base.MonedasSoportadas.Remove("01");
            Assert.AreEqual("MON-002", S.RealizarTransferencia("00200002", "00200001", 10m, token).Codigo);
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("-1")]
        [DataRow("10.555")]
        [DataRow("0.001")]
        public void MontosInvalidos_SonMon001(string monto)
        {
            Assert.AreEqual("MON-001", S.RealizarTransferencia("00200002", "00200001", D(monto), token).Codigo);
            SinEscrituras();
        }

        [TestMethod]
        public void MontoNulo_EsDat001()
        {
            Assert.AreEqual("DAT-001", S.RealizarTransferencia("00200002", "00200001", null, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "00200001")]
        [DataRow("00200002", null)]
        [DataRow("", "00200001")]
        [DataRow("00200002", " ")]
        public void CuentaVacia_EsDat001(string origen, string destino)
        {
            Assert.AreEqual("DAT-001", S.RealizarTransferencia(origen, destino, 10m, token).Codigo);
        }

        [TestMethod]
        [DataRow(null, "DAT-001")]
        [DataRow("falso", "AUTH-003")]
        public void TokenInvalido(string tokenMalo, string esperado)
        {
            Assert.AreEqual(esperado, S.RealizarTransferencia("00200002", "00200001", 10m, tokenMalo).Codigo);
        }

        [TestMethod]
        public void TokenVencido_EsAuth004()
        {
            entorno.Reloj.Avanzar(TimeSpan.FromMinutes(30));
            Assert.AreEqual("AUTH-004", S.RealizarTransferencia("00200002", "00200001", 10m, token).Codigo);
        }

        [TestMethod]
        public void FalloAlInsertarElMovimientoDelDestino_DeshaceElOrigen()
        {
            entorno.Base.FallarAlInsertarMovimiento = true;
            entorno.Base.InsercionesHastaFallar = 3;
            Assert.ThrowsException<InvalidOperationException>(() => S.RealizarTransferencia("00200002", "00200001", 1000m, token));
            Assert.AreEqual(5000m, entorno.Base.Saldo("00200002"));
            Assert.AreEqual(100m, entorno.Base.Saldo("00200001"));
            Assert.AreEqual(0, entorno.Base.De("00200002").Count);
            Assert.AreEqual(3, entorno.Base.Contador("00200002"));
        }

        [TestMethod]
        public void BloqueaLasCuentasEnOrdenDeCodigo_SinImportarElSentido()
        {
            RecordadorDeBloqueos registro = new RecordadorDeBloqueos(entorno.Base);
            ServicioMovimientos s = new ServicioMovimientos(entorno.Base, entorno.Base, entorno.Base, entorno.Base,
                entorno.Base, entorno.Base, entorno.Base, entorno.Tokens, () => new DateTime(2026, 11, 3));
            ServicioMovimientos s2 = new ServicioMovimientos(registro, entorno.Base, entorno.Base, entorno.Base,
                entorno.Base, entorno.Base, entorno.Base, entorno.Tokens, () => new DateTime(2026, 11, 3));
            s2.RealizarTransferencia("00200002", "00200001", 10m, token);
            s2.RealizarTransferencia("00200001", "00200002", 10m, token);
            CollectionAssert.AreEqual(new[] { "00200001", "00200002", "00200001", "00200002" }, registro.Orden);
            Assert.IsNotNull(s);
        }

        [TestMethod]
        public void SeTransfiereEnDolares()
        {
            entorno.Base.AgregarCuenta("00100003", "02", "Dolares", 10m, "ACTIVO", 0);
            RespuestaTransferencia r = S.RealizarTransferencia("00100002", "00100003", 100m, token);
            Assert.AreEqual("200", r.Codigo);
            Assert.AreEqual(110.00m, r.SaldoDestino);
            Assert.AreEqual(699.92m, r.SaldoOrigen);
        }

        [TestMethod]
        public void TransferenciasCruzadasSimultaneas_NoSeBloqueanNiPierdenDinero()
        {
            entorno.Base.Cuentas["00200002"].Saldo = 100000m;
            entorno.Base.Cuentas["00200001"].Saldo = 100000m;
            decimal antes = entorno.Base.Saldo("00200002") + entorno.Base.Saldo("00200001");
            int itfTotal = 0;
            Task[] tareas = Enumerable.Range(0, 40).Select(i => Task.Run(() =>
            {
                RespuestaTransferencia r = i % 2 == 0
                    ? S.RealizarTransferencia("00200002", "00200001", 100m, token)
                    : S.RealizarTransferencia("00200001", "00200002", 100m, token);
                Assert.AreEqual("200", r.Codigo);
                System.Threading.Interlocked.Increment(ref itfTotal);
            })).ToArray();
            Assert.IsTrue(Task.WaitAll(tareas, TimeSpan.FromSeconds(30)), "posible bloqueo cruzado");
            decimal despues = entorno.Base.Saldo("00200002") + entorno.Base.Saldo("00200001");
            decimal cargos = entorno.Base.De("00200002").Concat(entorno.Base.De("00200001"))
                .Where(m => m.TipoCodigo == "010").Sum(m => m.Importe);
            Assert.AreEqual(antes - 40 * 0.08m - cargos, despues);
        }

        private class RecordadorDeBloqueos : ec.edu.monster.datos.ICuentaDAO
        {
            private readonly BaseEnMemoria inner;
            public readonly List<string> Orden = new List<string>();

            public RecordadorDeBloqueos(BaseEnMemoria inner)
            {
                this.inner = inner;
            }

            public List<CuentaDTO> ListarPorCliente(string c) { return inner.ListarPorCliente(c); }
            public CuentaDTO BuscarPorCodigo(string c) { return inner.BuscarPorCodigo(c); }

            public CuentaDTO BloquearPorCodigo(ec.edu.monster.datos.ContextoTransaccional contexto, string codigo)
            {
                Orden.Add(codigo);
                return inner.BloquearPorCodigo(contexto, codigo);
            }

            public void ActualizarSaldoYContador(ec.edu.monster.datos.ContextoTransaccional contexto, string codigo, decimal saldo, int contador)
            {
                inner.ActualizarSaldoYContador(contexto, codigo, saldo, contador);
            }
        }
    }

    [TestClass]
    public class ConcurrenciaTest : MovimientosBase
    {
        [TestMethod]
        public void DosRetirosSimultaneosPorElSaldoCompleto_SoloUnoProcede()
        {
            Task<RespuestaMovimiento>[] tareas =
            {
                Task.Run(() => S.RealizarRetiro("00200001", 100m, token)),
                Task.Run(() => S.RealizarRetiro("00200001", 100m, token))
            };
            Task.WaitAll(tareas);
            List<string> codigos = tareas.Select(t => t.Result.Codigo).OrderBy(c => c).ToList();
            CollectionAssert.AreEqual(new[] { "200", "SAL-001" }, codigos);
            Assert.AreEqual(0m, entorno.Base.Saldo("00200001"));
        }

        [TestMethod]
        public void CincuentaRetirosSimultaneos_NuncaDejanSaldoNegativo()
        {
            entorno.Base.Cuentas["00200001"].Saldo = 1000m;
            Task<RespuestaMovimiento>[] tareas = Enumerable.Range(0, 50)
                .Select(i => Task.Run(() => S.RealizarRetiro("00200001", 100m, token))).ToArray();
            Task.WaitAll(tareas);
            int exitos = tareas.Count(t => t.Result.Codigo == "200");
            int sinSaldo = tareas.Count(t => t.Result.Codigo == "SAL-001");
            Assert.IsTrue(entorno.Base.Saldo("00200001") >= 0m);
            Assert.IsTrue(exitos >= 8 && exitos <= 10, "exitos=" + exitos);
            Assert.AreEqual(50, exitos + sinSaldo);
            Assert.AreEqual(1000m - exitos * 100m - CargosDe("00200001"), entorno.Base.Saldo("00200001"));
        }

        [TestMethod]
        public void DepositosSimultaneos_NoPierdenActualizaciones()
        {
            Task[] tareas = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
                Assert.AreEqual("200", S.RealizarDeposito("00200002", 1m, token).Codigo))).ToArray();
            Task.WaitAll(tareas);
            Assert.AreEqual(5100m - CargosDe("00200002"), entorno.Base.Saldo("00200002"));
            Assert.AreEqual(3 + 100 + (int)(CargosDe("00200002") / 2m), entorno.Base.Contador("00200002"));
        }

        [TestMethod]
        public void LosNumerosDeMovimientoNuncaSeRepiten()
        {
            Task.WaitAll(Enumerable.Range(0, 60).Select(i => Task.Run(() => S.RealizarDeposito("00200002", 1m, token))).ToArray());
            List<int> numeros = entorno.Base.De("00200002").Select(m => m.Numero).ToList();
            Assert.AreEqual(numeros.Count, numeros.Distinct().Count());
        }

        private decimal CargosDe(string cuenta)
        {
            return entorno.Base.De(cuenta).Where(m => m.TipoCodigo == "010").Sum(m => m.Importe);
        }
    }
}
