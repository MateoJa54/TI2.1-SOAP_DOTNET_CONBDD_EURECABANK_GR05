package ec.edu.monster.pruebas;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;

import ec.edu.monster.controlador.ControladorBanco;
import ec.edu.monster.modelo.RespuestaSoap;
import ec.edu.monster.ws.IServicioSoap;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import org.junit.Test;

/** Verifica los nombres y el orden del contrato en las llamadas del controlador. */
public class ControladorBancoTest {
    private static final String TOKEN = "token-prueba";

    @Test public void opcionesDeServicioNoIncluyenCerrarSesion() {
        assertEquals(7, ControladorBanco.OPERACIONES.length);
        for (String operation : ControladorBanco.OPERACIONES)
            assertFalse("Cerrar sesión".equalsIgnoreCase(operation));
    }

    @Test public void autenticaConSha256() throws Exception {
        FakeSoap fake = new FakeSoap(ok(fields("token", TOKEN)));
        ControladorBanco controller = new ControladorBanco(fake);
        controller.autenticar("cromero", "Cromero2026");
        assertEquals("autenticar", fake.operation);
        assertEquals(list("usuario", "clave"), new ArrayList<>(fake.args.keySet()));
        assertEquals("cromero", fake.args.get("usuario"));
        assertEquals("891337e9a11b6a3a19872e32f6318a2c24e5060f0d2652e716bf038fbd8c68d4", fake.args.get("clave"));
        assertTrue(controller.haySesion());
    }

    @Test public void cierraSesionConToken() throws Exception {
        TestContext c = authenticated(); c.fake.response = ok(fields());
        c.controller.cerrarSesion();
        assertCall(c.fake, "cerrarSesion", "token");
        assertFalse(c.controller.haySesion());
    }

    @Test public void listaCuentasConCedulaYTokenAlFinal() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(0, "06914897");
        assertCall(c.fake, "listarCuentasPorCliente", "cedula", "token");
        assertEquals("06914897", c.fake.args.get("cedula"));
    }

    @Test public void consultaCuentaConToken() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(1, "00200002");
        assertCall(c.fake, "consultarCuenta", "cuenta", "token");
    }

    @Test public void consultaMovimientosConToken() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(2, "00200002");
        assertCall(c.fake, "consultarMovimientos", "cuenta", "token");
    }

    @Test public void depositaMontoDecimalYRespetaFormatoAceptadoPorConsola() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(3, "00200002", "12,5");
        assertCall(c.fake, "realizarDeposito", "cuenta", "monto", "token");
        assertEquals("12.5", c.fake.args.get("monto"));
    }

    @Test public void retiraMontoDecimalConDosPosiciones() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(4, "00200002", "3.25");
        assertCall(c.fake, "realizarRetiro", "cuenta", "monto", "token");
        assertEquals("3.25", c.fake.args.get("monto"));
    }

    @Test public void transfiereConCuentasMontoYTokenEnOrden() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(5, "00200002", "00200003", "8");
        assertCall(c.fake, "realizarTransferencia", "cuentaOrigen", "cuentaDestino", "monto", "token");
        assertEquals("8", c.fake.args.get("monto"));
    }

    @Test public void enviaMontoConMasDeDosDecimalesParaQueLoValideServidor() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(3, "00200002", "10.555");
        assertEquals("10.555", c.fake.args.get("monto"));
    }

    @Test public void enviaCeroYNegativosParaValidacionDelServidor() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(4, "00200002", "-1");
        assertEquals("-1", c.fake.args.get("monto"));
    }

    @Test public void montoNoNumericoProduceCodigoDelContratoSinLlamarServicio() throws Exception {
        TestContext c = authenticated();
        try {
            c.controller.ejecutar(3, "00200002", "abc");
            org.junit.Assert.fail("Debe rechazar el texto no numérico");
        } catch (IllegalArgumentException error) {
            assertTrue(error.getMessage().startsWith("MON-001"));
        }
        assertEquals("autenticar", c.fake.operation);
    }

    @Test public void consultaCostosConToken() throws Exception {
        TestContext c = authenticated(); c.controller.ejecutar(6, "00200002");
        assertCall(c.fake, "consultarCostosMantenimiento", "cuenta", "token");
    }

    @Test public void invalidaSesionLocalSiServidorDevuelveAuth004() throws Exception {
        TestContext c = authenticated();
        c.fake.response = new RespuestaSoap("AUTH-004", "ERROR", "sesión expirada", fields());
        c.controller.ejecutar(1, "00200002");
        assertFalse(c.controller.haySesion());
    }

    private static void assertCall(FakeSoap fake, String operation, String... names) {
        assertEquals(operation, fake.operation);
        assertEquals(list(names), new ArrayList<>(fake.args.keySet()));
        assertEquals(TOKEN, fake.args.get("token"));
    }

    private static TestContext authenticated() throws Exception {
        FakeSoap fake = new FakeSoap(ok(fields("token", TOKEN)));
        ControladorBanco controller = new ControladorBanco(fake);
        controller.autenticar("cromero", "Cromero2026");
        return new TestContext(controller, fake);
    }

    private static List<String> list(String... values) {
        List<String> result = new ArrayList<>();
        java.util.Collections.addAll(result, values);
        return result;
    }
    private static Map<String, Object> fields(String... pairs) {
        Map<String, Object> result = new LinkedHashMap<>();
        for (int i = 0; i < pairs.length; i += 2) result.put(pairs[i], pairs[i + 1]);
        return result;
    }
    private static RespuestaSoap ok(Map<String, Object> fields) { return new RespuestaSoap("200", "OK", "Correcto", fields); }

    private static final class TestContext {
        final ControladorBanco controller; final FakeSoap fake;
        TestContext(ControladorBanco controller, FakeSoap fake) { this.controller = controller; this.fake = fake; }
    }
    private static final class FakeSoap implements IServicioSoap {
        RespuestaSoap response; String operation; LinkedHashMap<String, String> args;
        FakeSoap(RespuestaSoap response) { this.response = response; }
        @Override public RespuestaSoap llamar(String operation, LinkedHashMap<String, String> args) {
            this.operation = operation; this.args = new LinkedHashMap<>(args); return response;
        }
    }
}
