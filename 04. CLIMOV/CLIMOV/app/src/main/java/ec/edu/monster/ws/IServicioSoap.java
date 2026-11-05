package ec.edu.monster.ws;

import ec.edu.monster.modelo.RespuestaSoap;
import java.util.LinkedHashMap;

/** Puerto del transporte SOAP; permite probar el controlador sin red. */
public interface IServicioSoap {
    RespuestaSoap llamar(String operacion, LinkedHashMap<String, String> parametros) throws Exception;
}
