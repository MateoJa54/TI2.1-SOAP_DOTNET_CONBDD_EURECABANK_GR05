package ec.edu.monster.modelo;

import java.util.LinkedHashMap;
import java.util.Map;

/** Respuesta SOAP con el sobre estándar y los campos del retorno. */
public final class RespuestaSoap {
    private final String codigo;
    private final String estado;
    private final String mensaje;
    private final Map<String, Object> datos;

    public RespuestaSoap(String codigo, String estado, String mensaje, Map<String, Object> datos) {
        this.codigo = codigo;
        this.estado = estado;
        this.mensaje = mensaje;
        this.datos = new LinkedHashMap<>(datos);
    }

    public String getCodigo() { return codigo; }
    public String getEstado() { return estado; }
    public String getMensaje() { return mensaje; }
    public Map<String, Object> getDatos() { return new LinkedHashMap<>(datos); }
}
