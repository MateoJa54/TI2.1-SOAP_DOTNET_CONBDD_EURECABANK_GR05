using System;
using ec.edu.monster.escritorio.modelo;
using ec.edu.monster.escritorio.vista;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ec.edu.monster.escritorio.pruebas
{
    [TestClass]
    public class NavegacionPruebas
    {
        [TestMethod]
        public void Inicio_MuestraElAccesoSinMensaje()
        {
            var navegacion = new Navegacion();
            Assert.AreEqual(Pantalla.Acceso, navegacion.Actual);
            Assert.AreEqual(string.Empty, navegacion.MensajeAcceso);
            Assert.IsFalse(navegacion.MensajeEsError);
        }

        [TestMethod]
        public void IrAPrincipal_CambiaLaPantallaYAvisaUnaVez()
        {
            var navegacion = new Navegacion();
            int avisos = 0;
            navegacion.Cambio += (s, e) => avisos++;

            navegacion.IrAPrincipal();

            Assert.AreEqual(Pantalla.Principal, navegacion.Actual);
            Assert.AreEqual(1, avisos);
        }

        [TestMethod]
        public void IrAPrincipal_LimpiaElMensajeDeAcceso()
        {
            var navegacion = new Navegacion();
            navegacion.IrAAcceso("algo", esError: true);

            navegacion.IrAPrincipal();

            Assert.AreEqual(string.Empty, navegacion.MensajeAcceso);
            Assert.IsFalse(navegacion.MensajeEsError);
        }

        [TestMethod]
        [DataRow("Sesión cerrada correctamente", false)]
        [DataRow("[AUTH-004] Sesión expirada", true)]
        [DataRow("", false)]
        public void IrAAcceso_GuardaMensajeYTipo(string mensaje, bool esError)
        {
            var navegacion = new Navegacion();
            navegacion.IrAPrincipal();

            navegacion.IrAAcceso(mensaje, esError);

            Assert.AreEqual(Pantalla.Acceso, navegacion.Actual);
            Assert.AreEqual(mensaje, navegacion.MensajeAcceso);
            Assert.AreEqual(esError, navegacion.MensajeEsError);
        }

        [TestMethod]
        public void IrAAcceso_MensajeNuloSeGuardaVacio()
        {
            var navegacion = new Navegacion();
            navegacion.IrAAcceso(null);
            Assert.AreEqual(string.Empty, navegacion.MensajeAcceso);
        }

        [TestMethod]
        [DataRow("AUTH-003", "El token de sesión no es válido")]
        [DataRow("AUTH-004", "La sesión expiró")]
        public void RevisarRespuesta_AuthInvalidaVuelveAlAccesoConElMensaje(string codigo, string mensaje)
        {
            var navegacion = new Navegacion();
            navegacion.IrAPrincipal();

            bool redirigido = navegacion.RevisarRespuesta(new RespuestaBase("ERROR", codigo, mensaje));

            Assert.IsTrue(redirigido);
            Assert.AreEqual(Pantalla.Acceso, navegacion.Actual);
            Assert.AreEqual("[" + codigo + "] " + mensaje, navegacion.MensajeAcceso);
            Assert.IsTrue(navegacion.MensajeEsError);
        }

        [TestMethod]
        [DataRow("AUTH-001")]
        [DataRow("AUTH-002")]
        [DataRow("SAL-001")]
        [DataRow("SER-001")]
        [DataRow("MON-001")]
        [DataRow("200")]
        public void RevisarRespuesta_OtrosCodigosNoMuevenLaPantalla(string codigo)
        {
            var navegacion = new Navegacion();
            navegacion.IrAPrincipal();

            bool redirigido = navegacion.RevisarRespuesta(new RespuestaBase("ERROR", codigo, "x"));

            Assert.IsFalse(redirigido);
            Assert.AreEqual(Pantalla.Principal, navegacion.Actual);
        }

        [TestMethod]
        public void RevisarRespuesta_NulaNoMueveLaPantalla()
        {
            var navegacion = new Navegacion();
            navegacion.IrAPrincipal();
            Assert.IsFalse(navegacion.RevisarRespuesta(null));
            Assert.AreEqual(Pantalla.Principal, navegacion.Actual);
        }

        [TestMethod]
        public void RevisarRespuesta_AuthAvisaUnaVezPorRespuesta()
        {
            var navegacion = new Navegacion();
            navegacion.IrAPrincipal();
            int avisos = 0;
            navegacion.Cambio += (s, e) => avisos++;

            navegacion.RevisarRespuesta(new RespuestaBase("ERROR", "AUTH-003", "x"));

            Assert.AreEqual(1, avisos);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        public void Cambio_AvisaCadaVezQueSeNavega(int veces)
        {
            var navegacion = new Navegacion();
            int avisos = 0;
            navegacion.Cambio += (s, e) => avisos++;

            for (int i = 0; i < veces; i++)
            {
                navegacion.IrAPrincipal();
                navegacion.IrAAcceso("x");
            }

            Assert.AreEqual(veces * 2, avisos);
        }
    }
}
