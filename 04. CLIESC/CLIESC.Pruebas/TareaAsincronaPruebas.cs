using System;
using System.Threading;
using ec.edu.monster.escritorio.vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    [TestClass]
    public class TareaAsincronaPruebas
    {
        [TestMethod]
        public void Ejecutar_TrabajoNoCorreEnElHiloQueInvoca()
        {
            int hiloInvocador = Thread.CurrentThread.ManagedThreadId;
            int hiloTrabajo = -1;
            bool terminado = false;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(
                () => { hiloTrabajo = Thread.CurrentThread.ManagedThreadId; return 1; },
                r => terminado = true,
                null,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => terminado));
            Assert.AreNotEqual(hiloInvocador, hiloTrabajo, "El trabajo corrió en el hilo de la interfaz.");
        }

        [TestMethod]
        public void Ejecutar_ElCallbackDeExitoVuelveAlContexto()
        {
            int hiloPrueba = Thread.CurrentThread.ManagedThreadId;
            int hiloCallback = -1;
            int valor = 0;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(
                () => 42,
                r =>
                {
                    valor = r;
                    hiloCallback = Thread.CurrentThread.ManagedThreadId;
                },
                null,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => valor == 42));
            Assert.AreEqual(hiloPrueba, hiloCallback, "El callback no volvió al hilo del contexto.");
        }

        [TestMethod]
        public void Ejecutar_ElCallbackNoCorreHastaQueElContextoLoBombea()
        {
            bool llego = false;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(() => 7, r => llego = true, null, contexto);

            Assert.IsTrue(SpinHasta(() => contexto.Publicaciones == 1), "El resultado nunca se publicó.");
            Assert.IsFalse(llego, "El callback corrió sin pasar por el contexto.");
            Assert.AreEqual(1, contexto.Pendientes);

            contexto.Bombear();
            Assert.IsTrue(llego);
        }

        [TestMethod]
        public void Ejecutar_ExcepcionVaAlCallbackDeErrorYNoAlDeExito()
        {
            Exception recibida = null;
            bool exito = false;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar<int>(
                () => throw new InvalidOperationException("fallo controlado"),
                r => exito = true,
                ex => recibida = ex,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => recibida != null));
            Assert.IsFalse(exito);
            Assert.IsInstanceOfType(recibida, typeof(InvalidOperationException));
            Assert.AreEqual("fallo controlado", recibida.Message);
        }

        [TestMethod]
        public void Ejecutar_ExcepcionSinCallbackDeErrorNoLanzaNada()
        {
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar<int>(
                () => throw new TimeoutException("sin respuesta"),
                r => { },
                null,
                contexto);

            Assert.IsTrue(SpinHasta(() => contexto.Publicaciones == 1));
            contexto.Bombear();
            Assert.AreEqual(0, contexto.Pendientes);
        }

        [TestMethod]
        public void Ejecutar_TrabajoNuloLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                TareaAsincrona.Ejecutar<int>(null, r => { }, null, new ContextoManual()));
        }

        [TestMethod]
        public void Ejecutar_AlExitoNuloLanzaArgumentNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
                TareaAsincrona.Ejecutar<int>(() => 1, null, null, new ContextoManual()));
        }

        [TestMethod]
        public void Ejecutar_ElTrabajoSeEjecutaUnaSolaVez()
        {
            int ejecuciones = 0;
            bool terminado = false;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(
                () => Interlocked.Increment(ref ejecuciones),
                r => terminado = true,
                null,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => terminado));
            Thread.Sleep(20);
            contexto.Bombear();
            Assert.AreEqual(1, ejecuciones);
        }

        [TestMethod]
        public void Ejecutar_VariasTareasEntreganCadaResultadoASuPropioCallback()
        {
            int[] resultados = new int[5];
            int terminadas = 0;
            var contexto = new ContextoManual();

            for (int i = 0; i < resultados.Length; i++)
            {
                int indice = i;
                TareaAsincrona.Ejecutar(
                    () => indice * 10,
                    r =>
                    {
                        resultados[indice] = r;
                        terminadas++;
                    },
                    null,
                    contexto);
            }

            Assert.IsTrue(contexto.Esperar(() => terminadas == resultados.Length));
            for (int i = 0; i < resultados.Length; i++)
            {
                Assert.AreEqual(i * 10, resultados[i]);
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-7)]
        [DataRow(250)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Ejecutar_EntregaElResultadoIntactoAlCallback(int esperado)
        {
            int recibido = 0;
            bool llego = false;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(
                () => esperado,
                r =>
                {
                    recibido = r;
                    llego = true;
                },
                null,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => llego));
            Assert.AreEqual(esperado, recibido);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("200")]
        [DataRow("[AUTH-003] El token de sesión no es válido")]
        [DataRow("ñandú")]
        public void Ejecutar_EntregaTextosSinCambiarlos(string esperado)
        {
            string recibido = null;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar(() => esperado, r => recibido = r, null, contexto);

            Assert.IsTrue(contexto.Esperar(() => recibido != null));
            Assert.AreEqual(esperado, recibido);
        }

        [TestMethod]
        [DataRow("Fallo de red")]
        [DataRow("Tiempo agotado")]
        [DataRow("Respuesta inválida")]
        public void Ejecutar_ExcepcionEntregaSuMensajeAlCallbackDeError(string mensaje)
        {
            string recibido = null;
            var contexto = new ContextoManual();

            TareaAsincrona.Ejecutar<int>(
                () => throw new InvalidOperationException(mensaje),
                r => { },
                ex => recibido = ex.Message,
                contexto);

            Assert.IsTrue(contexto.Esperar(() => recibido != null));
            Assert.AreEqual(mensaje, recibido);
        }

        private static bool SpinHasta(Func<bool> condicion)
        {
            var limite = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < limite)
            {
                if (condicion())
                {
                    return true;
                }
                Thread.Sleep(2);
            }
            return condicion();
        }
    }
}
