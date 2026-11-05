package ec.edu.monster.pruebas;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

import ec.edu.monster.modelo.RespuestaSoap;
import ec.edu.monster.ws.SoapXml;
import java.util.LinkedHashMap;
import org.junit.Test;

public class SoapXmlTest {
    @Test public void construyeSobreConParametrosEnOrdenYEscapaXml() {
        LinkedHashMap<String, String> args = new LinkedHashMap<>();
        args.put("cuenta", "A&<1"); args.put("token", "t-1");
        String xml = SoapXml.envelope("consultarCuenta", args);
        assertTrue(xml.contains("<cuenta>A&amp;&lt;1</cuenta><token>t-1</token>"));
        assertTrue(xml.contains("<tns:consultarCuenta xmlns:tns=\"http://ws.monster.edu.ec/\">"));
    }

    @Test public void analizaSobreBaseListasYElementosAnidados() throws Exception {
        String xml = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:x=\"urn:test\">"
                + "<s:Body><x:consultarMovimientosResponse><return><codigo>200</codigo><estado>OK</estado>"
                + "<mensaje>Correcto</mensaje><movimientos><numero>1</numero></movimientos>"
                + "<movimientos><numero>2</numero></movimientos></return></x:consultarMovimientosResponse></s:Body></s:Envelope>";
        RespuestaSoap response = SoapXml.parse(xml);
        assertEquals("200", response.getCodigo());
        assertEquals("OK", response.getEstado());
        assertEquals("Correcto", response.getMensaje());
        assertEquals(2, ((java.util.List<?>) response.getDatos().get("movimientos")).size());
    }

    @Test(expected = IllegalArgumentException.class)
    public void rechazaDeclaracionesDtd() throws Exception {
        SoapXml.parse("<!DOCTYPE x [<!ENTITY e SYSTEM 'file:///secreto'>]><Envelope>&e;</Envelope>");
    }
}
